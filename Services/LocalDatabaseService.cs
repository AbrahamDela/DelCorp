using DelCorp.Models.Local;
using SQLite;
using System.Diagnostics;

namespace DelCorp.Services;

public class LocalDatabaseService : IDisposable
{
    private SQLiteAsyncConnection _database;

    public LocalDatabaseService()
    {
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "delcorp.db3");
        _database = new SQLiteAsyncConnection(dbPath);

        // Crear tablas necesarias
        _database.CreateTableAsync<LocalProject>().Wait();
        _database.CreateTableAsync<LocalPresupuesto>().Wait();
        _database.CreateTableAsync<LocalEtapa>().Wait();
        _database.CreateTableAsync<LocalSubEtapa>().Wait();
        _database.CreateTableAsync<LocalCategoriaRec>().Wait();
        _database.CreateTableAsync<LocalUniMedRe>().Wait();
        _database.CreateTableAsync<LocalRecurso>().Wait();
        _database.CreateTableAsync<LocalRecursoUti>().Wait();
        _database.CreateTableAsync<LocalCategoriaActividad>().Wait();
        _database.CreateTableAsync<LocalActividad>().Wait();
    }

    public async Task ClearSessionAsync()
    {
        await _database.ExecuteAsync("UPDATE Sessions SET IsActive = 0");
    }

    // Método para cerrar sesión
    public async Task LogoutAsync()
    {
        await ClearSessionAsync();
    }

    public async Task<List<LocalProject>> GetProjectsAsync()
    {
        return await _database.Table<LocalProject>().ToListAsync();
    }
    // Método auxiliar para obtener proyectos locales paginados
    public async Task<List<LocalProject>> GetLocalPagedProjects(int page, int pageSize)
    {
        var allProjects = await GetProjectsAsync();
        return allProjects
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    public async Task<List<LocalProject>> GetUnsyncedProjectsAsync()
    {
        return await _database.Table<LocalProject>().Where(p => !p.IsSynced).ToListAsync();
    }

    public async Task<LocalProject> GetProjectByServerIdAsync(int serverId)
    {
        return await _database.Table<LocalProject>().Where(p => p.ServerId == serverId).FirstOrDefaultAsync();
    }

    public async Task<LocalProject> GetProjectAsync(int id)
    {
        return await _database.Table<LocalProject>().Where(p => p.Id == id).FirstOrDefaultAsync();
    }

    public async Task<int> SaveProjectAsync(LocalProject project)
    {
        if (project.Id != 0)
        {
            return await _database.UpdateAsync(project);
        }
        else
        {
            return await _database.InsertAsync(project);
        }
    }

    public async Task<int> SaveProjectsAsync(List<LocalProject> projects)
    {
        return await _database.InsertAllAsync(projects, typeof(LocalProject));
    }

    public async Task<int> DeleteProjectAsync(LocalProject project)
    {
        return await _database.DeleteAsync(project);
    }

    public async Task<int> DeleteProjectByServerIdAsync(int serverId)
    {
        var project = await GetProjectByServerIdAsync(serverId);
        if (project != null)
        {
            return await _database.DeleteAsync(project);
        }
        return 0;
    }

    public async Task<int> MarkAsSyncedAsync(LocalProject project)
    {
        project.IsSynced = true;
        return await _database.UpdateAsync(project);
    }

    public async Task<List<LocalPresupuesto>> GetPresupuestosByProjectIdAsync(int projectId)
    {
        return await _database.Table<LocalPresupuesto>()
            .Where(p => p.IdProyecto == projectId)
            .ToListAsync();
    }

    public async Task<LocalPresupuesto> GetPresupuestoByServerIdAsync(long serverId)
    {
        return await _database.Table<LocalPresupuesto>()
            .FirstOrDefaultAsync(p => p.ServerId == serverId);
    }

    public async Task<LocalPresupuesto> GetPresupuestoByIdAsync(long id)
    {
        return await _database.Table<LocalPresupuesto>()
            .FirstOrDefaultAsync(p => p.Id == (int)id || p.ServerId == id);
    }

    public async Task SavePresupuestoAsync(LocalPresupuesto presupuesto)
    {
        // Verifica si el presupuesto ya tiene un ID local.
        if (presupuesto.Id != 0)
        {
            // Si tiene un ID, actualiza el registro existente.
            await _database.UpdateAsync(presupuesto);
        }
        else
        {
            // Si no tiene ID (es 0), inserta un nuevo registro.
            await _database.InsertAsync(presupuesto);
        }
    }

    public void Dispose()
    {
        _database?.CloseAsync().Wait();
    }


    //Etapas de presupuesto
    // Métodos para etapas
    public async Task<List<LocalEtapa>> GetEtapasByPresupuestoIdAsync(long presupuestoId)
    {
        System.Diagnostics.Debug.WriteLine($"[GetEtapasByPresupuestoIdAsync] Retornar etapas localmente del presupuesto ID: {presupuestoId}");
        return await _database.Table<LocalEtapa>()
            .Where(e => e.IdPresupuesto == presupuestoId)
            .ToListAsync();
    }

    public async Task<LocalEtapa> GetEtapaByIdAsync(long etapaId)
    {
        return await _database.Table<LocalEtapa>()
            .FirstOrDefaultAsync(e => e.Id == etapaId);
    }

    public async Task<LocalEtapa> GetEtapaByServerIdAsync(long serverId)
    {
        return await _database.Table<LocalEtapa>()
            .FirstOrDefaultAsync(e => e.ServerId == serverId);
    }

    public async Task SaveEtapaAsync(LocalEtapa etapa)
    {
        // Try to update existing records when possible to avoid duplicates
        if (etapa.Id != 0)
        {
            await _database.UpdateAsync(etapa);
            return;
        }

        if (etapa.ServerId.HasValue)
        {
            var existing = await _database.Table<LocalEtapa>()
                .FirstOrDefaultAsync(e => e.ServerId == etapa.ServerId.Value);
            if (existing != null)
            {
                etapa.Id = existing.Id;
                await _database.UpdateAsync(etapa);
                return;
            }
        }

        await _database.InsertAsync(etapa);
    }

    public async Task DeleteEtapaAsync(LocalEtapa etapa)
    {
        await _database.DeleteAsync(etapa);
    }

    public async Task DeleteEtapasByPresupuestoIdAsync(long presupuestoId)
    {
        var etapasToDelete = await _database.Table<LocalEtapa>()
            .Where(e => e.IdPresupuesto == presupuestoId)
            .ToListAsync();

        foreach (var etapa in etapasToDelete)
        {
            await _database.DeleteAsync(etapa);
        }
    }

    public async Task<List<LocalPresupuesto>> GetAllPresupuestosAsync()
    {
        return await _database.Table<LocalPresupuesto>().ToListAsync();
    }

    public async Task DeletePresupuestoAsync(LocalPresupuesto presupuesto)
    {
        if (presupuesto == null)
            return;

        await _database.DeleteAsync(presupuesto);
    }

    public async Task<List<LocalSubEtapa>> GetSubEtapasByEtapaIdAsync(long etapaId)
    {
        System.Diagnostics.Debug.WriteLine($"[GetSubEtapasByEtapaIdAsync] Retornar Sub Etapas localmente de la etapa ID: {etapaId}");
        return await _database.Table<LocalSubEtapa>().Where(x => x.IdEtapa == etapaId).ToListAsync();
    }

    public async Task<LocalSubEtapa> GetLocalSubEtapaByIdAsync(long id)
    {
        return await _database.Table<LocalSubEtapa>()
                             .Where(s => s.ServerId == id || s.Id == id)
                             .FirstOrDefaultAsync();
    }
    public async Task SaveSubEtapaAsync(LocalSubEtapa subEtapa)
    {
        if (subEtapa.Id != 0)
        {
            await _database.UpdateAsync(subEtapa);
            return;
        }

        if (subEtapa.ServerId.HasValue)
        {
            var existing = await _database.Table<LocalSubEtapa>()
                .FirstOrDefaultAsync(x => x.ServerId == subEtapa.ServerId.Value);
            if (existing != null)
            {
                subEtapa.Id = existing.Id;
                await _database.UpdateAsync(subEtapa);
                return;
            }
        }

        await _database.InsertAsync(subEtapa);
    }

    public async Task DeleteSubEtapasByEtapaIdAsync(long etapaId)
    {
        var itemsToDelete = await _database.Table<LocalSubEtapa>().Where(x => x.IdEtapa == etapaId).ToListAsync();
        int countDeleted = 0;
        foreach (var item in itemsToDelete)
        {
            await _database.DeleteAsync(item);
            countDeleted++;
        }
        System.Diagnostics.Debug.WriteLine($"[LocalDatabaseService.DeleteSubEtapasByEtapaIdAsync] Deleted {countDeleted} subetapas for EtapaId: {etapaId}");
    }

    // CategoriaRec
    public async Task<List<LocalCategoriaRec>> GetCategoriasRecAsync() => await _database.Table<LocalCategoriaRec>().ToListAsync();

    public async Task SaveCategoriaRecAsync(LocalCategoriaRec item)
    {
        var existing = await _database.Table<LocalCategoriaRec>().FirstOrDefaultAsync(x => x.ServerId == item.ServerId);
        if (existing != null)
        {
            item.LocalId = existing.LocalId; // Preserve local PK
            await _database.UpdateAsync(item);
        }
        else
        {
            item.LocalId = 0; // Ensure new local PK for insert
            await _database.InsertAsync(item);
        }
    }
    public async Task ClearCategoriasRecAsync() => await _database.DeleteAllAsync<LocalCategoriaRec>();


    // UniMedRe
    public async Task<List<LocalUniMedRe>> GetUniMedReAsync() => await _database.Table<LocalUniMedRe>().ToListAsync();

    public async Task SaveUniMedReAsync(LocalUniMedRe item)
    {
        var existing = await _database.Table<LocalUniMedRe>().FirstOrDefaultAsync(x => x.ServerId == item.ServerId);
        if (existing != null)
        {
            item.LocalId = existing.LocalId; // Preserve local PK
            await _database.UpdateAsync(item);
        }
        else
        {
            item.LocalId = 0; // Ensure new local PK for insert
            await _database.InsertAsync(item);
        }
    }
    public async Task ClearUniMedReAsync() => await _database.DeleteAllAsync<LocalUniMedRe>();


    // Recurso
    public async Task<List<LocalRecurso>> GetRecursosAsync(long? idCategoriaRec = null)
    {
        var query = _database.Table<LocalRecurso>();
        if (idCategoriaRec.HasValue)
        {
            query = query.Where(r => r.IdCatRec == idCategoriaRec.Value);
        }
        return await query.ToListAsync();
    }

    public async Task SaveRecursoAsync(LocalRecurso item)
    {
        var existing = await _database.Table<LocalRecurso>().FirstOrDefaultAsync(x => x.ServerId == item.ServerId);
        if (existing != null)
        {
            item.LocalId = existing.LocalId; // Preserve local PK
            await _database.UpdateAsync(item);
        }
        else
        {
            item.LocalId = 0; // Ensure new local PK for insert
            await _database.InsertAsync(item);
        }
    }
    public async Task ClearRecursosAsync(long? idCategoriaRec = null)
    {
        if (idCategoriaRec.HasValue)
        {
            var itemsToDelete = await _database.Table<LocalRecurso>().Where(r => r.IdCatRec == idCategoriaRec.Value).ToListAsync();
            foreach (var item in itemsToDelete) await _database.DeleteAsync(item);
        }
        else
        {
            await _database.DeleteAllAsync<LocalRecurso>();
        }
    }


    // RecursoUti
    public async Task<List<LocalRecursoUti>> GetRecursosUtiBySubEtapaIdAsync(long subEtapaId) =>
        await _database.Table<LocalRecursoUti>().Where(r => r.IdSubEtapa == subEtapaId).ToListAsync();

    public async Task<LocalRecursoUti> GetLocalRecursoUtiByServerIdAsync(long serverId) =>
        await _database.Table<LocalRecursoUti>().FirstOrDefaultAsync(r => r.ServerId == serverId);

    public async Task<LocalRecursoUti> GetLocalRecursoUtiByLocalIdAsync(long localId) =>
        await _database.Table<LocalRecursoUti>().FirstOrDefaultAsync(r => r.LocalId == localId);


    public async Task SaveRecursoUtiAsync(LocalRecursoUti item)
    {
        if (item.LocalId != 0)
        {
            await _database.UpdateAsync(item);
        }
        else if (item.ServerId.HasValue)
        {
            var existingByServer = await GetLocalRecursoUtiByServerIdAsync(item.ServerId.Value);
            if (existingByServer != null)
            {
                item.LocalId = existingByServer.LocalId;
                await _database.UpdateAsync(item);
            }
            else
            {
                item.LocalId = 0; // Ensure new LocalId for insert
                await _database.InsertAsync(item);
            }
        }
        else
        {
            item.LocalId = 0; // Ensure new LocalId for insert
            await _database.InsertAsync(item);
        }
    }

    public async Task DeleteRecursoUtiByLocalIdAsync(long localId)
    {
        var itemByLocalId = await _database.Table<LocalRecursoUti>().FirstOrDefaultAsync(x => x.LocalId == localId);
        if (itemByLocalId != null) await _database.DeleteAsync(itemByLocalId);
    }

    public async Task<List<LocalRecursoUti>> GetUnsyncedRecursosUtiAsync() =>
        await _database.Table<LocalRecursoUti>().Where(r => !r.IsSynced).ToListAsync();

    // Para LocalCategoriaActividad
    public async Task<List<LocalCategoriaActividad>> GetCategoriasActividadAsync() =>
        await _database.Table<LocalCategoriaActividad>().ToListAsync();

    public async Task SaveCategoriaActividadAsync(LocalCategoriaActividad item)
    {
        var existing = await _database.Table<LocalCategoriaActividad>()
                                     .FirstOrDefaultAsync(x => x.IdCategoriaActividad == item.IdCategoriaActividad);
        if (existing != null)
        {
            await _database.UpdateAsync(item);
        }
        else
        {
            await _database.InsertAsync(item);
        }
    }
    public async Task ClearCategoriasActividadAsync() => await _database.DeleteAllAsync<LocalCategoriaActividad>();


    // Para LocalActividad
    public async Task<List<LocalActividad>> GetActividadesAsync(string searchText = null) // Modificado
    {
        Debug.WriteLine($"[LocalDB.GetActividadesAsync] SearchText: {searchText}");
        var query = _database.Table<LocalActividad>();
        if (!string.IsNullOrWhiteSpace(searchText))
        {
            string searchTextLower = searchText.ToLower();
            // SQLite no tiene ToLower() directamente en LINQ to SQLite así,
            // tendrás que traer los datos y filtrar en memoria o usar una query SQL cruda
            // o asegurarte que los datos se guardan en un formato consistente para la búsqueda.
            // Una forma simple (pero no la más eficiente para grandes datasets) es filtrar en memoria:
            var allActivities = await query.ToListAsync();
            return allActivities.Where(a => a.NombreActividad.ToLower().Contains(searchTextLower)).ToList();
        }
        return await query.ToListAsync();
    }

    public async Task<List<LocalActividad>> GetActividadesByCategoriaIdAsync(long categoriaId, string searchText = null) // Modificado
    {
        Debug.WriteLine($"[LocalDB.GetActividadesByCategoriaIdAsync] CategoriaId: {categoriaId}, SearchText: {searchText}");
        var query = _database.Table<LocalActividad>().Where(a => a.CategoriaActividadId == categoriaId);
        // Aplicar filtro de texto si se provee
        if (!string.IsNullOrWhiteSpace(searchText))
        {
            string searchTextLower = searchText.ToLower();
            // Mismo comentario sobre ToLower() que en GetActividadesAsync
            var activitiesInCateogry = await query.ToListAsync();
            return activitiesInCateogry.Where(a => a.NombreActividad.ToLower().Contains(searchTextLower)).ToList();
        }
        return await query.ToListAsync();
    }

    public async Task<LocalActividad> GetActividadByIdAsync(long idActividad) =>
        await _database.Table<LocalActividad>().FirstOrDefaultAsync(a => a.IdActividad == idActividad);

    public async Task SaveActividadAsync(LocalActividad item)
    {
        var existing = await _database.Table<LocalActividad>()
                                     .FirstOrDefaultAsync(x => x.IdActividad == item.IdActividad);
        if (existing != null)
        {
            // Si es una actualización y el ID es generado por Supabase, el IdActividad ya debería estar.
            // Si permites crear localmente con un ID temporal, necesitarás lógica para manejar eso al sincronizar.
            await _database.UpdateAsync(item);
        }
        else
        {
            // Si el ID es generado por Supabase, este insert debería ocurrir después de obtener el ID del servidor.
            // Si permites crear localmente y luego sincronizar, necesitarás una PK local autoincremental y un ServerId.
            // Por ahora, asumimos que IdActividad viene del servidor o se genera de forma única antes de guardar localmente.
            await _database.InsertAsync(item);
        }
    }
    public async Task ClearActividadesAsync() => await _database.DeleteAllAsync<LocalActividad>();
}
