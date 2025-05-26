using DelCorp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DelCorp.Services
{
    public interface IDataService
    {
        //Proyectos
        Task<List<Project>> GetProjects();
        Task<List<Project>> GetPagedProjects(int page, int pageSize);
        Task<Project> GetProject(int id);
        Task<bool> SaveProject(Project project);
        Task<bool> DeleteProject(int id);
        Task<bool> SyncProjects();

        //Presupuestos
        Task<IEnumerable<Presupuesto>> GetPresupuestosByProjectId(int projectId);
        Task<IEnumerable<Presupuesto>> GetAllPresupuestos();
        Task<bool> DeletePresupuesto(long presupuestoId);


        //Etapas de presupuestos
        Task<IEnumerable<Etapa>> GetEtapasByPresupuestoId(int presupuestoId);
        Task<Etapa> SaveEtapa(Etapa etapa);
        Task DeleteEtapa(long etapaId);
        Task<Presupuesto> SavePresupuesto(Presupuesto presupuesto);

        //SubEtapas de presupuestos
        Task<IEnumerable<SubEtapa>> GetSubEtapasByEtapaId(long etapaId);
        Task SaveSubEtapa(SubEtapa subEtapa);

        // CategoriaRec
        Task<IEnumerable<CategoriaRec>> GetCategoriasRecAsync();
        Task SaveCategoriaRecAsync(CategoriaRec categoria); // For admin purposes if needed

        // UniMedRe
        Task<IEnumerable<UniMedRe>> GetUniMedReAsync();
        Task SaveUniMedReAsync(UniMedRe uniMedRe); // For admin purposes if needed

        // Recurso
        Task<IEnumerable<Recurso>> GetRecursosAsync(long? idCategoriaRec = null);
        Task SaveRecursoAsync(Recurso recurso); // For admin purposes if needed

        // RecursoUti
        Task<IEnumerable<RecursoUti>> GetRecursosUtiBySubEtapaIdAsync(long subEtapaId);
        Task<RecursoUti> SaveRecursoUtiAsync(RecursoUti recursoUti);
        Task DeleteRecursoUtiAsync(long recursoUtiId);
    }
}