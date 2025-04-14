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
        _database.CreateTableAsync<LocalSession>().Wait();
    }

    // Métodos para manejar sesiones
    public async Task<LocalSession> GetActiveSessionAsync()
    {
        return await _database.Table<LocalSession>()
            .Where(s => s.IsActive)
            .FirstOrDefaultAsync();
    }

    public async Task<int> SaveSessionAsync(LocalSession session)
    {
        // Invalidar sesiones anteriores
        await _database.ExecuteAsync("UPDATE Sessions SET IsActive = 0");

        // Guardar nueva sesión
        return await _database.InsertAsync(session);
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

    public void Dispose()
    {
        _database?.CloseAsync().Wait();
    }
}
