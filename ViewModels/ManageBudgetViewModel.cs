using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DelCorp.Models;
using DelCorp.Services;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace DelCorp.ViewModels;

public partial class ManageBudgetViewModel : ObservableObject, IQueryAttributable
{
    private readonly IDataService _dataService;

    [ObservableProperty]
    private Project _project;

    [ObservableProperty]
    private Presupuesto _currentPresupuesto;

    [ObservableProperty]
    private ObservableCollection<Etapa> _etapas;

    // Propiedades para agregar nueva etapa
    [ObservableProperty]
    private string _numeroEtapa;

    [ObservableProperty]
    private string _actividadEtapa;

    [ObservableProperty]
    private string _unidadMedida;

    [ObservableProperty]
    private decimal? _cantidadEtapa;

    [ObservableProperty]
    private string _estadoPresupuesto;

    [ObservableProperty]
    private DateTime? _fechaInicioPresupuesto;

    [ObservableProperty]
    private DateTime? _fechaFinPresupuesto;

    public ManageBudgetViewModel(IDataService dataService)
    {
        _dataService = dataService;
        Etapas = new ObservableCollection<Etapa>();
    }

    public async void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.ContainsKey("id"))
        {
            var projectId = int.Parse(query["id"].ToString());
            await LoadProject(projectId);
            await LoadPresupuesto(projectId);
        }
    }

    private async Task LoadProject(int id)
    {
        Project = await _dataService.GetProject(id);

        if (Project == null)
        {
            await Shell.Current.DisplayAlert("Error", "No se encontró el proyecto.", "OK");
            await Shell.Current.GoToAsync("..");
        }
    }

    private async Task LoadPresupuesto(int projectId)
    {
        // Obtener presupuesto existente o crear uno nuevo
        var presupuestos = await _dataService.GetPresupuestosByProjectId(projectId);
        CurrentPresupuesto = presupuestos.FirstOrDefault();

        if (CurrentPresupuesto == null)
        {
            // Crear nuevo presupuesto si no existe
            CurrentPresupuesto = new Presupuesto
            {
                Id = projectId,
                NombrePresupuesto = Project.NombreProyecto,
                CreatedAt = DateTime.Now
            };
        }
        else
        {
            // Cargar etapas existentes
            //await LoadEtapas(CurrentPresupuesto.Id);
        }

        // Establecer valores iniciales
        //EstadoPresupuesto = CurrentPresupuesto.Estado ?? "PENDIENTE";
        FechaInicioPresupuesto = CurrentPresupuesto.FechaInicioPrespuesto;
        FechaFinPresupuesto = CurrentPresupuesto.FechaFinPresupuesto;
    }

    private async Task LoadEtapas(int presupuestoId)
    {
        // Método para cargar etapas (implementar según tu servicio de datos)
        // Esta es una implementación de ejemplo
        var etapas = await _dataService.GetEtapasByPresupuestoId(presupuestoId);

        Etapas.Clear();
        foreach (var etapa in etapas)
        {
            Etapas.Add(etapa);
        }
    }

    [RelayCommand]
    private async Task AddEtapa()
    {
        // Validaciones
        if (string.IsNullOrWhiteSpace(NumeroEtapa))
        {
            await Shell.Current.DisplayAlert("Error", "Número de etapa es obligatorio", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(ActividadEtapa))
        {
            await Shell.Current.DisplayAlert("Error", "Actividad es obligatoria", "OK");
            return;
        }

        if (CantidadEtapa == null || CantidadEtapa <= 0)
        {
            await Shell.Current.DisplayAlert("Error", "Cantidad debe ser mayor a cero", "OK");
            return;
        }

        // Crear nueva etapa
        var nuevaEtapa = new Etapa
        {
            NumeroEtapa = int.Parse(NumeroEtapa),
            //ActividadEtapa = ActividadEtapa,
            //UnidadMedida = UnidadMedida,
            CantidadEtapa = CantidadEtapa,
            IdPresupuesto = (int)CurrentPresupuesto.Id
        };

        try
        {
            // Guardar etapa (implementar método en tu servicio de datos)
            await _dataService.SaveEtapa(nuevaEtapa);

            // Agregar a la colección local
            Etapas.Add(nuevaEtapa);

            // Limpiar campos
            NumeroEtapa = string.Empty;
            ActividadEtapa = string.Empty;
            UnidadMedida = string.Empty;
            CantidadEtapa = null;
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"No se pudo guardar la etapa: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task SavePresupuesto()
    {
        try
        {
            // Actualizar datos del presupuesto
            //CurrentPresupuesto.Estado = EstadoPresupuesto;
            CurrentPresupuesto.FechaInicioPrespuesto = FechaInicioPresupuesto;
            CurrentPresupuesto.FechaFinPresupuesto = FechaFinPresupuesto;

            // Guardar presupuesto
            await _dataService.SavePresupuesto(CurrentPresupuesto);

            await Shell.Current.DisplayAlert("Éxito", "Presupuesto guardado correctamente", "OK");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"No se pudo guardar el presupuesto: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task DeleteEtapa(Etapa etapa)
    {
        bool confirm = await Shell.Current.DisplayAlert(
            "Confirmar",
            "¿Está seguro de eliminar esta etapa?",
            "Sí",
            "No"
        );

        if (confirm)
        {
            try
            {
                await _dataService.DeleteEtapa(etapa.Id);
                Etapas.Remove(etapa);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"No se pudo eliminar la etapa: {ex.Message}", "OK");
            }
        }
    }

    [RelayCommand]
    private async Task GoBack()
    {
        await Shell.Current.GoToAsync("..");
    }
}