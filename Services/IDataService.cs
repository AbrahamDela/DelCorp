using DelCorp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DelCorp.Services
{
    public interface IDataService
    {
        Task<List<Project>> GetProjects();
        Task<List<Project>> GetPagedProjects(int page, int pageSize);
        Task<Project> GetProject(int id);
        Task<bool> SaveProject(Project project);
        Task<bool> DeleteProject(int id);
        Task<bool> SyncProjects();
    }
}