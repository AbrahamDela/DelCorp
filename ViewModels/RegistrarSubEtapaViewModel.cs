using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DelCorp.Models;
using DelCorp.Services;
using DelCorp.Views;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics;
using System;

namespace DelCorp.ViewModels;

public partial class RegistrarSubEtapaViewModel : ObservableObject, IQueryAttributable
{
    private readonly IDataService _dataService;

    [ObservableProperty] private long _idEtapa;

    // Campo de entrada principal para la SubEtapa, además de la Actividad
    [ObservableProperty] private decimal? _cantidadSubEtapa; // El usuario ingresa esto

    // Campos que se calculan o se actualizan desde la página de recursos
    [ObservableProperty] private decimal? _precioUniSubEtapa; // Se calculará (Total / Cantidad)
    [ObservableProperty] private decimal? _totalSubEstapa;    // Se actualizará desde recursos

    // Campo de entrada numérica directa
    [ObservableProperty] private long? _diasCalSubEtapa;

    // Otros campos (si son necesarios y editables, de lo contrario, se pueden quitar del formulario)
    // [ObservableProperty] private decimal? _precioUniEjeSubEtapa;
    // [ObservableProperty] private decimal? _montoEjeSubEtapa;
    // [ObservableProperty] private long? _diasEjeSubEtapa;


    [ObservableProperty] private ObservableCollection<SubEtapa> _subEtapas = new();
    [ObservableProperty] private bool _isBusy;

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
    private string _unidadMedidaActividadSeleccionada;

    [ObservableProperty]
    private bool _pickersForNewActivityEnabled;

    public bool IsNotBusy => !IsBusy;

    private void SetIsBusy(bool busy)
    {
        IsBusy = busy;
    }

    public RegistrarSubEtapaViewModel(IDataService dataService)
    {
        _dataService = dataService;
        PickersForNewActivityEnabled = true;
    }

    async partial void OnSelectedCategoriaActividadChanged(CategoriaActividad oldValue, CategoriaActividad newValue)
    {
        // Lógica si es necesaria
    }

    partial void OnSelectedActividadChanged(Actividad oldValue, Actividad newValue)
    {
        Actividad value = newValue;
        if (value != null && value.IdActividad == -1)
        {
            MostrarCampoNuevaActividad = true;
            NombreNuevaActividad = string.Empty;
            UnidadMedidaActividadSeleccionada = string.Empty;
            PickersForNewActivityEnabled = true;
        }
        else if (value != null)
        {
            MostrarCampoNuevaActividad = false;
            PickersForNewActivityEnabled = false;
            UnidadMedidaActividadSeleccionada = value.UnidadMedida?.NombreUniMedRe ?? "N/D";
            if (string.IsNullOrEmpty(UnidadMedidaActividadSeleccionada) || UnidadMedidaActividadSeleccionada == "N/D")
            {
                if (value.UnidadMedidaId.HasValue)
                {
                    Task.Run(async () => {
                        // Asegurarse que _dataService esté disponible y no sea null
                        if (_dataService != null)
                        {
                            var um = (await _dataService.GetUniMedReAsync()).FirstOrDefault(u => u.Id == value.UnidadMedidaId.Value);
                            if (um != null)
                            {
                                MainThread.BeginInvokeOnMainThread(() => {
                                    UnidadMedidaActividadSeleccionada = um.NombreUniMedRe;
                                });
                            }
                        }
                    });
                }
            }
        }
        else
        {
            MostrarCampoNuevaActividad = false;
            NombreNuevaActividad = string.Empty;
            UnidadMedidaActividadSeleccionada = string.Empty;
            PickersForNewActivityEnabled = true;
        }
    }

    public async void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("idEtapa", out var idObj) && long.TryParse(idObj.ToString(), out var etapaIdVal))
        {
            bool needsFullLoad = (IdEtapa != etapaIdVal || !SubEtapas.Any());
            IdEtapa = etapaIdVal;
            if (needsFullLoad)
            {
                await CargarDatosInicialesAsync();
            }
            else
            {
                await CargarSubEtapasAsync();
            }
        }
    }

    private async Task CargarDatosInicialesAsync()
    {
        SetIsBusy(true);
        try
        {
            await CargarCategoriasActividadAsync();
            await CargarActividadesAsync(null, null);
            await CargarSubEtapasAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[RegistrarSubEtapaVM.CargarDatosInicialesAsync] Error: {ex.Message}");
            await Shell.Current.DisplayAlert("Error", $"No se pudieron cargar los datos iniciales: {ex.Message}", "OK");
        }
        finally
        {
            SetIsBusy(false);
        }
    }

    private async Task CargarCategoriasActividadAsync()
    {
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
        catch (System.Exception ex)
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
        bool wasBusy = IsBusy;
        if (!wasBusy) SetIsBusy(true);
        try
        {
            ActividadesDisponibles.Clear();
            var actividades = await _dataService.GetActividadesAsync(categoriaIdFiltro, textoBusqueda);

            var todasUnidades = (await _dataService.GetUniMedReAsync()).ToList();
            var todasCategorias = CategoriasActividad.Any() ? CategoriasActividad.ToList() : (await _dataService.GetCategoriasActividadAsync()).ToList();

            foreach (var act in actividades.OrderBy(a => a.NombreActividad))
            {
                if (act.UnidadMedidaId.HasValue && act.UnidadMedida == null)
                {
                    act.UnidadMedida = todasUnidades.FirstOrDefault(u => u.Id == act.UnidadMedidaId.Value);
                }
                if (act.CategoriaActividadId.HasValue && act.CategoriaActividad == null)
                {
                    act.CategoriaActividad = todasCategorias.FirstOrDefault(c => c.IdCategoriaActividad == act.CategoriaActividadId.Value);
                }
                ActividadesDisponibles.Add(act);
            }
            ActividadesDisponibles.Add(new Actividad { IdActividad = -1, NombreActividad = "Registrar nueva actividad..." });
        }
        catch (System.Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"No se pudieron cargar las actividades: {ex.Message}", "OK");
        }
        finally
        {
            if (!wasBusy) SetIsBusy(false);
        }
    }

    // Se actualiza cuando TotalSubEstapa (desde recursos) o CantidadSubEtapa (ingresada por usuario) cambian.
    partial void OnTotalSubEstapaChanged(decimal? oldValue, decimal? newValue) => RecalcularPrecioUnitarioSubEtapa();
    partial void OnCantidadSubEtapaChanged(decimal? oldValue, decimal? newValue) => RecalcularPrecioUnitarioSubEtapa();

    private void RecalcularPrecioUnitarioSubEtapa()
    {
        if (CantidadSubEtapa.HasValue && CantidadSubEtapa.Value != 0 && TotalSubEstapa.HasValue)
        {
            PrecioUniSubEtapa = TotalSubEstapa.Value / CantidadSubEtapa.Value;
        }
        else
        {
            PrecioUniSubEtapa = null; // o 0
        }
    }

    [RelayCommand]
    public async Task GuardarSubEtapaAsync()
    {
        SetIsBusy(true);
        long? idActividadParaGuardar = null;

        if (SelectedActividad == null)
        {
            await Shell.Current.DisplayAlert("Validación", "Debe seleccionar una actividad para la subetapa.", "OK");
            SetIsBusy(false);
            return;
        }

        // Lógica para registrar nueva actividad (si aplica)
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
            // Asumimos que la nueva actividad hereda la UOM de alguna forma o se pide un picker adicional
            // Para simplificar, si no se define explícitamente una UOM para la NUEVA actividad, podría quedar null.
            // Es importante que la tabla `actividades` permita `unidad_medida_id` nullable si este es el caso.
            // O, añadir un picker para `UniMedRe` cuando `MostrarCampoNuevaActividad` es true.

            var nuevaActividadDto = new Actividad
            {
                NombreActividad = NombreNuevaActividad,
                CategoriaActividadId = SelectedCategoriaActividad.IdCategoriaActividad,
                // UnidadMedidaId = idDeLaUnidadDeMedidaSeleccionadaParaNuevaActividad, // Si se añade picker
                CreatedAt = System.DateTime.UtcNow
            };
            var actividadGuardada = await _dataService.SaveActividadAsync(nuevaActividadDto);
            if (actividadGuardada == null || actividadGuardada.IdActividad == 0)
            {
                await Shell.Current.DisplayAlert("Error", "No se pudo guardar la nueva actividad.", "OK");
                SetIsBusy(false);
                return;
            }
            idActividadParaGuardar = actividadGuardada.IdActividad;
            await CargarActividadesAsync(null, null);
        }
        else
        {
            idActividadParaGuardar = SelectedActividad.IdActividad;
        }

        if (!idActividadParaGuardar.HasValue || idActividadParaGuardar.Value == 0)
        {
            await Shell.Current.DisplayAlert("Error", "No se pudo determinar la actividad.", "OK");
            SetIsBusy(false);
            return;
        }

        // Validar CantidadSubEtapa ingresada por el usuario
        if (CantidadSubEtapa == null || CantidadSubEtapa.Value <= 0)
        {
            await Shell.Current.DisplayAlert("Validación", "La cantidad de la subetapa debe ser un valor positivo.", "OK");
            SetIsBusy(false);
            return;
        }

        if (DiasCalSubEtapa == null || DiasCalSubEtapa <= 0)
        {
            await Shell.Current.DisplayAlert("Validación", "Los días calendario estimados deben ser mayor a cero.", "OK");
            SetIsBusy(false);
            return;
        }

        try
        {
            long idSubEtapaLocal = OfflineFirstDataService.GenerarIdAleatorio();

            var subEtapa = new SubEtapa
            {
                Id = idSubEtapaLocal,
                ActividadSubEtapaId = idActividadParaGuardar,
                CantidadSubEtapa = CantidadSubEtapa, // Guardar la cantidad ingresada por el usuario
                PrecioUniSubEtapa = null, // Se calculará después de añadir recursos
                TotalSubEstapa = null,    // Se actualizará desde recursos
                DiasCalSubEtapa = DiasCalSubEtapa,
                IdEtapa = IdEtapa,
                CreatedAt = System.DateTime.UtcNow,
                NumeroSubEtapa = (SubEtapas.Any() ? SubEtapas.Max(s => s.NumeroSubEtapa) : 0) + 1
            };

            await _dataService.SaveSubEtapa(subEtapa);

            // Limpiar campos del formulario
            SelectedActividad = null;
            CantidadSubEtapa = null; // Limpiar la cantidad ingresada
            DiasCalSubEtapa = null;
            // Limpiar propiedades calculadas/de solo lectura que se muestran en el form
            PrecioUniSubEtapa = null;
            TotalSubEstapa = null;

            await CargarSubEtapasAsync();
            await Shell.Current.DisplayAlert("Éxito", "Subetapa guardada. Añada recursos para calcular costos.", "OK");
        }
        catch (System.Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"No se pudo guardar la subetapa: {ex.Message}", "OK");
        }
        finally
        {
            SetIsBusy(false);
        }
    }

    public async Task CargarSubEtapasAsync()
    {
        SetIsBusy(true);
        try
        {
            var listaSubEtapasDto = await _dataService.GetSubEtapasByEtapaId(IdEtapa);
            SubEtapas.Clear();
            foreach (var sub in listaSubEtapasDto.OrderBy(s => s.NumeroSubEtapa).ThenBy(s => s.CreatedAt))
            {
                // Recalcular precio unitario al cargar, por si Total o Cantidad cambiaron.
                if (sub.CantidadSubEtapa.HasValue && sub.CantidadSubEtapa > 0 && sub.TotalSubEstapa.HasValue)
                {
                    sub.PrecioUniSubEtapa = sub.TotalSubEstapa / sub.CantidadSubEtapa;
                }
                else
                {
                    sub.PrecioUniSubEtapa = null;
                }
                SubEtapas.Add(sub);
            }
        }
        catch (System.Exception ex)
        {
            Debug.WriteLine($"[RegistrarSubEtapaVM.CargarSubEtapasAsync] Error: {ex.Message}");
            await Shell.Current.DisplayAlert("Error", $"No se pudieron cargar las subetapas: {ex.Message}", "OK");
        }
        finally
        {
            SetIsBusy(false);
        }
    }

    [RelayCommand]
    private async Task NavigateToRegistrarRecursosAsync(SubEtapa subEtapa)
    {
        if (subEtapa == null || subEtapa.Id == 0)
        {
            await Shell.Current.DisplayAlert("Error", "La SubEtapa no es válida.", "OK");
            return;
        }
        SetIsBusy(true);
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
            SetIsBusy(false);
        }
    }

    public async Task OnPageAppearing()
    {
        Debug.WriteLine($"[RegistrarSubEtapaVM.OnPageAppearing] IdEtapa: {IdEtapa}");
        if (IdEtapa != 0)
        {
            await CargarSubEtapasAsync();
        }
    }
}