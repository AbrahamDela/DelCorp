using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DelCorp.Models;
using DelCorp.Services;
using DelCorp.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DelCorp.ViewModels;

public partial class RegistrarPresupuestoViewModel : ObservableObject
{
    [ObservableProperty] string nombrePresupuesto;
    [ObservableProperty] DateTime fechaInicioPresupuesto = DateTime.Today;
    [ObservableProperty] DateTime fechaFinPresupuesto = DateTime.Today;
    [ObservableProperty] string totalPresupuesto;
    [ObservableProperty] bool isBusy;
    private readonly IDataService _dataService;
    public ObservableCollection<Project> Proyectos { get; } = new();
    public ObservableCollection<Project> ProyectosFiltrados { get; } = new();
    private string _nombreProyectoBusqueda;
    private Project _proyectoSeleccionado;
    private bool _haySugerencias;

    public bool IsNotBusy => !IsBusy;

    public IRelayCommand GuardarCommand { get; }

    public RegistrarPresupuestoViewModel(IDataService dataService)
    {
        _dataService = dataService;
        GuardarCommand = new AsyncRelayCommand(GuardarAsync, () => IsNotBusy);
        _ = CargarProyectosAsync();
    }

    private async Task CargarProyectosAsync()
    {
        var proyectos = await _dataService.GetProjects();
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Proyectos.Clear();
            foreach (var proyecto in proyectos)
                Proyectos.Add(proyecto);
        });
    }

    public Project ProyectoSeleccionado
    {
        get => _proyectoSeleccionado;
        set
        {
            SetProperty(ref _proyectoSeleccionado, value);
            if (value != null)
                IdProyecto = value.Id;
        }
    }

    public string NombreProyectoBusqueda
    {
        get => _nombreProyectoBusqueda;
        set
        {
            SetProperty(ref _nombreProyectoBusqueda, value);
            FiltrarProyectos();
        }
    }
    public bool HaySugerencias
    {
        get => _haySugerencias;
        set => SetProperty(ref _haySugerencias, value);
    }

    private bool _mostrarRegistrarNuevoProyecto;
    public bool MostrarRegistrarNuevoProyecto
    {
        get => _mostrarRegistrarNuevoProyecto;
        set => SetProperty(ref _mostrarRegistrarNuevoProyecto, value);
    }

    private int _idProyecto;
    public int IdProyecto
    {
        get => _idProyecto;
        set => SetProperty(ref _idProyecto, value);
    }

    private CancellationTokenSource _ctsFiltro;

    private async void FiltrarProyectos()
    {
        _ctsFiltro?.Cancel();
        _ctsFiltro = new CancellationTokenSource();
        var token = _ctsFiltro.Token;
        var busqueda = NombreProyectoBusqueda;

        if (string.IsNullOrWhiteSpace(busqueda))
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ProyectosFiltrados.Clear();
                HaySugerencias = false;
                MostrarRegistrarNuevoProyecto = false;
            });
            return;
        }

        await Task.Run(() =>
        {
            var sugerencias = Proyectos
                .Where(p => p.NombreProyecto.Contains(busqueda, StringComparison.OrdinalIgnoreCase))
                .Take(3) // Limitar a 3 sugerencias
                .ToList();

            if (token.IsCancellationRequested) return;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                ProyectosFiltrados.Clear();
                foreach (var proyecto in sugerencias)
                    ProyectosFiltrados.Add(proyecto);

                HaySugerencias = ProyectosFiltrados.Any();
                MostrarRegistrarNuevoProyecto = !HaySugerencias;
            });
        }, token);
    }
    private async Task GuardarAsync()
    {
        if (string.IsNullOrWhiteSpace(NombrePresupuesto))
        {
            await Shell.Current.DisplayAlert("Error", "Completa todos los campos.", "OK");
            return;
        }

        if (IdProyecto == 0)
        {
            await Shell.Current.DisplayAlert("Error", "Debes seleccionar o registrar un proyecto.", "OK");
            return;
        }

        IsBusy = true;
        try
        {
            // Crear el objeto Presupuesto sin asignar un Id
            var presupuesto = new Presupuesto
            {
                NombrePresupuesto = NombrePresupuesto,
                FechaInicioPresupuesto = FechaInicioPresupuesto,
                FechaFinPresupuesto = FechaFinPresupuesto,
                IdProyecto = IdProyecto
            };

            // Guardar el presupuesto y obtener el objeto actualizado con el Id asignado
            var presupuestoGuardado = await _dataService.SavePresupuesto(presupuesto);

            if (presupuestoGuardado != null)
            {
                await Shell.Current.DisplayAlert("Éxito", $"El presupuesto se guardó correctamente. {presupuestoGuardado.Id}", "OK");
                // Navega a la página de registrar etapa con el ID del presupuesto guardado
                await NavigateToRegisterEtapa(presupuestoGuardado);
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", "No se pudo guardar el presupuesto.", "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Ocurrió un error al guardar el presupuesto: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    // Metodo para navegar a la pagina de resgistro de estapas con ID del presupuesto registrado
    [RelayCommand]
    private async Task NavigateToRegisterEtapa(Presupuesto presupuesto)
    {
        if (presupuesto == null)
            return;

        try
        {
            var uri = $"{nameof(RegistrarEtapaPage)}?id={presupuesto.Id}";
            await Shell.Current.GoToAsync(uri);
        }
        catch (Exception ex)
        {
            // Manejar cualquier error de navegación
            await Shell.Current.DisplayAlert(
                "Error",
                $"No se pudo navegar: {ex.Message}",
                "OK"
            );
        }
    }
}