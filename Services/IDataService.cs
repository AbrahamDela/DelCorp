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
        Task<Presupuesto> GetPresupuestoByIdAsync(long presupuestoId);


        //Etapas de presupuestos
        Task<IEnumerable<Etapa>> GetEtapasByPresupuestoId(int presupuestoId);
        Task<Etapa> SaveEtapa(Etapa etapa);
        Task DeleteEtapa(long etapaId);
        Task<Presupuesto> SavePresupuesto(Presupuesto presupuesto);

        //SubEtapas de presupuestos
        Task<IEnumerable<SubEtapa>> GetSubEtapasByEtapaId(long etapaId);
        Task SaveSubEtapa(SubEtapa subEtapa);
        Task<SubEtapa> GetSubEtapaByIdAsync(long subEtapaId);

        // CategoriaRec
        Task<IEnumerable<CategoriaRec>> GetCategoriasRecAsync();
        Task SaveCategoriaRecAsync(CategoriaRec categoria);

        // UniMedRe
        Task<IEnumerable<UniMedRe>> GetUniMedReAsync();
        Task SaveUniMedReAsync(UniMedRe uniMedRe);

        // Recurso
        Task<IEnumerable<Recurso>> GetRecursosAsync(long? idCategoriaRec = null);
        Task SaveRecursoAsync(Recurso recurso);

        // RecursoUti
        Task<IEnumerable<RecursoUti>> GetRecursosUtiBySubEtapaIdAsync(long subEtapaId);
        Task<RecursoUti> SaveRecursoUtiAsync(RecursoUti recursoUti);
        Task DeleteRecursoUtiAsync(long recursoUtiId);

        // Actividades categoría
        Task<IEnumerable<CategoriaActividad>> GetCategoriasActividadAsync();
        Task SaveCategoriaActividadAsync(CategoriaActividad categoriaActividad); // Principalmente para admin/sincronización

        // Actividades
        Task<IEnumerable<Actividad>> GetActividadesAsync(long? categoriaActividadId = null, string searchText = null);
        Task<Actividad> GetActividadByIdAsync(long actividadId);
        Task<Actividad> SaveActividadAsync(Actividad actividad); // Para crear nuevas actividades desde la app

        // Registro Recursos Utilizados
        Task<IEnumerable<RegistroRecursoUti>> GetRegistrosRecursosUtiBySubEtapaIdAsync(long subEtapaId);
        Task<RegistroRecursoUti> SaveRegistroRecursoUtiAsync(RegistroRecursoUti registro);
        Task DeleteRegistroRecursoUtiAsync(long registroId);

        // Obtiene la suma total de los recursos utilizados para un presupuesto
        Task<decimal> GetTotalEjecutadoForPresupuestoAsync(long presupuestoId);
    }
}