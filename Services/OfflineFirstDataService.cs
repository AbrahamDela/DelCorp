using DelCorp.Models;
using DelCorp.Models.Local;
using DelCorp.Models.Supabase;
using DelCorp.Services.Mapping;
using Supabase;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Postgrest.Responses;
using Supabase.Interfaces;

namespace DelCorp.Services
{
    public class OfflineFirstDataService : IDataService
    {
        private readonly Client _supabaseClient;
        private readonly LocalDatabaseService _localDatabase;
        private readonly IConnectivity _connectivity;
        private readonly ILogger<OfflineFirstDataService> _logger;
        private static readonly Random _randomGenerator = new Random();
        public OfflineFirstDataService(Supabase.Client supabaseClient, LocalDatabaseService localDatabase, IConnectivity connectivity, ILogger<OfflineFirstDataService> logger)
        {
            _supabaseClient = supabaseClient;
            _localDatabase = localDatabase;
            _connectivity = connectivity;
            _logger = logger;
        }

        // Método para obtener proyectos paginados
        public async Task<List<Project>> GetPagedProjects(int page, int pageSize)
        {
            // Obtener proyectos locales paginados
            var localProjects = await _localDatabase.GetLocalPagedProjects(page, pageSize);
            var dtoProjects = localProjects.ToDtoList();

            // Si hay conexión, intentar sincronizar y obtener mas proyectos
            if (_connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                try
                {
                    // Obtener proyectos del servidor con paginacion
                    var response = await _supabaseClient
                        .From<SupabaseProject>()
                        .Order(x => x.CreatedAt, Postgrest.Constants.Ordering.Descending)
                        .Range((page - 1) * pageSize, (page * pageSize) - 1)
                        .Get();

                    var remoteProjects = response.Models;

                    // Procesar y sincronizar proyectos remotos
                    foreach (var remoteProject in remoteProjects)
                    {
                        var dtoRemote = remoteProject.ToDto();
                        var localProject = await _localDatabase.GetProjectByServerIdAsync(remoteProject.Id);

                        if (localProject == null)
                        {
                            // Nuevo proyecto del servidor
                            var newLocalProject = dtoRemote.ToLocal();
                            newLocalProject.IsSynced = true;
                            await _localDatabase.SaveProjectAsync(newLocalProject);
                        }
                        else if (localProject.IsSynced)
                        {
                            // Actualizar proyecto existente
                            var updatedLocalProject = dtoRemote.ToLocal();
                            updatedLocalProject.Id = localProject.Id;
                            updatedLocalProject.ServerId = remoteProject.Id;
                            updatedLocalProject.IsSynced = true;
                            await _localDatabase.SaveProjectAsync(updatedLocalProject);
                        }
                    }

                    // Recargar proyectos locales
                    localProjects = await _localDatabase.GetLocalPagedProjects(page, pageSize);
                    dtoProjects = localProjects.ToDtoList();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error al sincronizar: {ex.Message}");
                }
            }

            return dtoProjects;
        }
        
        public async Task<List<Project>> GetProjects()
        {
            // Obtener proyectos locales
            var localProjects = await _localDatabase.GetProjectsAsync();
            var dtoProjects = localProjects.ToDtoList();

            // Si hay conexion, sincronizar con el servidor
            if (_connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                try
                {
                    // Primero sincronizar los cambios pendientes
                    await SyncProjects();

                    // Luego obtener datos del servidor
                    var response = await _supabaseClient.From<SupabaseProject>().Get();
                    var remoteProjects = response.Models.OrderByDescending(p => p.CreatedAt).ToList();

                    // Para cada proyecto remoto, actualizar la base local
                    foreach (var remoteProject in remoteProjects)
                    {
                        var dtoRemote = remoteProject.ToDto();
                        var localProject = await _localDatabase.GetProjectByServerIdAsync(remoteProject.Id);

                        if (localProject == null)
                        {
                            // Es un proyecto nuevo del servidor
                            var newLocalProject = dtoRemote.ToLocal();
                            newLocalProject.IsSynced = true;
                            await _localDatabase.SaveProjectAsync(newLocalProject);
                        }
                        else if (localProject.IsSynced)
                        {
                            // Actualizar solo si esta sincronizado (no tiene cambios locales pendientes)
                            var updatedLocalProject = dtoRemote.ToLocal();
                            updatedLocalProject.Id = localProject.Id; // Mantener el ID local
                            updatedLocalProject.ServerId = remoteProject.Id;
                            updatedLocalProject.IsSynced = true;
                            await _localDatabase.SaveProjectAsync(updatedLocalProject);
                        }
                    }

                    // Recargar datos locales
                    localProjects = await _localDatabase.GetProjectsAsync();
                    dtoProjects = localProjects.ToDtoList();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error al sincronizar: {ex.Message}");
                }
            }

            return dtoProjects.OrderByDescending(p => p.CreatedAt).ToList();
        }

        public async Task<Project> GetProject(int id)
        {
            // Primero buscamos por ServerId
            var localProject = await _localDatabase.GetProjectByServerIdAsync(id);

            // Si no se encuentra, puede ser un proyecto local nuevo con el id como ID local
            if (localProject == null)
            {
                localProject = await _localDatabase.GetProjectAsync(id);
            }

            return localProject?.ToDto();
        }

        public async Task<bool> SaveProject(Project project)
        {
            try
            {
                // Para proyectos nuevos, establecer fecha de creacion
                if (project.Id == 0)
                {
                    project.CreatedAt = DateTime.Now;
                }

                // Convertir a modelo local y guardar
                var localProject = project.ToLocal();

                // Si el proyecto tiene un ID pero no serverId, puede ser un proyecto nuevo local
                if (project.Id != 0 && localProject.ServerId == null)
                {
                    localProject.ServerId = project.Id;
                }

                await _localDatabase.SaveProjectAsync(localProject);

                // Si hay conexion, intentar sincronizar con el servidor
                if (_connectivity.NetworkAccess == NetworkAccess.Internet)
                {
                    try
                    {
                        // Convertir a modelo Supabase y enviar
                        var supabaseProject = project.ToSupabase();
                        var response = await _supabaseClient.From<SupabaseProject>().Upsert(supabaseProject);
                        var syncedProject = response.Models.FirstOrDefault();

                        if (syncedProject != null)
                        {
                            // Actualizar con el ID del servidor
                            localProject.ServerId = syncedProject.Id;
                            localProject.IsSynced = true;
                            await _localDatabase.SaveProjectAsync(localProject);
                        }
                    }
                    catch
                    {
                        // Error al sincronizar, marcar como pendiente
                        localProject.IsSynced = false;
                        await _localDatabase.SaveProjectAsync(localProject);
                    }
                }
                else
                {
                    // Sin conexion, marcar como pendiente
                    localProject.IsSynced = false;
                    await _localDatabase.SaveProjectAsync(localProject);
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al guardar: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteProject(int id)
        {
            try
            {
                // Buscar proyecto por ID de servidor o ID local
                var localProject = await _localDatabase.GetProjectByServerIdAsync(id);
                if (localProject == null)
                {
                    localProject = await _localDatabase.GetProjectAsync(id);
                }

                if (localProject != null)
                {
                    // Eliminar localmente
                    await _localDatabase.DeleteProjectAsync(localProject);

                    // Si tiene ID de servidor y hay conexion, eliminar en el servidor
                    if (localProject.ServerId.HasValue && _connectivity.NetworkAccess == NetworkAccess.Internet)
                    {
                        try
                        {
                            await _supabaseClient
                                .From<SupabaseProject>()
                                .Filter("id", Postgrest.Constants.Operator.Equals, localProject.ServerId.Value)
                                .Delete();

                        }
                        catch
                        {
                            // Error al eliminar en el servidor
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al eliminar: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SyncProjects()
        {
            if (_connectivity.NetworkAccess != NetworkAccess.Internet)
                return false;

            try
            {
                // Obtener proyectos no sincronizados
                var unsyncedProjects = await _localDatabase.GetUnsyncedProjectsAsync();

                foreach (var localProject in unsyncedProjects)
                {
                    try
                    {
                        // Convertir a DTO y luego a modelo Supabase
                        var dtoProject = localProject.ToDto();
                        var supabaseProject = dtoProject.ToSupabase();

                        // Enviar al servidor
                        var response = await _supabaseClient.From<SupabaseProject>().Upsert(supabaseProject);
                        var syncedProject = response.Models.FirstOrDefault();

                        if (syncedProject != null)
                        {
                            // Actualizar con el ID del servidor y marcar como sincronizado
                            localProject.ServerId = syncedProject.Id;
                            localProject.IsSynced = true;
                            await _localDatabase.SaveProjectAsync(localProject);
                        }
                    }
                    catch
                    {
                        // Error al sincronizar este proyecto, continuamos con el siguiente
                        continue;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al sincronizar: {ex.Message}");
                return false;
            }
        }

        public async Task<IEnumerable<Presupuesto>> GetPresupuestosByProjectId(int projectId)
        {
            try
            {
                // Primero, buscar presupuestos locales para el proyecto
                var localPresupuestos = await _localDatabase.GetPresupuestosByProjectIdAsync(projectId);
                var dtoPresupuestos = localPresupuestos.ToDtoList();

                // Si hay conexión a internet, intentar sincronizar
                if (_connectivity.NetworkAccess == NetworkAccess.Internet)
                {
                    try
                    {
                        // Obtener presupuestos del servidor para este proyecto
                        var response = await _supabaseClient
                            .From<SupabasePresupuesto>()
                            .Filter("id_proyecto", Postgrest.Constants.Operator.Equals, projectId)
                            .Order(x => x.CreatedAt, Postgrest.Constants.Ordering.Descending)
                            .Get();

                        var remotePresupuestos = response.Models;

                        // Procesar y sincronizar presupuestos remotos
                        foreach (var remotePresupuesto in remotePresupuestos)
                        {
                            var dtoRemote = remotePresupuesto.ToDto();
                            var localPresupuesto = await _localDatabase.GetPresupuestoByServerIdAsync(remotePresupuesto.Id);

                            if (localPresupuesto == null)
                            {
                                // Nuevo presupuesto del servidor
                                var newLocalPresupuesto = dtoRemote.ToLocal();
                                newLocalPresupuesto.IsSynced = true;
                                await _localDatabase.SavePresupuestoAsync(newLocalPresupuesto);
                            }
                            else if (localPresupuesto.IsSynced)
                            {
                                // Actualizar presupuesto existente
                                var updatedLocalPresupuesto = dtoRemote.ToLocal();
                                updatedLocalPresupuesto.Id = localPresupuesto.Id;
                                updatedLocalPresupuesto.ServerId = remotePresupuesto.Id;
                                updatedLocalPresupuesto.IsSynced = true;
                                await _localDatabase.SavePresupuestoAsync(updatedLocalPresupuesto);
                            }
                        }

                        // Recargar presupuestos locales
                        localPresupuestos = await _localDatabase.GetPresupuestosByProjectIdAsync(projectId);
                        dtoPresupuestos = localPresupuestos.ToDtoList();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error al sincronizar presupuestos: {ex.Message}");
                    }
                }

                return dtoPresupuestos.OrderByDescending(p => p.CreatedAt);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al obtener presupuestos: {ex.Message}");
                return Enumerable.Empty<Presupuesto>();
            }
        }

        //Etapas de presupuestos
        public async Task<IEnumerable<Etapa>> GetEtapasByPresupuestoId(int presupuestoId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[GetEtapasByPresupuestoId] INICIO para presupuestoId: {presupuestoId}");

                // 1. Obtener etapas locales
                System.Diagnostics.Debug.WriteLine($"Retornar etapas localmente del presupuesto ID: {presupuestoId}");
                var localEtapas = await _localDatabase.GetEtapasByPresupuestoIdAsync(presupuestoId);
                System.Diagnostics.Debug.WriteLine($"[GetEtapasByPresupuestoId] Etapas locales encontradas: {localEtapas.Count}");
                var dtoEtapas = localEtapas.Select(e => e.ToDto()).ToList();

                // Si hay conexión a internet, sincronizar
                if (_connectivity.NetworkAccess == NetworkAccess.Internet)
                {
                    System.Diagnostics.Debug.WriteLine("[GetEtapasByPresupuestoId] Hay conexión a internet. Consultando servidor...");
                    try
                    {
                        var response = await _supabaseClient
                            .From<SupabaseEtapa>()
                            .Filter("id_presupuesto", Postgrest.Constants.Operator.Equals, presupuestoId)
                            .Order(x => x.CreatedAt, Postgrest.Constants.Ordering.Descending)
                            .Get();

                        var remoteEtapas = response.Models;
                        System.Diagnostics.Debug.WriteLine($"[GetEtapasByPresupuestoId] Etapas remotas encontradas: {remoteEtapas.Count}");

                        if (remoteEtapas.Any())
                        {
                            foreach (var remoteEtapa in remoteEtapas)
                            {
                                var dtoRemote = remoteEtapa.ToDto();
                                var localEtapa = dtoRemote.ToLocal();

                                localEtapa.IdPresupuesto = presupuestoId;
                                localEtapa.IsSynced = true;

                                var existingLocal = await _localDatabase.GetEtapaByServerIdAsync(remoteEtapa.Id);
                                if (existingLocal != null)
                                {
                                    localEtapa.Id = existingLocal.Id;
                                }

                                await _localDatabase.SaveEtapaAsync(localEtapa);
                            }

                            // Recargar etapas locales
                            localEtapas = await _localDatabase.GetEtapasByPresupuestoIdAsync(presupuestoId);
                            dtoEtapas = localEtapas.ToDtoList();
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("[GetEtapasByPresupuestoId] No se encontraron etapas remotas.");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error al sincronizar etapas: {ex.Message}");
                        System.Diagnostics.Debug.WriteLine($"[GetEtapasByPresupuestoId] Error al sincronizar etapas: {ex.Message}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[GetEtapasByPresupuestoId] No hay conexión a internet. Solo se usan etapas locales.");
                }

                var todasActividades = await GetActividadesAsync(); // Carga todas una vez
                var todasCategorias = await GetCategoriasActividadAsync();
                var todasUnidades = await GetUniMedReAsync();

                var etapaTasks = dtoEtapas.Select(async etapaDto =>
                {
                    // Poblar Actividad principal de la Etapa
                    if (etapaDto.IdActividadEtapa.HasValue && etapaDto.Actividad == null)
                    {
                        etapaDto.Actividad = todasActividades.FirstOrDefault(a => a.IdActividad == etapaDto.IdActividadEtapa.Value);
                        if (etapaDto.Actividad != null)
                        {
                            if (etapaDto.Actividad.CategoriaActividadId.HasValue && etapaDto.Actividad.CategoriaActividad == null)
                            {
                                etapaDto.Actividad.CategoriaActividad = todasCategorias.FirstOrDefault(c => c.IdCategoriaActividad == etapaDto.Actividad.CategoriaActividadId.Value);
                            }
                            // Poblar UnidadMedida de la Actividad principal si no está ya
                            if (etapaDto.Actividad.UnidadMedidaId.HasValue && etapaDto.Actividad.UnidadMedida == null)
                            {
                                etapaDto.Actividad.UnidadMedida = todasUnidades.FirstOrDefault(u => u.Id == etapaDto.Actividad.UnidadMedidaId.Value);
                            }
                        }
                    }

                    // Cargar y poblar SubEtapas de la Etapa
                    var subEtapasDeEtapa = await GetSubEtapasByEtapaId(etapaDto.Id); // Este método ya debe poblar Actividad y CategoriaActividad en las subetapas
                    etapaDto.SubEtapas = subEtapasDeEtapa.ToList();
                    System.Diagnostics.Debug.WriteLine($"[OFDS.GetEtapasByPresupuestoId] Cargadas {etapaDto.SubEtapas.Count} subetapas para Etapa ID: {etapaDto.Id}");
                });

                await Task.WhenAll(etapaTasks);

                System.Diagnostics.Debug.WriteLine($"[GetEtapasByPresupuestoId] Retornando {dtoEtapas.Count} etapas, ahora con sus subetapas pobladas.");
                return dtoEtapas.OrderBy(e => e.CreatedAt);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener etapas: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[GetEtapasByPresupuestoId] Error al obtener etapas: {ex.Message}");
                return Enumerable.Empty<Etapa>();
            }
        }

        public async Task<Etapa> SaveEtapa(Etapa etapa)
        {
            try
            {
                // Convertir a modelo local
                var localEtapa = etapa.ToLocal();

                // Guardar localmente
                await _localDatabase.SaveEtapaAsync(localEtapa);

                // Si hay conexión a internet, sincronizar con el servidor
                if (_connectivity.NetworkAccess == NetworkAccess.Internet)
                {
                    try
                    {
                        // Convertir a modelo Supabase
                        var supabaseEtapa = etapa.ToSupabase();

                        // Enviar al servidor
                        var response = await _supabaseClient.From<SupabaseEtapa>().Upsert(supabaseEtapa);
                        var syncedEtapa = response.Models.FirstOrDefault();

                        if (syncedEtapa != null)
                        {
                            // Actualizar con el ID del servidor
                            localEtapa.Id = syncedEtapa.Id;
                            localEtapa.IsSynced = true;
                            await _localDatabase.SaveEtapaAsync(localEtapa);

                            // Devolver el DTO actualizado
                            return syncedEtapa.ToDto();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error al sincronizar etapa: {ex.Message}");
                        System.Diagnostics.Debug.WriteLine($"Error al sincronizar etapa: {ex.Message}");

                        // Marcar como no sincronizado
                        localEtapa.IsSynced = false;
                        await _localDatabase.SaveEtapaAsync(localEtapa);
                    }
                }

                return localEtapa.ToDto();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al guardar etapa: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Error al guardar etapa: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteEtapa(long etapaId)
        {
            try
            {
                // Buscar etapa local
                var localEtapa = await _localDatabase.GetEtapaByIdAsync(etapaId);

                if (localEtapa != null)
                {
                    // Eliminar localmente
                    await _localDatabase.DeleteEtapaAsync(localEtapa);

                    // Si hay conexión a internet y tiene ID de servidor, eliminar en el servidor
                    if (localEtapa.ServerId.HasValue && _connectivity.NetworkAccess == NetworkAccess.Internet)
                    {
                        try
                        {
                            await _supabaseClient
                                .From<SupabaseEtapa>()
                                .Filter("id", Postgrest.Constants.Operator.Equals, localEtapa.ServerId.Value)
                                .Delete();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Error al eliminar etapa en el servidor: {ex.Message}");
                            System.Diagnostics.Debug.WriteLine($"Error al eliminar etapa en el servidor: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al eliminar etapa: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Error al eliminar etapa: {ex.Message}");
                throw;
            }
        }

        public static long GenerarIdAleatorio()
        {
            int numeroAleatorioInt = _randomGenerator.Next(1, 100_000_000);
            return numeroAleatorioInt;
        }

        //Mejorarlo solo se muestran los presupuestos que se guardan en el servidor, pero si se estan guardando localmente
        public async Task<Presupuesto> SavePresupuesto(Presupuesto presupuesto)
        {
            try
            {
                if (presupuesto.Id == 0)
                {
                    presupuesto.CreatedAt = DateTime.Now;
                }

                var localPresupuesto = presupuesto.ToLocal();
                if (presupuesto.Id != 0 && localPresupuesto.ServerId == null)
                {
                    localPresupuesto.ServerId = presupuesto.Id;
                }

                await _localDatabase.SavePresupuestoAsync(localPresupuesto);

                if (_connectivity.NetworkAccess == NetworkAccess.Internet)
                {
                    try
                    {
                        var supabasePresupuesto = presupuesto.ToSupabase();
                        var response = await _supabaseClient.From<SupabasePresupuesto>().Upsert(supabasePresupuesto);
                        var syncedPresupuesto = response.Models.FirstOrDefault();

                        if (syncedPresupuesto != null)
                        {
                            localPresupuesto.ServerId = syncedPresupuesto.Id;
                            localPresupuesto.CreatedAt = syncedPresupuesto.CreatedAt;
                            localPresupuesto.IsSynced = true;
                            await _localDatabase.SavePresupuestoAsync(localPresupuesto);
                            return syncedPresupuesto.ToDto();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error al sincronizar presupuesto: {ex.Message}");
                        localPresupuesto.IsSynced = false;
                        await _localDatabase.SavePresupuestoAsync(localPresupuesto);
                    }
                }
                else
                {
                    localPresupuesto.IsSynced = false;
                    await _localDatabase.SavePresupuestoAsync(localPresupuesto);
                }

                return localPresupuesto.ToDto();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al guardar presupuesto: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Error al guardar presupuesto: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<Presupuesto>> GetAllPresupuestos()
        {
            try
            {
                // 1. Obtener presupuestos locales
                var localPresupuestos = await _localDatabase.GetAllPresupuestosAsync();
                var dtoPresupuestos = localPresupuestos.ToDtoList();

                // 2. Si hay conexion, sincronizar con el servidor
                if (_connectivity.NetworkAccess == NetworkAccess.Internet)
                {
                    System.Diagnostics.Debug.WriteLine($"Hay conexion");
                    try
                    {
                        // 2.1. Sincroniza cambios locales pendientes (presupuestos no sincronizados)
                        var noSincronoLocalPresupuestos = localPresupuestos.Where(p => !p.IsSynced).ToList();
                        foreach (var localPresupuesto in noSincronoLocalPresupuestos)
                        {
                            System.Diagnostics.Debug.WriteLine($"Presupuesto id: {localPresupuesto.Id}"); //Para debugging
                            try
                            {
                                var dto = localPresupuesto.ToDto();
                                var supabaseModel = dto.ToSupabase();

                                // insertar o actualizar en servidor
                                var response = await _supabaseClient.From<SupabasePresupuesto>().Upsert(supabaseModel);
                                var syncedPresupuesto = response.Models.FirstOrDefault();

                                if (syncedPresupuesto != null)
                                {
                                    // Actualiza el local con el id de servidor y marca como sincronizado
                                    System.Diagnostics.Debug.WriteLine($"Presupuesto sincornizado id: {syncedPresupuesto.Id}");
                                    localPresupuesto.ServerId = syncedPresupuesto.Id;
                                    localPresupuesto.IsSynced = true;
                                    await _localDatabase.SavePresupuestoAsync(localPresupuesto);
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error al sincronizar presupuesto local: {ex.Message}");
                                continue;
                            }
                        }

                        // 2.2 Descargar todos los presupuestos del servidor
                        System.Diagnostics.Debug.WriteLine("Descargar todos los presupuestos del servidor");
                        var responseRemote = await _supabaseClient
                            .From<SupabasePresupuesto>()
                            .Order(x => x.CreatedAt, Postgrest.Constants.Ordering.Descending)
                            .Get();

                        var remotePresupuestos = responseRemote.Models;
                        System.Diagnostics.Debug.WriteLine($"Presupuestos remotos {remotePresupuestos.Count()}");

                        // 2.3. Agregar solo los presupuestos remotos que no existen localmente
                        var localServerIds = localPresupuestos
                            .Where(p => p.ServerId != null)
                            .Select(p => p.ServerId.Value)
                            .ToHashSet();

                        foreach (var remotePresupuesto in remotePresupuestos)
                        {
                            if (!localServerIds.Contains(remotePresupuesto.Id))
                            {
                                var dtoRemote = remotePresupuesto.ToDto();
                                var newLocal = dtoRemote.ToLocal();
                                newLocal.IsSynced = true;
                                await _localDatabase.SavePresupuestoAsync(newLocal);
                            }
                        }

                        // 2.4 Recarga todos los locales actualizados
                        localPresupuestos = await _localDatabase.GetAllPresupuestosAsync();
                        dtoPresupuestos = localPresupuestos.ToDtoList();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error al sincronizar presupuestos: {ex.Message}");
                    }
                }

                System.Diagnostics.Debug.WriteLine($"Presupuestos retornados: {dtoPresupuestos.Count()}");
                return dtoPresupuestos.OrderByDescending(p => p.CreatedAt);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al obtener presupuestos: {ex.Message}");
                return Enumerable.Empty<Presupuesto>();
            }
        }

        public async Task<bool> DeletePresupuesto(long presupuestoId)
        {
            try
            {
                // Buscar presupuesto local por ServerId o Id local
                var localPresupuesto = await _localDatabase.GetPresupuestoByServerIdAsync(presupuestoId);
                if (localPresupuesto == null)
                {
                    // Si no se encuentra por ServerId, intenta por Id local
                    var allLocal = await _localDatabase.GetAllPresupuestosAsync();
                    localPresupuesto = allLocal.FirstOrDefault(p => p.Id == presupuestoId);
                }

                if (localPresupuesto != null)
                {
                    // Eliminar localmente
                    await _localDatabase.DeletePresupuestoAsync(localPresupuesto);

                    // Si tiene ServerId y hay conexión, eliminar en el servidor
                    if (localPresupuesto.ServerId.HasValue && _connectivity.NetworkAccess == NetworkAccess.Internet)
                    {
                        try
                        {
                            await _supabaseClient
                                .From<SupabasePresupuesto>()
                                .Filter("id", Postgrest.Constants.Operator.Equals, (int)localPresupuesto.ServerId.Value)
                                .Delete();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Error al eliminar presupuesto en el servidor: {ex.Message}");
                            System.Diagnostics.Debug.WriteLine($"Error al eliminar presupuesto en el servidor: {ex.Message}");
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al eliminar presupuesto: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Error al eliminar presupuesto: {ex.Message}");
                return false;
            }
        }

        public async Task<Presupuesto> GetPresupuestoByIdAsync(long presupuestoId)
        {
            try
            {
                var localPresupuesto = await _localDatabase.GetPresupuestoByServerIdAsync(presupuestoId);
                if (localPresupuesto == null)
                {
                    var allLocal = await _localDatabase.GetAllPresupuestosAsync();
                    localPresupuesto = allLocal.FirstOrDefault(p => p.Id == presupuestoId);
                }

                if (_connectivity.NetworkAccess == NetworkAccess.Internet)
                {
                    try
                    {
                        var response = await _supabaseClient
                            .From<SupabasePresupuesto>()
                            .Filter("id", Postgrest.Constants.Operator.Equals, presupuestoId.ToString())
                            .Single();

                        if (response != null)
                        {
                            var remoteDto = response.ToDto();
                            if (localPresupuesto == null || (localPresupuesto.ServerId == remoteDto.Id && localPresupuesto.IsSynced))
                            {
                                var updatedLocal = remoteDto.ToLocal();
                                if (localPresupuesto != null) updatedLocal.Id = localPresupuesto.Id;
                                updatedLocal.IsSynced = true;
                                await _localDatabase.SavePresupuestoAsync(updatedLocal);
                            }
                            return remoteDto;
                        }
                    }
                    catch (Postgrest.Exceptions.PostgrestException pgex) when (pgex.Message.Contains("PGRST116"))
                    {
                        _logger.LogWarning($"Presupuesto with ID {presupuestoId} not found on server. PGRST116.");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error fetching Presupuesto {presupuestoId} from Supabase. Falling back to local if available.");
                    }
                }

                return localPresupuesto?.ToDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in GetPresupuestoByIdAsync for {presupuestoId}.");
                return null;
            }
        }

        public async Task<SubEtapa> GetSubEtapaByIdAsync(long subEtapaId)
        {
            try
            {
                LocalSubEtapa localSubEtapa = await _localDatabase.GetLocalSubEtapaByIdAsync(subEtapaId);

                if (_connectivity.NetworkAccess == NetworkAccess.Internet)
                {
                    try
                    {
                        var response = await _supabaseClient
                            .From<SupabaseSubEtapa>()
                            .Filter("id", Postgrest.Constants.Operator.Equals, subEtapaId.ToString())
                            .Single();

                        if (response != null)
                        {
                            var remoteDto = response.ToDto();
                            if (localSubEtapa == null || (localSubEtapa.ServerId == remoteDto.Id && localSubEtapa.IsSynced))
                            {
                                var updatedLocal = remoteDto.ToLocal(isSynced: true);
                                if (localSubEtapa != null) updatedLocal.Id = localSubEtapa.Id;
                                await _localDatabase.SaveSubEtapaAsync(updatedLocal);
                                return remoteDto;
                            }
                        }
                    }
                    catch (Postgrest.Exceptions.PostgrestException pgex) when (pgex.Message.Contains("PGRST116"))
                    {
                        _logger.LogWarning($"SubEtapa with ID {subEtapaId} not found on server. PGRST116.");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error fetching SubEtapa {subEtapaId} from Supabase. Falling back to local if available.");
                    }
                }

                return localSubEtapa?.ToDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in GetSubEtapaByIdAsync for {subEtapaId}.");
                return null;
            }
        }

        public async Task<IEnumerable<SubEtapa>> GetSubEtapasByEtapaId(long etapaId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[GetSubEtapasByEtapaId] INICIO para EtapaId: {etapaId}");

                // 1. Obtener subetapas locales
                System.Diagnostics.Debug.WriteLine($"[GetSubEtapasByEtapaId] Obteniendo subetapas locales para EtapaId: {etapaId}");
                var localSubEtapas = await _localDatabase.GetSubEtapasByEtapaIdAsync(etapaId);
                System.Diagnostics.Debug.WriteLine($"[GetSubEtapasByEtapaId] Subetapas locales encontradas: {localSubEtapas.Count}");

                var dtoEtapas = localSubEtapas.Select(e => e.ToDto()).ToList();

                // 2. Si hay conexión a internet, sincronizar con Supabase
                System.Diagnostics.Debug.WriteLine($"[GetSubEtapasByEtapaId] Estado de conexión: {_connectivity.NetworkAccess}");
                if (_connectivity.NetworkAccess == NetworkAccess.Internet)
                {
                    System.Diagnostics.Debug.WriteLine("[GetSubEtapasByEtapaId] Hay conexión a Internet. Consultando servidor Supabase...");
                    try
                    {
                        System.Diagnostics.Debug.WriteLine($"[GetSubEtapasByEtapaId] Filtro Supabase: id_etapa == {etapaId} (tipo: {etapaId.GetType()})");
                        var response = await _supabaseClient
                            .From<SupabaseSubEtapa>()
                            .Filter("id_etapa", Postgrest.Constants.Operator.Equals, etapaId.ToString())
                            .Order(x => x.CreatedAt, Postgrest.Constants.Ordering.Descending)
                            .Get();
                        System.Diagnostics.Debug.WriteLine($"[GetSubEtapasByEtapaId] Subetapas remotas encontradas: {response.Models.Count}");

                        var remoteSubEtapas = response.Models;
                        System.Diagnostics.Debug.WriteLine($"[GetSubEtapasByEtapaId] Subetapas remotas encontradas: {remoteSubEtapas.Count}");

                        if (remoteSubEtapas.Any())
                        {
                            foreach (var remoteSubEtapa in remoteSubEtapas)
                            {
                                var dtoRemote = remoteSubEtapa.ToDto();
                                var localSubEtapa = dtoRemote.ToLocal();

                                localSubEtapa.IdEtapa = etapaId;
                                localSubEtapa.IsSynced = true;

                                var existingLocal = await _localDatabase.GetLocalSubEtapaByIdAsync(remoteSubEtapa.Id);
                                if (existingLocal != null)
                                {
                                    localSubEtapa.Id = existingLocal.Id;
                                }

                                await _localDatabase.SaveSubEtapaAsync(localSubEtapa);
                            }

                            // Recargar subetapas locales
                            localSubEtapas = await _localDatabase.GetSubEtapasByEtapaIdAsync(etapaId);
                            dtoEtapas = localSubEtapas.Select(e => e.ToDto()).ToList();
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("[GetSubEtapasByEtapaId] No se encontraron subetapas remotas.");
                        }
                    }
                    catch (Exception ex)
                    {
                        string msg = $"[GetSubEtapasByEtapaId] Error al sincronizar subetapas: {ex.Message}";
                        _logger?.LogError(msg);
                        System.Diagnostics.Debug.WriteLine(msg);
                        System.Diagnostics.Debug.WriteLine($"[GetSubEtapasByEtapaId] StackTrace: {ex.StackTrace}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[GetSubEtapasByEtapaId] No hay conexión a Internet. Se usan solo subetapas locales.");
                }

                var todasActividades = await GetActividadesAsync(); // Carga todas una vez
                var todasCategorias = await GetCategoriasActividadAsync(); // Carga todas una vez
                //var todasUnidades = await GetUniMedReAsync(); // Carga todas una vez

                foreach (var subEtapaDto in dtoEtapas) // Asegúrate que esta variable es la colección de SubEtapa DTOs
                {
                    if (subEtapaDto.ActividadSubEtapaId.HasValue && subEtapaDto.Actividad == null)
                    {
                        subEtapaDto.Actividad = todasActividades.FirstOrDefault(a => a.IdActividad == subEtapaDto.ActividadSubEtapaId.Value);
                        if (subEtapaDto.Actividad != null)
                        {
                            if (subEtapaDto.Actividad.CategoriaActividadId.HasValue && subEtapaDto.Actividad.CategoriaActividad == null)
                            {
                                subEtapaDto.Actividad.CategoriaActividad = todasCategorias.FirstOrDefault(cat => cat.IdCategoriaActividad == subEtapaDto.Actividad.CategoriaActividadId.Value);
                            }
                            /* Poblar UnidadMedida de la Actividad de la SubEtapa si no está ya
                            if (subEtapaDto.Actividad.UnidadMedidaId.HasValue && subEtapaDto.Actividad.UnidadMedida == null)
                            {
                                subEtapaDto.Actividad.UnidadMedida = todasUnidades.FirstOrDefault(u => u.Id == subEtapaDto.Actividad.UnidadMedidaId.Value);
                            } */
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine($"[GetSubEtapasByEtapaId] Retornando {dtoEtapas.Count} subetapas.");
                return dtoEtapas.OrderBy(e => e.CreatedAt);
            }
            catch (Exception ex)
            {
                string msg = $"[GetSubEtapasByEtapaId] Error general al obtener subetapas: {ex.Message}";
                _logger?.LogError(msg);
                System.Diagnostics.Debug.WriteLine(msg);
                return Enumerable.Empty<SubEtapa>();
            }
        }

        public async Task SaveSubEtapa(SubEtapa subEtapaDto) // El DTO viene del ViewModel
        {
            System.Diagnostics.Debug.WriteLine($"Iniciando SaveSubEtapa para DTO Id: {subEtapaDto.Id}");
            subEtapaDto.CreatedAt = DateTime.UtcNow; // Asegurar CreatedAt

            var local = subEtapaDto.ToLocal();
            local.IsSynced = false; // Marcar como no sincronizado inicialmente

            await _localDatabase.SaveSubEtapaAsync(local); // Guarda/Actualiza localmente. Genera LocalId si es nuevo.
            System.Diagnostics.Debug.WriteLine($"SubEtapa guardada/actualizada localmente. LocalId: {local.Id}, ServerId: {local.ServerId}");

            if (_connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("Conectado. Intentando sincronizar SubEtapa con Supabase...");
                    SupabaseSubEtapa supabaseModel = subEtapaDto.ToSupabase();

                    if (!local.ServerId.HasValue || local.ServerId == 0) // Indica que es un nuevo ítem para Supabase
                    {
                        supabaseModel.Id = default(long);
                    }
                    else
                    {
                        supabaseModel.Id = local.ServerId.Value; // Actualiza usando el ServerId existente
                    }

                    var response = await _supabaseClient.From<SupabaseSubEtapa>().Upsert(supabaseModel);
                    var synced = response.Models.FirstOrDefault();

                    if (synced != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Sincronización de SubEtapa exitosa. ServerId obtenido: {synced.Id}");
                        local.ServerId = synced.Id; // ¡Importante! Actualiza el ServerId del registro local.
                        local.CreatedAt = synced.CreatedAt; // Actualiza CreatedAt desde el servidor si es necesario.
                        local.IsSynced = true;
                        await _localDatabase.SaveSubEtapaAsync(local); // Vuelve a guardar el registro local con el ServerId y estado sincronizado.
                        System.Diagnostics.Debug.WriteLine($"Registro local de SubEtapa actualizado con ServerId: {local.ServerId}, LocalId: {local.Id}");

                        // Actualiza el DTO original con el ServerId (y potencialmente otros campos del servidor)
                        subEtapaDto.Id = synced.Id;
                        subEtapaDto.CreatedAt = synced.CreatedAt;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Sincronización de SubEtapa con Supabase no devolvió ningún modelo.");
                    }
                }
                catch (Exception ex)
                {
                    var msg = $"Error al sincronizar subetapa: {ex.Message}";
                    _logger?.LogError(msg);
                    System.Diagnostics.Debug.WriteLine(msg);
                    System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                    // local.IsSynced permanece false.
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("No hay conexión a Internet. Sincronización de SubEtapa omitida.");
            }
            System.Diagnostics.Debug.WriteLine("Finalizando SaveSubEtapa.");
        }

        // CategoriaRec
        public async Task<IEnumerable<CategoriaRec>> GetCategoriasRecAsync()
        {
            try
            {
                // var localItems = await _localDatabase.GetCategoriasRecAsync();
                // For now, let's assume GetCategoriasRecAsync exists in LocalDatabaseService
                // If not, you need to add: public async Task<List<LocalCategoriaRec>> GetCategoriasRecAsync() => await _database.Table<LocalCategoriaRec>().ToListAsync();
                var localItems = await _localDatabase.GetCategoriasRecAsync();
                var dtos = localItems.ToDtoList();

                if (_connectivity.NetworkAccess == NetworkAccess.Internet)
                {
                    var response = await _supabaseClient.From<SupabaseCategoriaRec>().Get();
                    var remoteItems = response.Models.ToDtoList(); // Using mapper

                    // Simple sync: clear local and repopulate from server
                    // More advanced sync would involve comparing timestamps or versions.
                    // await _localDatabase.ClearCategoriasRecAsync(); // Requires this method in LocalDatabaseService
                    // For now, let's just replace the DTO list and save to local
                    dtos = remoteItems;
                    foreach (var dto in dtos)
                    {
                        // Assuming SaveCategoriaRecAsync in LocalDatabaseService handles InsertOrUpdate based on ServerId
                        await _localDatabase.SaveCategoriaRecAsync(dto.ToLocal(isSynced: true));
                    }
                }
                return dtos.OrderBy(c => c.NombreCatRec);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting CategoriasRec.");
                return Enumerable.Empty<CategoriaRec>();
            }
        }

        public async Task SaveCategoriaRecAsync(CategoriaRec categoria)
        {
            // This is likely an admin function not fully implemented for offline first from client.
            // For now, just a pass-through if online.
            _logger.LogInformation("SaveCategoriaRecAsync called. This is primarily an admin function.");
            if (_connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                await _supabaseClient.From<SupabaseCategoriaRec>().Upsert(categoria.ToSupabase());
            }
            else
            {
                // Optionally save locally as unsynced if modification from client is a feature
                var local = categoria.ToLocal(isSynced: false);
                await _localDatabase.SaveCategoriaRecAsync(local);
            }
        }

        // UniMedRe
        public async Task<IEnumerable<UniMedRe>> GetUniMedReAsync()
        {
            try
            {
                var localItems = await _localDatabase.GetUniMedReAsync();
                var dtos = localItems.ToDtoList();

                if (_connectivity.NetworkAccess == NetworkAccess.Internet)
                {
                    var response = await _supabaseClient.From<SupabaseUniMedRe>().Get();
                    var remoteItems = response.Models.ToDtoList();

                    dtos = remoteItems;
                    foreach (var dto in dtos)
                    {
                        await _localDatabase.SaveUniMedReAsync(dto.ToLocal(isSynced: true));
                    }
                }
                return dtos.OrderBy(u => u.NombreUniMedRe);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting UniMedRe.");
                return Enumerable.Empty<UniMedRe>();
            }
        }

        public async Task SaveUniMedReAsync(UniMedRe uniMedRe)
        {
            _logger.LogInformation("SaveUniMedReAsync called. This is primarily an admin function.");
            if (_connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                await _supabaseClient.From<SupabaseUniMedRe>().Upsert(uniMedRe.ToSupabase());
            }
            else
            {
                var local = uniMedRe.ToLocal(isSynced: false);
                await _localDatabase.SaveUniMedReAsync(local);
            }
        }

        // Recurso
        public async Task<IEnumerable<Recurso>> GetRecursosAsync(long? idCategoriaRec = null)
        {
            try
            {
                var localItems = await _localDatabase.GetRecursosAsync(idCategoriaRec);
                var dtos = localItems.ToDtoList();

                if (_connectivity.NetworkAccess == NetworkAccess.Internet)
                {
                    var query = _supabaseClient.From<SupabaseRecurso>();
                    if (idCategoriaRec.HasValue)
                    {
                        query = (Supabase.Interfaces.ISupabaseTable<SupabaseRecurso, Supabase.Realtime.RealtimeChannel>)query.Filter("id_cat_rec", Postgrest.Constants.Operator.Equals, idCategoriaRec.Value.ToString());
                    }
                    var response = await query.Get();
                    var remoteItems = response.Models.ToDtoList();

                    dtos = remoteItems; // Simple overwrite for now
                    // Clear local for this category or all if no category specified
                    // await _localDatabase.ClearRecursosAsync(idCategoriaRec); 
                    foreach (var dto in dtos)
                    {
                        await _localDatabase.SaveRecursoAsync(dto.ToLocal(isSynced: true));
                    }
                }

                // Populate CategoriaRec name for display convenience
                var categorias = await GetCategoriasRecAsync();
                foreach (var recurso in dtos)
                {
                    if (recurso.IdCatRec.HasValue)
                    {
                        recurso.CategoriaRec = categorias.FirstOrDefault(c => c.Id == recurso.IdCatRec.Value);
                    }
                }
                return dtos.OrderBy(r => r.NombreRecurso);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting Recursos (category: {idCategoriaRec}).");
                return Enumerable.Empty<Recurso>();
            }
        }

        public async Task SaveRecursoAsync(Recurso recurso)
        {
            _logger.LogInformation("SaveRecursoAsync called. This is primarily an admin function.");
            if (_connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                await _supabaseClient.From<SupabaseRecurso>().Upsert(recurso.ToSupabase());
            }
            else
            {
                var local = recurso.ToLocal(isSynced: false);
                await _localDatabase.SaveRecursoAsync(local);
            }
        }

        // RecursoUti (from previous detailed generation, ensure it's complete)
        public async Task<IEnumerable<RecursoUti>> GetRecursosUtiBySubEtapaIdAsync(long subEtapaId)
        {
            try
            {
                await SyncRecursosUtiAsync(); // Sync pending local changes first

                var localItems = await _localDatabase.GetRecursosUtiBySubEtapaIdAsync(subEtapaId);
                var dtos = localItems.Select(l => RecursosMapper.ToDto(l)).ToList(); // Use explicit mapper call

                if (_connectivity.NetworkAccess == NetworkAccess.Internet)
                {
                    var response = await _supabaseClient.From<SupabaseRecursoUti>()
                                                    .Filter("id_sub_etapa", Postgrest.Constants.Operator.Equals, subEtapaId.ToString())
                                                    .Get();
                    var remoteSupabaseItems = response.Models;

                    // Sync logic:
                    // 1. Get all local ServerIds for this subEtapa
                    var localServerIds = localItems.Where(l => l.ServerId.HasValue).Select(l => l.ServerId.Value).ToHashSet();
                    // 2. Keep track of ServerIds from remote to find items to delete locally (if server is source of truth)
                    var remoteServerIds = new HashSet<long>();

                    foreach (var remoteSupabase in remoteSupabaseItems)
                    {
                        var remoteDto = RecursosMapper.ToDto(remoteSupabase); // Use explicit mapper call
                        remoteServerIds.Add(remoteDto.Id);
                        var existingLocal = await _localDatabase.GetLocalRecursoUtiByServerIdAsync(remoteDto.Id);

                        if (existingLocal == null) // New item from server
                        {
                            var newLocal = RecursosMapper.ToLocal(remoteDto, isSynced: true); // Use explicit mapper call
                            newLocal.LocalId = 0;
                            await _localDatabase.SaveRecursoUtiAsync(newLocal);
                        }
                        // Assuming server is truth, if local item has IsSynced = true and differs, update it
                        // This simple example doesn't handle complex conflict resolution (e.g. last-write-wins based on a timestamp)
                        else if (existingLocal.IsSynced && remoteDto.CreatedAt > existingLocal.CreatedAt) // Simple check: if server version is newer
                        {
                            var updatedLocal = RecursosMapper.ToLocal(remoteDto, isSynced: true); // Use explicit mapper call
                            updatedLocal.LocalId = existingLocal.LocalId;
                            await _localDatabase.SaveRecursoUtiAsync(updatedLocal);
                        }
                    }
                    // Example: Delete local items that are synced but no longer on the server for this subEtapa
                    var itemsToDeleteLocally = localItems.Where(l => l.IsSynced && l.ServerId.HasValue && !remoteServerIds.Contains(l.ServerId.Value)).ToList();
                    foreach (var itemToDel in itemsToDeleteLocally)
                    {
                        await _localDatabase.DeleteRecursoUtiByLocalIdAsync(itemToDel.LocalId); // Need DeleteRecursoUtiByLocalIdAsync
                    }


                    // Re-fetch from local to get a consistent merged view
                    localItems = await _localDatabase.GetRecursosUtiBySubEtapaIdAsync(subEtapaId);
                    dtos = localItems.Select(l => RecursosMapper.ToDto(l)).ToList(); // Use explicit mapper call
                }

                // Populate navigation properties
                var allRecursos = await GetRecursosAsync(); // Asegúrate que esto devuelva todos los recursos necesarios.
                var allUniMedRe = await GetUniMedReAsync(); // Asegúrate que esto devuelva todas las unidades necesarias.

                foreach (var dto in dtos)
                {
                    if (dto.IdRecurso.HasValue)
                        dto.Recurso = allRecursos.FirstOrDefault(r => r.Id == dto.IdRecurso.Value);
                    if (dto.IdUniMedRe.HasValue)
                        dto.UniMedRe = allUniMedRe.FirstOrDefault(u => u.Id == dto.IdUniMedRe.Value);
                    // Si dto.Recurso o dto.UniMedRe son null aquí, los nombres no aparecerán en la UI.
                }
                return dtos.OrderByDescending(r => r.CreatedAt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting RecursosUti for SubEtapaId {subEtapaId}.");
                return Enumerable.Empty<RecursoUti>();
            }
        }

        public async Task<RecursoUti> SaveRecursoUtiAsync(RecursoUti recursoUti)
        {
            try
            {
                recursoUti.CreatedAt = DateTime.UtcNow;
                if (recursoUti.CantidadRecursosUti.HasValue && recursoUti.PrecioUniRecursosUti.HasValue)
                {
                    recursoUti.TotalRecursosUti = recursoUti.CantidadRecursosUti * recursoUti.PrecioUniRecursosUti;
                }

                var local = RecursosMapper.ToLocal(recursoUti, isSynced: false); // Use explicit mapper call

                // Ensure LocalId is 0 if it's a brand new record, so SQLite auto-increments.
                // If recursoUti.Id contains a ServerId for an existing record, ToLocal should map it to local.ServerId.
                if (recursoUti.Id == 0) local.LocalId = 0; // If DTO Id is 0, it's new


                await _localDatabase.SaveRecursoUtiAsync(local);
                recursoUti.Id = local.ServerId ?? local.LocalId;

                if (_connectivity.NetworkAccess == NetworkAccess.Internet)
                {
                    var supabaseModel = RecursosMapper.ToSupabase(recursoUti); // Use explicit mapper call
                    // If it's a new item for Supabase (local.ServerId was null), SupabaseRecursoUti.Id should be default
                    if (local.ServerId == null) supabaseModel.Id = default;
                    else supabaseModel.Id = local.ServerId.Value;


                    var response = await _supabaseClient.From<SupabaseRecursoUti>().Upsert(supabaseModel);
                    var synced = response.Models.FirstOrDefault();
                    if (synced != null)
                    {
                        var syncedDto = RecursosMapper.ToDto(synced); // Use explicit mapper call
                        local.ServerId = syncedDto.Id;
                        local.IsSynced = true;
                        // Update other fields from syncedDto if necessary (e.g. server-generated CreatedAt)
                        local.CreatedAt = syncedDto.CreatedAt;
                        await _localDatabase.SaveRecursoUtiAsync(local);
                        return syncedDto; // Return the DTO from server response
                    }
                }
                // If offline, or sync failed, return the DTO based on local data
                // The DTO's ID should reflect the local ID if it hasn't been synced yet.
                recursoUti.Id = local.LocalId; // Make sure DTO Id is local if not synced
                return recursoUti;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving RecursoUti.");
                throw;
            }
        }

        public async Task DeleteRecursoUtiAsync(long recursoUtiId) // This Id is likely the DTO's Id (ServerId if synced, LocalId if not)
        {
            try
            {
                // Try to find by ServerId first, then by LocalId
                var localItem = await _localDatabase.GetLocalRecursoUtiByServerIdAsync(recursoUtiId) ?? await _localDatabase.GetLocalRecursoUtiByLocalIdAsync(recursoUtiId);

                if (localItem != null)
                {
                    await _localDatabase.DeleteRecursoUtiByLocalIdAsync(localItem.LocalId); // Use local PK for deletion

                    if (_connectivity.NetworkAccess == NetworkAccess.Internet && localItem.ServerId.HasValue)
                    {
                        await _supabaseClient.From<SupabaseRecursoUti>()
                                             .Filter("id", Postgrest.Constants.Operator.Equals, localItem.ServerId.Value)
                                             .Delete();
                    }
                    // If offline & had ServerId, should mark for server deletion on next sync
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting RecursoUti {recursoUtiId}.");
            }
        }

        public async Task SyncRecursosUtiAsync() //
        {
            if (_connectivity.NetworkAccess != NetworkAccess.Internet) return; //

            var unsyncedItems = await _localDatabase.GetUnsyncedRecursosUtiAsync(); //
            foreach (var localItem in unsyncedItems) //
            {
                try
                {
                    var dto = RecursosMapper.ToDto(localItem); // Use explicit mapper call
                    var supabaseModel = RecursosMapper.ToSupabase(dto); // Use explicit mapper call

                    // If localItem.ServerId is null, it's a new item, Supabase should generate Id
                    // If localItem.ServerId has value, it's an update to existing server item
                    if (localItem.ServerId == null)
                    {
                        supabaseModel.Id = default; // Let Supabase assign ID
                    }
                    else
                    {
                        supabaseModel.Id = localItem.ServerId.Value;
                    }

                    var response = await _supabaseClient.From<SupabaseRecursoUti>().Upsert(supabaseModel); //
                    var synced = response.Models.FirstOrDefault(); //
                    if (synced != null) //
                    {
                        localItem.ServerId = synced.Id; //
                        localItem.IsSynced = true; //
                        localItem.CreatedAt = synced.CreatedAt; // Update CreatedAt from server
                        await _localDatabase.SaveRecursoUtiAsync(localItem);
                    }
                }
                catch (Exception ex) //
                {
                    _logger.LogError(ex, $"Failed to sync RecursoUti with LocalId {localItem.LocalId}."); //
                }
            }
        }

        // --- Implementación para CategoriasActividad ---
        public async Task<IEnumerable<CategoriaActividad>> GetCategoriasActividadAsync()
        {
            try
            {
                var localItems = await _localDatabase.GetCategoriasActividadAsync(); //
                var dtos = localItems.ToDtoList(); // Asume que ActividadMapper tiene ToDtoList para LocalCategoriaActividad

                if (_connectivity.NetworkAccess == NetworkAccess.Internet)
                {
                    try
                    {
                        var response = await _supabaseClient.From<SupabaseCategoriaActividad>().Get();
                        var remoteDtos = response.Models.ToDtoList(); // Asume que ActividadMapper tiene ToDtoList para SupabaseCategoriaActividad

                        // Lógica de Sincronización (simple: reemplazar local con remoto)
                        // Podrías implementar una sincronización más robusta si es necesario
                        await _localDatabase.ClearCategoriasActividadAsync(); //
                        foreach (var dto in remoteDtos)
                        {
                            await _localDatabase.SaveCategoriaActividadAsync(dto.ToLocal(isSynced: true)); //
                        }
                        dtos = remoteDtos;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error sincronizando CategoriasActividad desde Supabase.");
                        // Continuar con datos locales si la sincronización falla
                    }
                }
                return dtos.OrderBy(c => c.NombreCategoriaActividad);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo CategoriasActividad.");
                return Enumerable.Empty<CategoriaActividad>();
            }
        }

        public async Task SaveCategoriaActividadAsync(CategoriaActividad categoriaActividad)
        {
            _logger.LogInformation("SaveCategoriaActividadAsync llamado. Principalmente para admin/sincronización.");
            var localItem = categoriaActividad.ToLocal(isSynced: false); // Mapear a local

            if (_connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                try
                {
                    var supabaseItem = categoriaActividad.ToSupabase(); // Mapear a Supabase
                    var response = await _supabaseClient.From<SupabaseCategoriaActividad>().Upsert(supabaseItem);
                    var syncedItem = response.Models.FirstOrDefault()?.ToDto();

                    if (syncedItem != null)
                    {
                        localItem = syncedItem.ToLocal(isSynced: true); // Actualizar local con datos del servidor
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error guardando CategoriaActividad '{categoriaActividad.NombreCategoriaActividad}' en Supabase.");
                    localItem.IsSynced = false; // Marcar como no sincronizado si falla
                }
            }
            await _localDatabase.SaveCategoriaActividadAsync(localItem); //
        }

        // --- Implementación para Actividades ---
        public async Task<IEnumerable<Actividad>> GetActividadesAsync(long? categoriaActividadId = null, string searchText = null)
        {
            Debug.WriteLine($"[OFDS.GetActividadesAsync] Solicitado con CategoriaId: {categoriaActividadId}, SearchText: \"{searchText}\"");
            try
            {
                List<LocalActividad> localItems;
                if (categoriaActividadId.HasValue)
                {
                    localItems = await _localDatabase.GetActividadesByCategoriaIdAsync(categoriaActividadId.Value, searchText);
                }
                else
                {
                    localItems = await _localDatabase.GetActividadesAsync(searchText);
                }
                var dtos = localItems.ToDtoList();
                Debug.WriteLine($"[OFDS.GetActividadesAsync] {dtos.Count} actividades encontradas localmente con filtros.");

                if (_connectivity.NetworkAccess == NetworkAccess.Internet)
                {
                    Debug.WriteLine("[OFDS.GetActividadesAsync] Conectado a Internet. Consultando Supabase...");
                    try
                    {
                        // Iniciar la construcción de la consulta
                        var queryBuilder = _supabaseClient.From<SupabaseActividad>();

                        // Aplicar filtros condicionalmente
                        if (categoriaActividadId.HasValue)
                        {
                            queryBuilder = (ISupabaseTable<SupabaseActividad, Supabase.Realtime.RealtimeChannel>)queryBuilder.Filter("categoria_actividad_id", Postgrest.Constants.Operator.Equals, categoriaActividadId.Value.ToString());
                        }
                        if (!string.IsNullOrWhiteSpace(searchText))
                        {
                            // Usar 'ilike' para búsqueda insensible a mayúsculas/minúsculas
                            // El método 'Like' en el cliente de C# para Postgrest usualmente es 'ilike' por defecto
                            // o tiene una opción para ello. Si no, PostgREST soporta 'ilike'.
                            // Asegúrate que la columna nombre_actividad existe en tu tabla SupabaseActividad
                            queryBuilder = (ISupabaseTable<SupabaseActividad, Supabase.Realtime.RealtimeChannel>)queryBuilder.Filter("nombre_actividad", Postgrest.Constants.Operator.ILike, $"%{searchText}%");
                        }

                        // Ejecutar la consulta
                        var response = await queryBuilder.Get();
                        var remoteSupabaseActividades = response.Models;
                        var remoteDtos = remoteSupabaseActividades.ToDtoList();
                        Debug.WriteLine($"[OFDS.GetActividadesAsync] Supabase devolvió {remoteDtos.Count} actividades.");

                        // Lógica de Sincronización...
                        foreach (var remoteDto in remoteDtos)
                        {
                            // var localEquivalent = await _localDatabase.GetActividadByIdAsync(remoteDto.IdActividad); // Ya no es necesario si SaveActividadAsync maneja upsert
                            var localToSave = remoteDto.ToLocal(isSynced: true);
                            await _localDatabase.SaveActividadAsync(localToSave);
                        }

                        // Recargar desde local después de la sincronización
                        if (categoriaActividadId.HasValue)
                        {
                            localItems = await _localDatabase.GetActividadesByCategoriaIdAsync(categoriaActividadId.Value, searchText);
                        }
                        else
                        {
                            localItems = await _localDatabase.GetActividadesAsync(searchText);
                        }
                        dtos = localItems.ToDtoList();
                        Debug.WriteLine($"[OFDS.GetActividadesAsync] {dtos.Count} actividades después de la sincronización y recarga local.");

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"[OFDS.GetActividadesAsync] Error sincronizando Actividades desde Supabase. CategoriaId: {categoriaActividadId}, Search: \"{searchText}\".");
                        Debug.WriteLine($"[OFDS.GetActividadesAsync] Error Supabase: {ex.Message} - StackTrace: {ex.StackTrace}");
                    }
                }
                else
                {
                    Debug.WriteLine("[OFDS.GetActividadesAsync] Sin conexión a Internet. Usando solo datos locales.");
                }

                var todasCategorias = await GetCategoriasActividadAsync();
                var todasUnidades = await GetUniMedReAsync();

                foreach (var actividadDto in dtos)
                {
                    if (actividadDto.CategoriaActividadId.HasValue)
                    {
                        actividadDto.CategoriaActividad = todasCategorias.FirstOrDefault(cat => cat.IdCategoriaActividad == actividadDto.CategoriaActividadId.Value);
                    }
                    if (actividadDto.UnidadMedidaId.HasValue)
                    {
                        actividadDto.UnidadMedida = todasUnidades.FirstOrDefault(u => u.Id == actividadDto.UnidadMedidaId.Value);
                    }
                }

                Debug.WriteLine($"[OFDS.GetActividadesAsync] Retornando {dtos.Count()} DTOs ordenados.");
                return dtos.OrderBy(a => a.NombreActividad);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[OFDS.GetActividadesAsync] Error general obteniendo Actividades.");
                Debug.WriteLine($"[OFDS.GetActividadesAsync] Error General: {ex.Message} - StackTrace: {ex.StackTrace}");
                return Enumerable.Empty<Actividad>();
            }
        }

        public async Task<Actividad> GetActividadByIdAsync(long actividadId)
        {
            try
            {
                var localItem = await _localDatabase.GetActividadByIdAsync(actividadId); //
                var dto = localItem?.ToDto();

                if (dto != null && dto.CategoriaActividadId.HasValue)
                {
                    var categorias = await GetCategoriasActividadAsync(); //
                    dto.CategoriaActividad = categorias.FirstOrDefault(c => c.IdCategoriaActividad == dto.CategoriaActividadId.Value);
                }

                // Podrías añadir lógica para buscar en Supabase si no se encuentra localmente o si está desactualizado
                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error obteniendo Actividad por ID: {actividadId}.");
                return null;
            }
        }

        public async Task<Actividad> SaveActividadAsync(Actividad actividad)
        {
            // Este método es crucial para cuando el usuario escribe una nueva actividad.
            var localItem = actividad.ToLocal(isSynced: false);
            Actividad syncedDto = null;

            if (_connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                try
                {
                    // Usar ToSupabaseForUpsert para asegurar que IdActividad sea 0 si es nueva
                    var supabaseItem = actividad.ToSupabaseForUpsert();

                    var response = await _supabaseClient.From<SupabaseActividad>().Upsert(supabaseItem);
                    var syncedSupabaseItem = response.Models.FirstOrDefault();

                    if (syncedSupabaseItem != null)
                    {
                        syncedDto = syncedSupabaseItem.ToDto();
                        // Actualizar el DTO original con el ID del servidor
                        actividad.IdActividad = syncedDto.IdActividad;
                        actividad.CreatedAt = syncedDto.CreatedAt; // Y cualquier otro campo generado por el servidor

                        localItem = actividad.ToLocal(isSynced: true);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error guardando Actividad '{actividad.NombreActividad}' en Supabase.");
                    localItem.IsSynced = false;
                }
            }

            // Si el localItem aún no tiene un ID (porque era nuevo y la sincronización falló o estaba offline),
            // SaveActividadAsync en LocalDatabaseService necesita manejar la generación de un ID local si es necesario
            // o asumir que ya tiene un ID (posiblemente temporal si se permite creación offline completa con IDs temporales).
            // El mapper ToLocal actualmente usa actividad.IdActividad, que sería 0 para un nuevo ítem no sincronizado.
            // Si IdActividad en LocalActividad es PK y no autoincremental, y esperamos que Supabase lo genere,
            // entonces solo guardamos localmente DESPUÉS de una sincronización exitosa.
            // Si queremos soporte offline completo para nuevas actividades, LocalActividad necesitaría
            // un LocalId (PK, autoincrement) y un ServerId (IdActividad de Supabase).
            // Por ahora, con la estructura actual de LocalActividad.IdActividad siendo PK:
            if (localItem.IdActividad == 0 && syncedDto == null) // Nuevo, offline o sincronización falló sin ID
            {
                _logger.LogWarning($"No se pudo guardar la nueva actividad '{localItem.NombreActividad}' localmente sin un ID del servidor, ya que IdActividad es PK.");
                // No se puede guardar localmente si IdActividad es la PK y es 0, a menos que sea autoincremental (no lo es).
                // Esto requeriría cambiar LocalActividad.IdActividad a autoincremental y añadir un ServerId.
                // O, el usuario no puede crear actividades si está offline.
                // O, se genera un ID temporal negativo para guardarlo localmente y se resuelve en la sincronización.
                // Por ahora, solo se guardará si se obtuvo un ID del servidor.
                if (syncedDto != null)
                { // Solo devolver el DTO si se sincronizó y tiene ID
                    return syncedDto;
                }
                return null; // Indicar fallo si no se pudo obtener ID del servidor
            }

            await _localDatabase.SaveActividadAsync(localItem); //

            return syncedDto ?? actividad; // Devolver el DTO sincronizado si está disponible, sino el original (que ahora podría tener ID)
        }

        // --- RegistroRecursoUti methods ---
        public async Task<IEnumerable<RegistroRecursoUti>> GetRegistrosRecursosUtiBySubEtapaIdAsync(long subEtapaId)
        {
            try
            {
                if (_connectivity.NetworkAccess == NetworkAccess.Internet)
                {
                    var response = await _supabaseClient.From<SupabaseRegistroRecursoUti>()
                                                        .Filter("id_sub_etapa", Postgrest.Constants.Operator.Equals, subEtapaId.ToString())
                                                        .Get();
                    var dtos = response.Models.ToDtoList();

                    var allRecursos = await GetRecursosAsync();
                    var allUniMedRe = await GetUniMedReAsync();
                    foreach (var dto in dtos)
                    {
                        dto.Recurso = allRecursos.FirstOrDefault(r => r.Id == dto.IdRecurso);
                        dto.UniMedRe = allUniMedRe.FirstOrDefault(u => u.Id == dto.IdUniMedida);
                    }
                    return dtos;
                }
                else
                {
                    var localItems = await _localDatabase.GetRegistrosRecursosUtiBySubEtapaIdAsync(subEtapaId);
                    return localItems.ToDtoList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo registros de recursos utilizados.");
                return Enumerable.Empty<RegistroRecursoUti>();
            }
        }

        public async Task<RegistroRecursoUti> SaveRegistroRecursoUtiAsync(RegistroRecursoUti registro)
        {
            var local = registro.ToLocal();
            local.IsSynced = false;

            if (registro.CantidadRecursosUti.HasValue && registro.PrecioUniRecursosUti.HasValue)
            {
                local.TotalRecursosUti = registro.CantidadRecursosUti.Value * registro.PrecioUniRecursosUti.Value;
                registro.TotalRecursosUti = local.TotalRecursosUti;
            }

            await _localDatabase.SaveRegistroRecursoUtiAsync(local);

            if (_connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                try
                {
                    var supabaseModel = registro.ToSupabase();
                    supabaseModel.IdRegistroRecursoUti = 0;

                    var response = await _supabaseClient.From<SupabaseRegistroRecursoUti>().Insert(supabaseModel);
                    var synced = response.Models.FirstOrDefault();

                    if (synced != null)
                    {
                        local.ServerId = synced.IdRegistroRecursoUti;
                        local.IsSynced = true;
                        await _localDatabase.SaveRegistroRecursoUtiAsync(local);
                        return synced.ToDto();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error guardando registro de recurso utilizado en Supabase.");
                }
            }
            return registro;
        }

        public async Task DeleteRegistroRecursoUtiAsync(long registroId)
        {
            if (_connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                await _supabaseClient.From<SupabaseRegistroRecursoUti>()
                                         .Filter("id_registro_recurso_uti", Postgrest.Constants.Operator.Equals, registroId)
                                         .Delete();
            }
        }
    }
}
