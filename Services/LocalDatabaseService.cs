using DelCorp.Models.Local;
using SQLite;

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
        _database.CreateTableAsync<LocalSubEtapa>();
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

    public async Task SavePresupuestoAsync(LocalPresupuesto presupuesto)
    {
        await _database.InsertAsync(presupuesto);
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

    public async Task SaveSubEtapaAsync(LocalSubEtapa subEtapa)
    {
        await _database.UpdateAsync(subEtapa);
    }

    public async Task DeleteSubEtapasByEtapaIdAsync(long etapaId)
    {
        await _database.Table<LocalSubEtapa>().Where(x => x.IdEtapa == etapaId).DeleteAsync();
    }
}
