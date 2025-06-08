using System.Threading.Tasks;
using DelCorp.ViewModels;
using DelCorp.Services;
using DelCorp.Models;
using Xunit;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;

namespace DelCorp.Tests;

public class ManageBudgetViewModelTests
{
    private class StubDataService : IDataService
    {
        public Task<List<Project>> GetProjects() => Task.FromResult(new List<Project>());
        public Task<List<Project>> GetPagedProjects(int page, int pageSize) => Task.FromResult(new List<Project>());
        public Task<Project> GetProject(int id) => Task.FromResult(new Project());
        public Task<bool> SaveProject(Project project) => Task.FromResult(true);
        public Task<bool> DeleteProject(int id) => Task.FromResult(true);
        public Task<bool> SyncProjects() => Task.FromResult(true);
        public Task<IEnumerable<Presupuesto>> GetPresupuestosByProjectId(int projectId) => Task.FromResult<IEnumerable<Presupuesto>>(new List<Presupuesto>());
        public Task<IEnumerable<Presupuesto>> GetAllPresupuestos() => Task.FromResult<IEnumerable<Presupuesto>>(new List<Presupuesto>());
        public Task<bool> DeletePresupuesto(long presupuestoId) => Task.FromResult(true);
        public Task<IEnumerable<Etapa>> GetEtapasByPresupuestoId(int presupuestoId) => Task.FromResult<IEnumerable<Etapa>>(new List<Etapa>());
        public Task<Etapa> SaveEtapa(Etapa etapa) => Task.FromResult(etapa);
        public Task DeleteEtapa(long etapaId) => Task.CompletedTask;
        public Task<Presupuesto> SavePresupuesto(Presupuesto presupuesto) => Task.FromResult(presupuesto);
        public Task<IEnumerable<SubEtapa>> GetSubEtapasByEtapaId(long etapaId) => Task.FromResult<IEnumerable<SubEtapa>>(new List<SubEtapa>());
        public Task SaveSubEtapa(SubEtapa subEtapa) => Task.CompletedTask;
        public Task<SubEtapa> GetSubEtapaByIdAsync(long subEtapaId) => Task.FromResult(new SubEtapa());
        public Task<IEnumerable<CategoriaRec>> GetCategoriasRecAsync() => Task.FromResult<IEnumerable<CategoriaRec>>(new List<CategoriaRec>());
        public Task SaveCategoriaRecAsync(CategoriaRec categoria) => Task.CompletedTask;
        public Task<IEnumerable<UniMedRe>> GetUniMedReAsync() => Task.FromResult<IEnumerable<UniMedRe>>(new List<UniMedRe>());
        public Task SaveUniMedReAsync(UniMedRe uniMedRe) => Task.CompletedTask;
        public Task<IEnumerable<Recurso>> GetRecursosAsync(long? idCategoriaRec = null) => Task.FromResult<IEnumerable<Recurso>>(new List<Recurso>());
        public Task SaveRecursoAsync(Recurso recurso) => Task.CompletedTask;
        public Task<IEnumerable<RecursoUti>> GetRecursosUtiBySubEtapaIdAsync(long subEtapaId) => Task.FromResult<IEnumerable<RecursoUti>>(new List<RecursoUti>());
        public Task<RecursoUti> SaveRecursoUtiAsync(RecursoUti recursoUti) => Task.FromResult(recursoUti);
        public Task DeleteRecursoUtiAsync(long recursoUtiId) => Task.CompletedTask;
        public Task<IEnumerable<CategoriaActividad>> GetCategoriasActividadAsync() => Task.FromResult<IEnumerable<CategoriaActividad>>(new List<CategoriaActividad>());
        public Task SaveCategoriaActividadAsync(CategoriaActividad categoriaActividad) => Task.CompletedTask;
        public Task<IEnumerable<Actividad>> GetActividadesAsync(long? categoriaActividadId = null, string searchText = null) => Task.FromResult<IEnumerable<Actividad>>(new List<Actividad>());
        public Task<Actividad> GetActividadByIdAsync(long actividadId) => Task.FromResult(new Actividad());
        public Task<Actividad> SaveActividadAsync(Actividad actividad) => Task.FromResult(actividad);
    }

    [Fact]
    public async Task AddEtapa_InvalidNumeroEtapa_DoesNotThrow()
    {
        var service = new StubDataService();
        var vm = new ManageBudgetViewModel(service)
        {
            CurrentPresupuesto = new Presupuesto { Id = 1 },
            ActividadEtapa = "foo",
            CantidadEtapa = 1,
            NumeroEtapa = "invalid"
        };

        Application.Current ??= new Application();
        Shell.SetCurrent(new Shell());

        var ex = await Record.ExceptionAsync(() => vm.AddEtapaCommand.ExecuteAsync(null));
        Assert.Null(ex);
        Assert.Empty(vm.Etapas);
    }
}
