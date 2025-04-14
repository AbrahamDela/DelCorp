using DelCorp.Models;
using DelCorp.Models.Local;
using DelCorp.Models.Supabase;
using DelCorp.Services.Mapping;
using Supabase;
using Microsoft.Extensions.Logging;

namespace DelCorp.Services
{
    public class OfflineFirstDataService : IDataService
    {
        private readonly Client _supabaseClient;
        private readonly LocalDatabaseService _localDatabase;
        private readonly IConnectivity _connectivity;
        private readonly ILogger<OfflineFirstDataService> _logger;

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

            // Si hay conexión, intentar sincronizar y obtener más proyectos
            if (_connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                try
                {
                    // Obtener proyectos del servidor con paginación
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

            // Si hay conexión, sincronizar con el servidor
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
                            // Actualizar solo si está sincronizado (no tiene cambios locales pendientes)
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
                    // Log error (en una implementación real) y continuar con datos locales
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
                // Para proyectos nuevos, establecer fecha de creación
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

                // Si hay conexión, intentar sincronizar con el servidor
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
                    // Sin conexión, marcar como pendiente
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

                    // Si tiene ID de servidor y hay conexión, eliminar en el servidor
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
                            // Error al eliminar en el servidor, pero continuamos
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
    }
}
