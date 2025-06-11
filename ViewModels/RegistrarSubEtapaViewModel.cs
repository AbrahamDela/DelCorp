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
    [ObservableProperty] private decimal? _cantidadSubEtapa;
    [ObservableProperty] private decimal? _precioUniSubEtapa;
    [ObservableProperty] private decimal? _totalSubEstapa;
    [ObservableProperty] private long? _diasCalSubEtapa;

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

    [ObservableProperty]
    private bool _hasPendingSubEtapaOrderChanges = false; // Nueva propiedad para cambios de orden

    public bool IsNotBusy => !IsBusy;

    private void SetIsBusy(bool busy)
    {
        IsBusy = busy;
        OnPropertyChanged(nameof(IsNotBusy)); // Asegúrate que esto se llama
    }

    public RegistrarSubEtapaViewModel(IDataService dataService)
    {
        _dataService = dataService;
        PickersForNewActivityEnabled = true;
    }

    // ... (OnSelectedCategoriaActividadChanged, OnSelectedActividadChanged sin cambios significativos para esta tarea) ...
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
            bool needsFullLoadOrDifferentEtapa = IdEtapa != etapaIdVal;

            if (needsFullLoadOrDifferentEtapa)
            {
                if (HasPendingSubEtapaOrderChanges)
                {
                    Debug.WriteLine($"Cambiando de Etapa ID {IdEtapa} a {etapaIdVal} con cambios de orden pendientes. Los cambios se perderán.");
                    // Opcional: preguntar al usuario. Por ahora, se descartan.
                    HasPendingSubEtapaOrderChanges = false;
                }
                IdEtapa = etapaIdVal;
                await CargarDatosInicialesAsync(); // Incluye CargarSubEtapasAsync
            }
            else if (!SubEtapas.Any() || !HasPendingSubEtapaOrderChanges) // Misma etapa, sin subetapas o sin cambios pendientes
            {
                IdEtapa = etapaIdVal; // Asegurar que IdEtapa esté seteado
                await CargarSubEtapasAsync();
            }
            // Si es la misma etapa y SÍ hay cambios pendientes, no hacemos nada para que el usuario pueda guardarlos.
        }
    }

    private async Task CargarDatosInicialesAsync()
    {
        SetIsBusy(true);
        try
        {
            await CargarCategoriasActividadAsync();
            await CargarActividadesAsync(null, null); // Carga actividades generales
            await CargarSubEtapasAsync(); // Carga subetapas para el IdEtapa actual
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

    // ... (CargarCategoriasActividadAsync, CargarActividadesAsync sin cambios) ...
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
            PrecioUniSubEtapa = null;
        }
    }

    [RelayCommand]
    public async Task GuardarSubEtapaAsync() // Guardar NUEVA subetapa
    {
        SetIsBusy(true);
        long? idActividadParaGuardar = null;

        // ... (Validaciones y lógica de creación de nueva actividad sin cambios) ...
        if (SelectedActividad == null)
        {
            await Shell.Current.DisplayAlert("Validación", "Debe seleccionar una actividad para la subetapa.", "OK");
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

            var nuevaActividadDto = new Actividad
            {
                NombreActividad = NombreNuevaActividad,
                CategoriaActividadId = SelectedCategoriaActividad.IdCategoriaActividad,
                // Asumimos que UnidadMedidaId se seleccionará para la nueva actividad si es necesario
                // o se deja null si la tabla lo permite.
                // UnidadMedidaId = _selectedUnidadMedidaParaNuevaActividad?.Id, 
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
            await CargarActividadesAsync(null, null); // Recargar actividades
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
            long nuevoNumeroSubEtapa = (SubEtapas.Any() ? SubEtapas.Max(s => s.NumeroSubEtapa) : 0) + 1;

            var subEtapa = new SubEtapa
            {
                Id = idSubEtapaLocal, // ID temporal para el DTO, el servicio lo manejará
                ActividadSubEtapaId = idActividadParaGuardar,
                CantidadSubEtapa = CantidadSubEtapa,
                PrecioUniSubEtapa = null,
                TotalSubEstapa = null,
                DiasCalSubEtapa = DiasCalSubEtapa,
                IdEtapa = IdEtapa,
                CreatedAt = System.DateTime.UtcNow,
                NumeroSubEtapa = nuevoNumeroSubEtapa // Asignar el número de subetapa
            };

            await _dataService.SaveSubEtapa(subEtapa); // El servicio debería manejar el ID final

            SelectedActividad = null;
            CantidadSubEtapa = null;
            DiasCalSubEtapa = null;
            PrecioUniSubEtapa = null;
            TotalSubEstapa = null;
            // No es necesario limpiar NombreNuevaActividad aquí, ya que se limpia cuando SelectedActividad cambia.

            await CargarSubEtapasAsync(); // Recarga y ordena por NumeroSubEtapa
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
            // Ordenar por NumeroSubEtapa al cargar
            foreach (var sub in listaSubEtapasDto.OrderBy(s => s.NumeroSubEtapa))
            {
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
            HasPendingSubEtapaOrderChanges = false; // Resetea al cargar desde la fuente
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
        if (subEtapa == null || subEtapa.Id == 0) // Id podría ser 0 si es una subetapa nueva aún no sincronizada con ID de servidor.
        {                                        // El servicio SaveSubEtapa debe asegurar que el Id local sea usable o que el Id del DTO sea el del server.
            await Shell.Current.DisplayAlert("Error", "La SubEtapa no es válida o no tiene un ID asignado.", "OK");
            return;
        }

        if (HasPendingSubEtapaOrderChanges)
        {
            bool saveChanges = await Shell.Current.DisplayAlert("Cambios sin Guardar",
                "El orden de las sub-etapas ha cambiado. ¿Desea guardar los cambios antes de continuar?",
                "Guardar y Continuar", "Continuar sin Guardar");

            if (saveChanges)
            {
                await SaveSubEtapaOrderChangesAsync();
                if (IsBusy)
                {
                    await Shell.Current.DisplayAlert("Guardado en progreso", "Espere a que termine el guardado.", "OK");
                    return;
                }
            }
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

    [RelayCommand]
    private async Task NavigateToRegistroRecursos(SubEtapa subEtapa)
    {
        if (subEtapa == null) return;
        await Shell.Current.GoToAsync($"{nameof(RegistroRecursosPage)}?idSubEtapa={subEtapa.Id}");
    }

    // --- MÉTODOS Y COMANDOS PARA REORDENAMIENTO OPTIMIZADO DE SUBETAPAS ---
    private void LocalRenumberSubEtapas()
    {
        for (int i = 0; i < SubEtapas.Count; i++)
        {
            if (SubEtapas[i].NumeroSubEtapa != (i + 1))
            {
                SubEtapas[i].NumeroSubEtapa = i + 1;
            }
        }
        // Si SubEtapa NO es ObservableObject, y la UI no se actualiza para NumeroSubEtapa:
        // var tempList = SubEtapas.ToList();
        // SubEtapas.Clear();
        // foreach(var se in tempList) SubEtapas.Add(se);
    }

    [RelayCommand]
    private void MoveSubEtapaUp(SubEtapa subEtapa)
    {
        if (subEtapa == null || IsBusy) return;
        int currentIndex = SubEtapas.IndexOf(subEtapa);
        if (currentIndex > 0)
        {
            SubEtapas.Move(currentIndex, currentIndex - 1);
            LocalRenumberSubEtapas();
            HasPendingSubEtapaOrderChanges = true;
        }
    }

    [RelayCommand]
    private void MoveSubEtapaDown(SubEtapa subEtapa)
    {
        if (subEtapa == null || IsBusy) return;
        int currentIndex = SubEtapas.IndexOf(subEtapa);
        if (currentIndex < SubEtapas.Count - 1)
        {
            SubEtapas.Move(currentIndex, currentIndex + 1);
            LocalRenumberSubEtapas();
            HasPendingSubEtapaOrderChanges = true;
        }
    }

    [RelayCommand]
    private async Task SaveSubEtapaOrderChangesAsync()
    {
        if (!HasPendingSubEtapaOrderChanges || IsBusy) return;

        SetIsBusy(true);
        try
        {
            // Los NumeroSubEtapa ya están actualizados en la colección local 'SubEtapas'.
            // Persistimos estos cambios.
            for (int i = 0; i < SubEtapas.Count; i++)
            {
                await _dataService.SaveSubEtapa(SubEtapas[i]);
            }

            HasPendingSubEtapaOrderChanges = false;
            await Shell.Current.DisplayAlert("Éxito", "El orden de las sub-etapas ha sido guardado.", "OK");

            // Recargar para asegurar consistencia desde la fuente de datos.
            await CargarSubEtapasAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[RegistrarSubEtapaVM] Error en SaveSubEtapaOrderChangesAsync: {ex.Message}");
            await Shell.Current.DisplayAlert("Error", $"No se pudo guardar el orden de las sub-etapas: {ex.Message}", "OK");
            await CargarSubEtapasAsync(); // Reintentar cargar para un estado consistente
        }
        finally
        {
            SetIsBusy(false);
        }
    }

    // Llamado desde la vista (Page.OnAppearing)
    public async Task OnPageAppearing()
    {
        Debug.WriteLine($"[RegistrarSubEtapaVM.OnPageAppearing] IdEtapa: {IdEtapa}");
        if (IdEtapa != 0 && !HasPendingSubEtapaOrderChanges) // Solo recargar si no hay cambios pendientes
        {
            await CargarSubEtapasAsync();
        }
        // Si hay cambios pendientes, el usuario debe guardarlos explícitamente.
        // La lógica en ApplyQueryAttributes ya maneja esto parcialmente.
    }
}