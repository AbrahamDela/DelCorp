using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DelCorp.Models;
using DelCorp.Services;
using DelCorp.Views;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace DelCorp.ViewModels;

public partial class ProjectDetailViewModel : ObservableObject, IQueryAttributable
{
    private readonly IDataService _dataService;

    [ObservableProperty]
    private bool _visibleMap;

    [ObservableProperty]
    private Project _project;

    [ObservableProperty]
    private ObservableCollection<Presupuesto> _presupuestos;

    public ProjectDetailViewModel(IDataService dataService)
    {
        _dataService = dataService;
        _presupuestos = new ObservableCollection<Presupuesto>();
    }

    public async void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.ContainsKey("id"))
        {
            var projectId = int.Parse(query["id"].ToString());
            await LoadProject(projectId);
        }
    }

    private async Task LoadProject(int id)
    {
        // Obtener el proyecto por ID con el metodo de _dataService
        Project = await _dataService.GetProject(id);

        // Si el proyecto no se encuentra navegar hacia atrás y mostrar un mensaje de error
        if (Project == null)
        {
            await Shell.Current.DisplayAlert("Error", "No se encontró el proyecto.", "OK");
            await Shell.Current.GoToAsync("..");
        }

        // Cargar presupuestos del proyecto
        await LoadPresupuestos(id);

        if (Project.LatitudProyecto != null && Project.LongitudProyecto != null)
        {
            VisibleMap = true;
        }
        else
        {
            VisibleMap = false;
        }
    }

    // Método para abrir ubicación en mapa si está disponible
    [RelayCommand]
    private async Task OpenLocation()
    {
        if (!string.IsNullOrWhiteSpace(Project.LatitudProyecto) &&
            !string.IsNullOrWhiteSpace(Project.LongitudProyecto))
        {
            try
            {
                var location = new Location(
                    double.Parse(Project.LatitudProyecto),
                    double.Parse(Project.LongitudProyecto)
                );

                await Launcher.OpenAsync(new Uri(
                    $"http://maps.google.com/?q={location.Latitude},{location.Longitude}"
                ));
            }
            catch
            {
                await Shell.Current.DisplayAlert(
                    "Error",
                    "No se puede abrir la ubicación.",
                    "OK"
                );
            }
        }
        else
        {
            await Shell.Current.DisplayAlert(
                "Advertencia",
                "La ubicación del proyecto no está disponible.",
                "OK"
            );
        }
    }

    // Método para cargar presupuestos
    private async Task LoadPresupuestos(int projectId)
    {
        try
        {
            // Limpiar la colección existente
            Presupuestos.Clear();

            Debug.WriteLine($"Presupuestos limpio: {Presupuestos.Count()}");

            // Obtener presupuestos desde el servicio de datos
            var presupuestos = await _dataService.GetPresupuestosByProjectId(projectId);

            Debug.WriteLine($"Presupuestos recuperados: {presupuestos.Count()}");

            if(presupuestos != null && presupuestos.Any())
            {
                // Agregar presupuestos a la colección
                foreach (var presupuesto in presupuestos)
                {
                    Presupuestos.Add(presupuesto);
                }

                var anyUnsyncedPresupuestos = Presupuestos.Any(p => !p.IsSynced);
            }

            // Notificar que la colección ha cambiado
            OnPropertyChanged(nameof(Presupuestos));
        }
        catch (Exception ex)
        {
            // Manejar cualquier error de carga
            await Shell.Current.DisplayAlert(
                "Error",
                $"No se pudieron cargar los presupuestos: {ex.Message}",
                "OK"
            );
        }
    }
}