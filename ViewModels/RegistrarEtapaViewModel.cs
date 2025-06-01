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

    [ObservableProperty] // Nueva propiedad para el texto de búsqueda
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

    // Método invocado cuando SearchTextActividad cambia
    async partial void OnSearchTextActividadChanged(string oldValue, string newValue)
    {
        // Aquí puedes añadir un debounce si prefieres no buscar en cada tecleo.
        // Por ahora, buscará directamente.
        // Asumimos que la búsqueda de actividades no se filtra por la categoría seleccionada para "Nueva Actividad".
        // Si se quisiera filtrar también por una categoría general, se necesitaría otro picker para ello.
        Debug.WriteLine($"[RegistrarEtapaVM] SearchTextActividad cambiado a: {newValue}");
        await CargarActividadesAsync(null, newValue); // Cargar actividades filtrando por texto, sin filtro de categoría
    }


    public async Task ActualizarEtapaConSubetapas(long idEtapa)
    {
        var etapa = Etapas.FirstOrDefault(e => e.Id == idEtapa);
        if (etapa != null)
        {
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
            // Cargar actividades inicialmente sin filtro de texto ni categoría
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
        if (DisponibleUniMedRe.Any() && !IsBusy) return;
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

    // Modificado para aceptar texto de búsqueda
    private async Task CargarActividadesAsync(long? categoriaIdFiltro, string textoBusqueda)
    {
        Debug.WriteLine($"[RegistrarEtapaVM] CargarActividadesAsync llamado con CategoriaId: {categoriaIdFiltro}, TextoBusqueda: {textoBusqueda}");
        bool wasBusy = IsBusy;
        if (!wasBusy) SetIsBusy(true);

        try
        {
            ActividadesDisponibles.Clear();
            // Llamar al servicio con ambos filtros
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
            // Siempre añadir la opción de registrar nueva actividad, independientemente de los resultados de búsqueda
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

            foreach (var etapa in listaEtapasDto.OrderBy(e => e.CreatedAt))
            {
                RecalcularTotalesParaEtapa(etapa); // Llamar después de que SubEtapas estén listas.
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
        Actividad value = newValue; // Para mantener la lógica original que usa 'value'
        if (value != null && value.IdActividad == -1)
        {
            MostrarCampoNuevaActividad = true;
            NombreNuevaActividad = string.Empty;
            PickersForNewActivityEnabled = true;
            SelectedCategoriaActividad = null;
            SelectedUniMedRe = null;
        }
        else if (value != null)
        {
            MostrarCampoNuevaActividad = false;
            // NombreNuevaActividad = value.NombreActividad; // No actualizar este campo, es para nueva actividad
            PickersForNewActivityEnabled = false;

            SelectedCategoriaActividad = _categoriasActividad.FirstOrDefault(c => c.IdCategoriaActividad == value.CategoriaActividadId);
            SelectedUniMedRe = _disponibleUniMedRe.FirstOrDefault(u => u.Id == value.UnidadMedidaId);
        }
        else
        {
            MostrarCampoNuevaActividad = false;
            NombreNuevaActividad = string.Empty;
            PickersForNewActivityEnabled = true;
            SelectedCategoriaActividad = null;
            SelectedUniMedRe = null;
        }
    }

    private void RecalcularTotalesParaEtapa(Etapa etapa)
    {
        if (etapa == null) return;
        etapa.SubEtapas ??= new List<SubEtapa>(); // Asegurar que la lista no sea null

        // Calcular MontoTotalEtapa (esto ya lo tenías)
        etapa.MontoTotalEtapa = etapa.SubEtapas.Sum(s => s.TotalSubEstapa ?? 0M);

        // Nueva lógica para calcular CantidadEtapa
        decimal cantidadCalculadaEtapa = 0;

        var subEtapasContables = etapa.SubEtapas
            .Where(s => s.Actividad?.CategoriaActividad?.EsContable == true && s.CantidadSubEtapa.HasValue)
            .ToList();

        if (subEtapasContables.Any())
        {
            // Si hay al menos una subetapa contable, sumar solo las cantidades de las contables
            cantidadCalculadaEtapa = subEtapasContables.Sum(s => s.CantidadSubEtapa.Value);
        }
        else
        {
            // Si no hay ninguna subetapa contable (o no hay subetapas),
            // sumar las cantidades de TODAS las subetapas (si las hay).
            // Esto cubre el caso de "acabados" donde podrías tener M2 de pintura, repello, etc.
            // y quieres que la cantidad de la etapa refleje la suma de esas áreas/volúmenes.
            cantidadCalculadaEtapa = etapa.SubEtapas
                .Where(s => s.CantidadSubEtapa.HasValue)
                .Sum(s => s.CantidadSubEtapa.Value);
        }

        etapa.CantidadEtapa = cantidadCalculadaEtapa;

        // Importante: Si Etapa.CantidadEtapa no es una [ObservableProperty] directamente en el modelo Etapa
        // y estás mostrando esto en una CollectionView, puede que necesites forzar una actualización de la UI
        // para ese ítem específico si la CollectionView no detecta el cambio interno.
        // Sin embargo, si Etapas es una ObservableCollection<Etapa> y Etapa es una clase,
        // modificar sus propiedades debería reflejarse si el DataTemplate bindea a esas propiedades.
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

        if (SelectedActividad.IdActividad == -1)
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
                CreatedAt = DateTime.UtcNow
            };

            var actividadGuardada = await _dataService.SaveActividadAsync(nuevaActividadDto);
            if (actividadGuardada == null || actividadGuardada.IdActividad == 0)
            {
                await Shell.Current.DisplayAlert("Error", "No se pudo guardar la nueva actividad. Verifique los datos o la conexión.", "OK");
                SetIsBusy(false);
                return;
            }
            idActividadParaGuardar = actividadGuardada.IdActividad;
            // Recargar lista de actividades para que la nueva aparezca y se pueda seleccionar sin reiniciar la página
            await CargarActividadesAsync(null, SearchTextActividad); // Usar el texto de búsqueda actual
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
            long nuevoIdEtapaLocal = OfflineFirstDataService.GenerarIdAleatorio();

            var etapa = new Etapa
            {
                Id = nuevoIdEtapaLocal,
                IdActividadEtapa = idActividadParaGuardar.Value,
                IdPresupuesto = this.IdPresupuesto,
                CantidadEtapa = this.CantidadEtapa,
                MontoTotalEtapa = 0,
                NumeroEtapa = Etapas.Count + 1,
                CreatedAt = DateTime.UtcNow
            };

            var etapaGuardada = await _dataService.SaveEtapa(etapa);
            if (etapaGuardada != null)
            {
                SelectedActividad = null;
                CantidadEtapa = null;
                SearchTextActividad = string.Empty; // Limpiar búsqueda después de guardar

                await CargarEtapasAsync();
                await Shell.Current.DisplayAlert("Éxito", $"Etapa guardada.", "OK");
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
}