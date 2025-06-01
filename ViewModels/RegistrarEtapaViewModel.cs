using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DelCorp.Models;
using DelCorp.Services;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Diagnostics; // Para Debug.WriteLine

namespace DelCorp.ViewModels;

public partial class RegistrarEtapaViewModel : ObservableObject, IQueryAttributable
{
    private readonly IDataService _dataService;

    [ObservableProperty]
    private decimal? _cantidadEtapa;

    [ObservableProperty]
    private int _idPresupuesto;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private ObservableCollection<Etapa> _etapas = new();

    [ObservableProperty]
    private ObservableCollection<UniMedRe> _disponibleUniMedRe = new();

    [ObservableProperty]
    private UniMedRe _selectedUniMedRe;

    [ObservableProperty]
    private ObservableCollection<CategoriaActividad> _categoriasActividad = new();

    [ObservableProperty]
    private CategoriaActividad _selectedCategoriaActividad;

    [ObservableProperty]
    private ObservableCollection<Actividad> _actividadesDisponibles = new();

    [ObservableProperty]
    private Actividad _selectedActividad;

    [ObservableProperty]
    private string _nombreNuevaActividad;

    [ObservableProperty]
    private bool _mostrarCampoNuevaActividad;

    [ObservableProperty]
    private bool _pickersForNewActivityEnabled;

    [ObservableProperty]
    private string _searchTextActividad;

    public bool IsNotBusy => !IsBusy;

    public RegistrarEtapaViewModel(IDataService dataService)
    {
        _dataService = dataService;
        _pickersForNewActivityEnabled = true;
    }

    private void SetIsBusy(bool busy)
    {
        IsBusy = busy;
        OnPropertyChanged(nameof(IsNotBusy));
    }

    async partial void OnSearchTextActividadChanged(string oldValue, string newValue)
    {
        Debug.WriteLine($"[RegistrarEtapaVM] SearchTextActividad cambiado a: {newValue}");
        await CargarActividadesAsync(null, newValue);
    }

    public async Task ActualizarEtapaConSubetapas(long idEtapa)
    {
        var etapa = Etapas.FirstOrDefault(e => e.Id == idEtapa);
        if (etapa != null)
        {
            // Instead of just reloading everything, you might want to find the specific etapa
            // and update its properties if only its sub-etapa related totals changed.
            // For simplicity here, we reload all, but this can be optimized.
            await CargarEtapasAsync();
        }
    }

    private async Task Initialize(int presupuestoId)
    {
        IdPresupuesto = presupuestoId;
        Debug.WriteLine($"[RegistrarEtapaVM] Inicializando con IdPresupuesto: {IdPresupuesto}");
        SetIsBusy(true);
        try
        {
            await CargarUniMedReAsync();
            await CargarCategoriasActividadAsync();
            await CargarActividadesAsync(null, null);
            await CargarEtapasAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[RegistrarEtapaVM] Error en Initialize: {ex.Message}");
            await Shell.Current.DisplayAlert("Error de Inicialización", ex.Message, "OK");
        }
        finally
        {
            SetIsBusy(false);
        }
    }

    private async Task CargarUniMedReAsync()
    {
        if (DisponibleUniMedRe.Any() && !IsBusy) return; // Avoid reloading if already populated and not busy
        bool wasBusy = IsBusy;
        if (!wasBusy) SetIsBusy(true);
        try
        {
            var unidades = await _dataService.GetUniMedReAsync();
            DisponibleUniMedRe.Clear();
            foreach (var unidad in unidades.OrderBy(u => u.NombreUniMedRe))
            {
                DisponibleUniMedRe.Add(unidad);
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"No se pudieron cargar las unidades de medida: {ex.Message}", "OK");
        }
        finally
        {
            if (!wasBusy) SetIsBusy(false);
        }
    }

    private async Task CargarCategoriasActividadAsync()
    {
        if (CategoriasActividad.Any() && !IsBusy) return;
        bool wasBusy = IsBusy;
        if (!wasBusy) SetIsBusy(true);
        try
        {
            var categorias = await _dataService.GetCategoriasActividadAsync();
            CategoriasActividad.Clear();
            foreach (var cat in categorias.OrderBy(c => c.NombreCategoriaActividad))
            {
                CategoriasActividad.Add(cat);
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"No se pudieron cargar las categorías de actividad: {ex.Message}", "OK");
        }
        finally
        {
            if (!wasBusy) SetIsBusy(false);
        }
    }

    private async Task CargarActividadesAsync(long? categoriaIdFiltro, string textoBusqueda)
    {
        Debug.WriteLine($"[RegistrarEtapaVM] CargarActividadesAsync llamado con CategoriaId: {categoriaIdFiltro}, TextoBusqueda: {textoBusqueda}");
        bool wasBusy = IsBusy;
        if (!wasBusy) SetIsBusy(true);

        try
        {
            ActividadesDisponibles.Clear();
            var actividades = await _dataService.GetActividadesAsync(categoriaIdFiltro, textoBusqueda);

            var categoriasCacheadas = CategoriasActividad.Any() ? CategoriasActividad.ToList() : (await _dataService.GetCategoriasActividadAsync()).ToList();
            var unidadesMedidaCacheadas = DisponibleUniMedRe.Any() ? DisponibleUniMedRe.ToList() : (await _dataService.GetUniMedReAsync()).ToList();

            foreach (var act in actividades.OrderBy(a => a.NombreActividad))
            {
                if (act.CategoriaActividadId.HasValue && act.CategoriaActividad == null)
                {
                    act.CategoriaActividad = categoriasCacheadas.FirstOrDefault(c => c.IdCategoriaActividad == act.CategoriaActividadId.Value);
                }
                if (act.UnidadMedidaId.HasValue && act.UnidadMedida == null)
                {
                    act.UnidadMedida = unidadesMedidaCacheadas.FirstOrDefault(u => u.Id == act.UnidadMedidaId.Value);
                }
                ActividadesDisponibles.Add(act);
            }
            ActividadesDisponibles.Add(new Actividad { IdActividad = -1, NombreActividad = "Registrar nueva actividad..." });
            Debug.WriteLine($"[RegistrarEtapaVM] ActividadesDisponibles cargadas: {ActividadesDisponibles.Count} items.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[RegistrarEtapaVM] Error en CargarActividadesAsync: {ex.Message}");
            await Shell.Current.DisplayAlert("Error", $"No se pudieron cargar las actividades: {ex.Message}", "OK");
        }
        finally
        {
            if (!wasBusy) SetIsBusy(false);
        }
    }

    public async Task CargarEtapasAsync()
    {
        SetIsBusy(true);
        try
        {
            var listaEtapasDto = await _dataService.GetEtapasByPresupuestoId(IdPresupuesto);
            Etapas.Clear();

            // Order by NumeroEtapa when initially loading
            foreach (var etapa in listaEtapasDto.OrderBy(e => e.NumeroEtapa))
            {
                RecalcularTotalesParaEtapa(etapa);
                Etapas.Add(etapa);
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"No se pudieron cargar las etapas: {ex.Message}", "OK");
            System.Diagnostics.Debug.WriteLine($"[RegistrarEtapaVM.CargarEtapasAsync] Error: {ex.Message}");
        }
        finally
        {
            SetIsBusy(false);
        }
    }

    public async void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("id", out var idObj) && int.TryParse(idObj.ToString(), out var presupuestoId))
        {
            if (IdPresupuesto != presupuestoId || !Etapas.Any())
            {
                await Initialize(presupuestoId);
            }
            else
            {
                // If already initialized for this budget, just refresh the stages
                await CargarEtapasAsync();
            }
        }
    }

    [RelayCommand]
    public async Task RegistrarSubEtapaAsync(Etapa etapa)
    {
        if (etapa == null)
        {
            await Shell.Current.DisplayAlert("Error", "Selecciona una etapa.", "OK");
            return;
        }
        await Shell.Current.GoToAsync($"RegistrarSubEtapaPage?idEtapa={etapa.Id}");
    }

    partial void OnSelectedActividadChanged(Actividad oldValue, Actividad newValue)
    {
        Actividad value = newValue;
        if (value != null && value.IdActividad == -1)
        {
            MostrarCampoNuevaActividad = true;
            NombreNuevaActividad = string.Empty;
            PickersForNewActivityEnabled = true; // Allow selecting category/UOM for new activity
            SelectedCategoriaActividad = null;  // Reset category picker for new activity
            SelectedUniMedRe = null;            // Reset UOM picker for new activity
        }
        else if (value != null) // An existing activity is selected
        {
            MostrarCampoNuevaActividad = false;
            PickersForNewActivityEnabled = false; // Disable category/UOM pickers, use activity's own
            // Set pickers to reflect the selected activity's properties
            SelectedCategoriaActividad = _categoriasActividad.FirstOrDefault(c => c.IdCategoriaActividad == value.CategoriaActividadId);
            SelectedUniMedRe = _disponibleUniMedRe.FirstOrDefault(u => u.Id == value.UnidadMedidaId);
        }
        else // No activity selected or selection cleared
        {
            MostrarCampoNuevaActividad = false;
            NombreNuevaActividad = string.Empty;
            PickersForNewActivityEnabled = true; // Enable pickers
            SelectedCategoriaActividad = null;
            SelectedUniMedRe = null;
        }
    }

    private void RecalcularTotalesParaEtapa(Etapa etapa)
    {
        if (etapa == null) return;
        etapa.SubEtapas ??= new List<SubEtapa>();

        etapa.MontoTotalEtapa = etapa.SubEtapas.Sum(s => s.TotalSubEstapa ?? 0M);

        decimal cantidadCalculadaEtapa = 0;
        var subEtapasContables = etapa.SubEtapas
            .Where(s => s.Actividad?.CategoriaActividad?.EsContable == true && s.CantidadSubEtapa.HasValue)
            .ToList();

        if (subEtapasContables.Any())
        {
            cantidadCalculadaEtapa = subEtapasContables.Sum(s => s.CantidadSubEtapa.Value);
        }
        else
        {
            cantidadCalculadaEtapa = etapa.SubEtapas
                .Where(s => s.CantidadSubEtapa.HasValue)
                .Sum(s => s.CantidadSubEtapa.Value);
        }
        etapa.CantidadEtapa = cantidadCalculadaEtapa;
    }

    [RelayCommand]
    public async Task GuardarEtapaAsync()
    {
        SetIsBusy(true);
        long? idActividadParaGuardar = null;

        if (SelectedActividad == null)
        {
            await Shell.Current.DisplayAlert("Validación", "Debe seleccionar una actividad para la etapa.", "OK");
            SetIsBusy(false);
            return;
        }

        if (SelectedActividad.IdActividad == -1) // User wants to register a new activity
        {
            if (string.IsNullOrWhiteSpace(NombreNuevaActividad))
            {
                await Shell.Current.DisplayAlert("Validación", "El nombre de la nueva actividad es obligatorio.", "OK");
                SetIsBusy(false);
                return;
            }
            if (SelectedCategoriaActividad == null)
            {
                await Shell.Current.DisplayAlert("Validación", "Debe seleccionar una categoría para la nueva actividad.", "OK");
                SetIsBusy(false);
                return;
            }
            if (SelectedUniMedRe == null)
            {
                await Shell.Current.DisplayAlert("Validación", "Debe seleccionar una unidad de medida para la nueva actividad.", "OK");
                SetIsBusy(false);
                return;
            }

            var nuevaActividadDto = new Actividad
            {
                NombreActividad = NombreNuevaActividad,
                CategoriaActividadId = SelectedCategoriaActividad.IdCategoriaActividad,
                UnidadMedidaId = SelectedUniMedRe.Id,
                CreatedAt = DateTime.UtcNow // Handled by service/DB ideally, but good for local
            };

            var actividadGuardada = await _dataService.SaveActividadAsync(nuevaActividadDto);
            if (actividadGuardada == null || actividadGuardada.IdActividad == 0)
            {
                await Shell.Current.DisplayAlert("Error", "No se pudo guardar la nueva actividad. Verifique los datos o la conexión.", "OK");
                SetIsBusy(false);
                return;
            }
            idActividadParaGuardar = actividadGuardada.IdActividad;
            await CargarActividadesAsync(null, SearchTextActividad); // Refresh activities list
        }
        else
        {
            idActividadParaGuardar = SelectedActividad.IdActividad;
        }

        if (!idActividadParaGuardar.HasValue || idActividadParaGuardar.Value == 0)
        {
            await Shell.Current.DisplayAlert("Error", "No se pudo determinar la actividad para la etapa.", "OK");
            SetIsBusy(false);
            return;
        }

        if (CantidadEtapa == null || CantidadEtapa <= 0)
        {
            await Shell.Current.DisplayAlert("Validación", "La cantidad para la etapa debe ser un valor positivo.", "OK");
            SetIsBusy(false);
            return;
        }

        try
        {
            long nuevoIdEtapaLocal = OfflineFirstDataService.GenerarIdAleatorio(); // Consider if this ID generation is appropriate
            long nuevoNumeroEtapa = (Etapas.Any() ? Etapas.Max(e => e.NumeroEtapa) : 0) + 1;

            var etapa = new Etapa
            {
                Id = nuevoIdEtapaLocal, // This ID might be temporary if server generates its own
                IdActividadEtapa = idActividadParaGuardar.Value,
                IdPresupuesto = this.IdPresupuesto,
                CantidadEtapa = this.CantidadEtapa,
                MontoTotalEtapa = 0, // Calculated based on sub-etapas later
                NumeroEtapa = nuevoNumeroEtapa, // Assign new stage number
                CreatedAt = DateTime.UtcNow
            };

            var etapaGuardada = await _dataService.SaveEtapa(etapa);
            if (etapaGuardada != null)
            {
                // Reset form fields
                SelectedActividad = null;
                CantidadEtapa = null;
                NombreNuevaActividad = string.Empty; // If a new activity was being entered
                SearchTextActividad = string.Empty; // Clear search text
                MostrarCampoNuevaActividad = false; // Hide new activity field

                await CargarEtapasAsync(); // Refresh the list, which will now be sorted by NumeroEtapa
                await Shell.Current.DisplayAlert("Éxito", $"Etapa guardada con número {etapaGuardada.NumeroEtapa}.", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", "No se pudo guardar la etapa.", "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"No se pudo guardar la etapa: {ex.Message}", "OK");
        }
        finally
        {
            SetIsBusy(false);
        }
    }

    private async Task RenumberAndSaveEtapasAsync()
    {
        SetIsBusy(true);
        try
        {
            for (int i = 0; i < Etapas.Count; i++)
            {
                var etapa = Etapas[i];
                if (etapa.NumeroEtapa != (i + 1)) // Only save if number changed
                {
                    etapa.NumeroEtapa = i + 1;
                    await _dataService.SaveEtapa(etapa);
                }
            }
            // After renumbering and saving all, reload from data source to ensure sorted by new NumeroEtapa
            await CargarEtapasAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[RegistrarEtapaVM] Error in RenumberAndSaveEtapasAsync: {ex.Message}");
            await Shell.Current.DisplayAlert("Error", $"No se pudo reordenar las etapas: {ex.Message}", "OK");
            await CargarEtapasAsync(); // Attempt to reload to a consistent state
        }
        finally
        {
            SetIsBusy(false);
        }
    }

    [RelayCommand]
    private async Task MoveEtapaUpAsync(Etapa etapa)
    {
        if (etapa == null) return;
        int currentIndex = Etapas.IndexOf(etapa);
        if (currentIndex > 0) // Can move up
        {
            Etapas.Move(currentIndex, currentIndex - 1);
            await RenumberAndSaveEtapasAsync();
        }
    }

    [RelayCommand]
    private async Task MoveEtapaDownAsync(Etapa etapa)
    {
        if (etapa == null) return;
        int currentIndex = Etapas.IndexOf(etapa);
        if (currentIndex < Etapas.Count - 1) // Can move down
        {
            Etapas.Move(currentIndex, currentIndex + 1);
            await RenumberAndSaveEtapasAsync();
        }
    }
}