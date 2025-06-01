using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DelCorp.Models;
using DelCorp.Services;
using DelCorp.Views;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace DelCorp.ViewModels;

public partial class RegistrarSubEtapaViewModel : ObservableObject, IQueryAttributable
{
    private readonly IDataService _dataService;

    [ObservableProperty] private long idEtapa;
    [ObservableProperty] private decimal? cantidadSubEtapa;
    [ObservableProperty] private decimal? precioUniSubEtapa;
    [ObservableProperty] private decimal? precioUniEjeSubEtapa;
    //[ObservableProperty] private decimal? totalSubEstapa;
    [ObservableProperty] private decimal? montoEjeSubEtapa;
    [ObservableProperty] private long? diasCalSubEtapa;
    [ObservableProperty] private long? diasEjeSubEtapa;
    [ObservableProperty] private long? idUniMedida;
    [ObservableProperty] private ObservableCollection<SubEtapa> subEtapas = new();
    [ObservableProperty] private bool isBusy;

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

    //public record SubEtapaRegistradaMessage(long IdEtapa);


    public bool IsNotBusy => !IsBusy;

    public RegistrarSubEtapaViewModel(IDataService dataService)
    {
        _dataService = dataService;
    }

    // Método invocado cuando SelectedCategoriaActividad cambia
    async partial void OnSelectedCategoriaActividadChanged(CategoriaActividad value)
    {
        await CargarActividadesAsync(value?.IdCategoriaActividad);
        SelectedActividad = null;
        NombreNuevaActividad = string.Empty;
        MostrarCampoNuevaActividad = false;
    }

    // Método invocado cuando SelectedActividad cambia
    partial void OnSelectedActividadChanged(Actividad value)
    {
        if (value != null && value.IdActividad == -1) // Asumimos que IdActividad = -1 es el placeholder "Otra..."
        {
            MostrarCampoNuevaActividad = true;
            NombreNuevaActividad = string.Empty;
        }
        else
        {
            MostrarCampoNuevaActividad = false;
            if (value != null)
            {
                NombreNuevaActividad = value.NombreActividad;
            }
        }
    }

    private async Task CargarCategoriasActividadAsync()
    {
        if (IsBusy && CategoriasActividad.Any()) return;
        IsBusy = true; // Control de IsBusy
        try
        {
            var categorias = await _dataService.GetCategoriasActividadAsync();
            CategoriasActividad.Clear();
            foreach (var cat in categorias.OrderBy(c => c.NombreCategoriaActividad))
            {
                CategoriasActividad.Add(cat);
            }
        }
        catch (System.Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"No se pudieron cargar las categorías de actividad: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false; // Control de IsBusy
        }
    }

    private async Task CargarActividadesAsync(long? categoriaId)
    {
        // bool DueloDeIsBusy = IsBusy; if (!DueloDeIsBusy) IsBusy = true; // Manejar IsBusy si es necesario
        try
        {
            ActividadesDisponibles.Clear();
            if (categoriaId.HasValue)
            {
                var actividades = await _dataService.GetActividadesAsync(categoriaId.Value);
                foreach (var act in actividades.OrderBy(a => a.NombreActividad))
                {
                    ActividadesDisponibles.Add(act);
                }
            }
            ActividadesDisponibles.Add(new Actividad { IdActividad = -1, NombreActividad = "Registrar nueva actividad..." });
        }
        catch (System.Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"No se pudieron cargar las actividades: {ex.Message}", "OK");
        }
        finally
        {
            // if (!DueloDeIsBusy) IsBusy = false;
        }
    }

    public async void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("idEtapa", out var idObj) && long.TryParse(idObj.ToString(), out var etapaIdVal))
        {
            if (IdEtapa != etapaIdVal || !SubEtapas.Any())
            {
                IdEtapa = etapaIdVal;
                await CargarDatosInicialesAsync();
            }
            // Si se navega de vuelta y un recurso cambió (actualizando TotalSubEstapa),
            // necesitamos refrescar la lista de subetapas. OnAppearing en la Page es mejor para esto.
            else if (IdEtapa == etapaIdVal) // Volviendo a la misma etapa, refrescar
            {
                await CargarSubEtapasAsync();
            }
        }
    }

    private async Task CargarDatosInicialesAsync()
    {
        // Cargar UniMedRe y Actividades/Categorías es para los campos de ENTRADA de una NUEVA subetapa
        await CargarUniMedReAsync();
        await CargarCategoriasActividadAsync();
        // CargarActividadesAsync se llamará cuando SelectedCategoriaActividad cambie

        // CargarSubEtapasAsync es para la LISTA de subetapas existentes de la etapa actual
        await CargarSubEtapasAsync();
    }

    private async Task CargarUniMedReAsync()
    {
        // Optimización: no recargar si ya está ocupado o si ya hay datos y no se espera que cambien frecuentemente.
        if (IsBusy && DisponibleUniMedRe.Any()) return;
        IsBusy = true;
        try
        {
            var unidades = await _dataService.GetUniMedReAsync(); //
            DisponibleUniMedRe.Clear();
            foreach (var unidad in unidades.OrderBy(u => u.NombreUniMedRe)) // Ordenar para el Picker
            {
                DisponibleUniMedRe.Add(unidad);
            }
        }
        catch (System.Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"No se pudieron cargar las unidades de medida: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    partial void OnSelectedUniMedReChanged(UniMedRe value)
    {
        IdUniMedida = value?.Id;
    }

    [RelayCommand]
    public async Task GuardarSubEtapaAsync()
    {
        long? idActividadParaGuardar = SelectedActividad?.IdActividad;

        if (SelectedActividad != null && SelectedActividad.IdActividad == -1) // "Registrar nueva actividad..."
        {
            if (string.IsNullOrWhiteSpace(NombreNuevaActividad))
            {
                await Shell.Current.DisplayAlert("Error", "El nombre de la nueva actividad es obligatorio.", "OK");
                return;
            }
            var nuevaActividadDto = new Actividad
            {
                NombreActividad = NombreNuevaActividad,
                CategoriaActividadId = SelectedCategoriaActividad?.IdCategoriaActividad
            };
            var actividadGuardada = await _dataService.SaveActividadAsync(nuevaActividadDto);
            if (actividadGuardada == null || actividadGuardada.IdActividad == 0)
            {
                await Shell.Current.DisplayAlert("Error", "No se pudo guardar la nueva actividad.", "OK");
                return;
            }
            idActividadParaGuardar = actividadGuardada.IdActividad;
        }
        else if (SelectedActividad == null && !string.IsNullOrWhiteSpace(NombreNuevaActividad))
        {
            await Shell.Current.DisplayAlert("Error", "Seleccione una actividad de la lista o elija 'Registrar nueva actividad...' y escriba el nombre.", "OK");
            return;
        }


        if (!idActividadParaGuardar.HasValue || idActividadParaGuardar.Value == 0 || (idActividadParaGuardar.Value == -1 && string.IsNullOrWhiteSpace(NombreNuevaActividad)))
        {
            await Shell.Current.DisplayAlert("Error", "La actividad es obligatoria.", "OK");
            return;
        }
        if (CantidadSubEtapa == null) // Ajusta validaciones según necesites
        {
            await Shell.Current.DisplayAlert("Error", "Completa todos los campos obligatorios.", "OK");
            return;
        }

        IsBusy = true;
        try
        {
            long id = OfflineFirstDataService.GenerarIdAleatorio();

            var subEtapa = new SubEtapa
            {
                Id = id,
                ActividadSubEtapaId = idActividadParaGuardar, // Usar el ID de la actividad
                CantidadSubEtapa = CantidadSubEtapa,
                PrecioUniSubEtapa = PrecioUniSubEtapa,
                PrecioUniEjeSubEtapa = PrecioUniEjeSubEtapa,
                // TotalSubEstapa se calcula al agregar/modificar recursos
                MontoEjeSubEtapa = MontoEjeSubEtapa,
                DiasCalSubEtapa = DiasCalSubEtapa,
                DiasEjeSubEtapa = DiasEjeSubEtapa,
                IdEtapa = IdEtapa,
                IdUniMedida = IdUniMedida,
                CreatedAt = System.DateTime.UtcNow
            };

            await _dataService.SaveSubEtapa(subEtapa);
            await Shell.Current.DisplayAlert("Éxito", "Subetapa guardada correctamente.", "OK");

            // Limpiar campos de entrada
            SelectedCategoriaActividad = null; // Disparará la limpieza de actividades
            // SelectedActividad = null; // Se limpia por el cambio de categoría
            // NombreNuevaActividad = string.Empty; // Se limpia por el cambio de actividad/categoría
            CantidadSubEtapa = null;
            PrecioUniSubEtapa = null;
            PrecioUniEjeSubEtapa = null;
            MontoEjeSubEtapa = null;
            DiasCalSubEtapa = null;
            DiasEjeSubEtapa = null;
            SelectedUniMedRe = null; // Esto limpiará IdUniMedida

            await CargarSubEtapasAsync();
        }
        catch (System.Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"No se pudo guardar la subetapa: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task CargarSubEtapasAsync() // Lista de subetapas existentes
    {
        IsBusy = true;
        try
        {
            var lista = await _dataService.GetSubEtapasByEtapaId(IdEtapa);
            SubEtapas.Clear();
            var todasActividades = await _dataService.GetActividadesAsync(); // Cargar todas las actividades una vez

            foreach (var sub in lista.OrderBy(s => s.CreatedAt))
            {
                if (sub.ActividadSubEtapaId.HasValue)
                {
                    sub.Actividad = todasActividades.FirstOrDefault(a => a.IdActividad == sub.ActividadSubEtapaId.Value);
                }
                SubEtapas.Add(sub);
            }
        }
        catch (System.Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"No se pudieron cargar las subetapas: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task NavigateToRegistrarRecursosAsync(SubEtapa subEtapa)
    {
        if (subEtapa == null || subEtapa.Id == 0)
        {
            await Shell.Current.DisplayAlert("Error", "La SubEtapa no es válida o necesita ser sincronizada primero para agregar recursos.", "OK");
            return;
        }
        IsBusy = true;
        try
        {
            await Shell.Current.GoToAsync($"{nameof(RegistrarRecursoUtiPage)}?idSubEtapa={subEtapa.Id}");
        }
        catch (System.Exception ex)
        {
            await Shell.Current.DisplayAlert("Error de Navegación", $"No se pudo navegar: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
}