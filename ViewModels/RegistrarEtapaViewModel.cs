using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DelCorp.Models;
using DelCorp.Services;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Diagnostics;

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

    [ObservableProperty]
    private bool _showNewActividadFields;

    [ObservableProperty]
    private bool _isActividadFromList;

    [ObservableProperty]
    private bool _hasPendingOrderChanges = false; // Nueva propiedad

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

    // ... (CargarUniMedReAsync, CargarCategoriasActividadAsync, CargarActividadesAsync, etc. sin cambios) ...
    async partial void OnSearchTextActividadChanged(string oldValue, string newValue)
    {
        // Si el cambio proviene de la selección en la lista, no recargar
        if (IsActividadFromList)
        {
            IsActividadFromList = false;
            return;
        }

        if (string.IsNullOrWhiteSpace(newValue))
        {
            ActividadesDisponibles.Clear();
            ShowNewActividadFields = false;
            SelectedActividad = null;
            return;
        }

        await CargarActividadesAsync(null, newValue);

        var exactMatch = ActividadesDisponibles
            .FirstOrDefault(a => a.NombreActividad.Equals(newValue, StringComparison.OrdinalIgnoreCase));

        if (exactMatch != null && exactMatch.IdActividad != -1)
        {
            ShowNewActividadFields = false;
            SelectedActividad = exactMatch;
        }
        else
        {
            ShowNewActividadFields = true;
            SelectedActividad = null;
        }
    }

    [RelayCommand]
    private void SelectActividad(Actividad actividad)
    {
        if (actividad == null) return;

        IsActividadFromList = true;
        SearchTextActividad = actividad.NombreActividad;
        SelectedActividad = actividad;

        ShowNewActividadFields = false;
        ActividadesDisponibles.Clear();
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
            foreach (var etapa in listaEtapasDto.OrderBy(e => e.NumeroEtapa))
            {
                RecalcularTotalesParaEtapa(etapa);
                Etapas.Add(etapa);
            }
            HasPendingOrderChanges = false; // Resetea la bandera después de cargar desde la fuente
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
            // Si hay cambios pendientes de orden y el usuario navega de nuevo a esta página para el mismo presupuesto,
            // podríamos querer preguntarle si quiere descartar los cambios o guardarlos.
            // Por ahora, si el IdPresupuesto es el mismo, simplemente recargamos.
            // Si el IdPresupuesto es diferente, o si la lista está vacía, inicializamos completamente.
            if (IdPresupuesto != presupuestoId || !Etapas.Any())
            {
                await Initialize(presupuestoId);
            }
            else if (HasPendingOrderChanges)
            {
                // Opcional: Preguntar al usuario si desea guardar o descartar cambios de orden
                // bool discard = await Shell.Current.DisplayAlert("Cambios pendientes", "Hay cambios en el orden de las etapas sin guardar. ¿Desea descartarlos y recargar?", "Descartar", "Cancelar");
                // if (discard) { await CargarEtapasAsync(); }
                // else { // El usuario canceló, la lista se mantiene como está con cambios pendientes }

                // Por simplicidad, recargamos y se pierden los cambios de orden no guardados.
                // O, podríamos simplemente no hacer nada si es el mismo presupuesto y hay cambios pendientes,
                // permitiendo al usuario continuar y guardar explícitamente.
                // Por ahora, para evitar pérdida de datos no guardados, no recargamos si hay cambios pendientes para el mismo presupuesto.
                Debug.WriteLine($"[RegistrarEtapaVM.ApplyQueryAttributes] Mismo presupuesto (Id: {IdPresupuesto}), cambios de orden pendientes. No se recargan etapas automáticamente.");
            }
            else
            {
                // Mismo presupuesto, sin cambios pendientes, se puede recargar.
                await CargarEtapasAsync();
            }
        }
        // Resetear el estado de cambios pendientes si se navega a un presupuesto diferente
        if (query.TryGetValue("id", out var idObjNew) && int.TryParse(idObjNew.ToString(), out var nuevoPresupuestoId))
        {
            if (IdPresupuesto != nuevoPresupuestoId)
            {
                HasPendingOrderChanges = false;
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
        // Antes de navegar, verificar si hay cambios de orden pendientes y preguntar al usuario
        if (HasPendingOrderChanges)
        {
            bool proceed = await Shell.Current.DisplayAlert("Cambios sin guardar",
                "El orden de las etapas ha cambiado. ¿Desea guardar estos cambios antes de continuar? Si no guarda, se perderán.",
                "Guardar y Continuar", "Continuar sin Guardar");

            if (proceed) // "Guardar y Continuar"
            {
                await SaveOrderChangesAsync();
                // Verificar si el guardado fue exitoso (IsBusy = false y no hubo error) podría ser necesario antes de navegar.
                // Por ahora, asumimos que SaveOrderChangesAsync maneja errores y el estado IsBusy.
                if (IsBusy) // Si SaveOrderChangesAsync aún está ocupado (p.ej. por un error que no reseteó IsBusy)
                {
                    await Shell.Current.DisplayAlert("Guardado en progreso", "Espere a que termine el guardado.", "OK");
                    return;
                }
            }
            // Si elige "Continuar sin Guardar", los cambios de HasPendingOrderChanges se perderán al volver a cargar.
            // Podríamos resetear HasPendingOrderChanges aquí si no se guardan.
            // HasPendingOrderChanges = false; // Si continúan sin guardar
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
            PickersForNewActivityEnabled = true;
            SelectedCategoriaActividad = null;
            SelectedUniMedRe = null;
        }
        else if (value != null)
        {
            MostrarCampoNuevaActividad = false;
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

        if (string.IsNullOrWhiteSpace(SearchTextActividad))
        {
            await Shell.Current.DisplayAlert("Validación", "Debe escribir o seleccionar una actividad para la etapa.", "OK");
            SetIsBusy(false);
            return;
        }

        if (ShowNewActividadFields)
        {
            if (SelectedCategoriaActividad == null || SelectedUniMedRe == null)
            {
                await Shell.Current.DisplayAlert("Validación", "Para registrar una nueva actividad, debe seleccionar su categoría y unidad de medida.", "OK");
                SetIsBusy(false);
                return;
            }

            var nuevaActividadDto = new Actividad
            {
                NombreActividad = SearchTextActividad,
                CategoriaActividadId = SelectedCategoriaActividad.IdCategoriaActividad,
                UnidadMedidaId = SelectedUniMedRe.Id,
                CreatedAt = DateTime.UtcNow
            };

            var actividadGuardada = await _dataService.SaveActividadAsync(nuevaActividadDto);
            if (actividadGuardada == null || actividadGuardada.IdActividad == 0)
            {
                await Shell.Current.DisplayAlert("Error", "No se pudo guardar la nueva actividad.", "OK");
                SetIsBusy(false);
                return;
            }
            idActividadParaGuardar = actividadGuardada.IdActividad;
        }
        else
        {
            if (SelectedActividad == null)
            {
                await Shell.Current.DisplayAlert("Error", "La actividad seleccionada no es válida. Por favor, elíjala de la lista.", "OK");
                SetIsBusy(false);
                return;
            }
            idActividadParaGuardar = SelectedActividad.IdActividad;
        }

        if (!idActividadParaGuardar.HasValue)
        {
            await Shell.Current.DisplayAlert("Error", "No se pudo determinar la actividad para la etapa.", "OK");
            SetIsBusy(false);
            return;
        }

        try
        {
            long nuevoIdEtapaLocal = OfflineFirstDataService.GenerarIdAleatorio();
            long nuevoNumeroEtapa = (Etapas.Any() ? Etapas.Max(e => e.NumeroEtapa) : 0) + 1;

            var etapa = new Etapa
            {
                Id = nuevoIdEtapaLocal,
                IdActividadEtapa = idActividadParaGuardar.Value,
                IdPresupuesto = this.IdPresupuesto,
                CantidadEtapa = this.CantidadEtapa,
                MontoTotalEtapa = 0,
                NumeroEtapa = nuevoNumeroEtapa,
                CreatedAt = DateTime.UtcNow
            };

            var etapaGuardada = await _dataService.SaveEtapa(etapa);
            if (etapaGuardada != null)
            {
                SearchTextActividad = string.Empty;
                ShowNewActividadFields = false;
                SelectedActividad = null;
                SelectedCategoriaActividad = null;
                SelectedUniMedRe = null;
                CantidadEtapa = null;

                await CargarEtapasAsync(); // Recarga la lista de etapas
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

    // --- NUEVOS MÉTODOS Y COMANDOS PARA REORDENAMIENTO OPTIMIZADO ---

    private void LocalRenumberEtapas()
    {
        for (int i = 0; i < Etapas.Count; i++)
        {
            if (Etapas[i].NumeroEtapa != (i + 1))
            {
                Etapas[i].NumeroEtapa = i + 1; // Actualiza la propiedad en el objeto Etapa
                                               // Si Etapa es ObservableObject, la UI se actualiza.
            }
        }

        // Si Etapa NO es ObservableObject, y necesitas forzar el refresco de NumeroEtapa en la UI:
        // (Descomenta y adapta si es tu caso, aunque convertir Etapa a ObservableObject es preferible)
        /*
        var tempList = Etapas.ToList(); // Copia de la lista ya ordenada físicamente
        // Los NumeroEtapa ya fueron actualizados en el bucle anterior sobre Etapas[i]
        Etapas.Clear();
        foreach(var etapa in tempList)
        {
            Etapas.Add(etapa);
        }
        */
    }

    [RelayCommand]
    private void MoveEtapaUp(Etapa etapa) // Ya no es async si solo modifica localmente
    {
        if (etapa == null || IsBusy) return;
        int currentIndex = Etapas.IndexOf(etapa);
        if (currentIndex > 0)
        {
            Etapas.Move(currentIndex, currentIndex - 1);
            LocalRenumberEtapas();
            HasPendingOrderChanges = true;
        }
    }

    [RelayCommand]
    private void MoveEtapaDown(Etapa etapa) // Ya no es async si solo modifica localmente
    {
        if (etapa == null || IsBusy) return;
        int currentIndex = Etapas.IndexOf(etapa);
        if (currentIndex < Etapas.Count - 1)
        {
            Etapas.Move(currentIndex, currentIndex + 1);
            LocalRenumberEtapas();
            HasPendingOrderChanges = true;
        }
    }

    [RelayCommand]
    private async Task SaveOrderChangesAsync()
    {
        if (!HasPendingOrderChanges || IsBusy) return;

        SetIsBusy(true);
        try
        {
            // Los NumeroEtapa ya están actualizados en la colección local 'Etapas'
            // gracias a LocalRenumberEtapas(). Ahora los persistimos.
            bool guardadoExitoso = true;
            for (int i = 0; i < Etapas.Count; i++)
            {
                // Podríamos añadir una comprobación para ver si el NumeroEtapa realmente cambió
                // con respecto a su valor original en la BD antes de guardar, pero
                // guardar todas las etapas en su nuevo orden numérico es más simple y robusto aquí.
                var etapaGuardada = await _dataService.SaveEtapa(Etapas[i]);
                if (etapaGuardada == null)
                {
                    guardadoExitoso = false;
                    Debug.WriteLine($"[RegistrarEtapaVM] Error guardando etapa ID {Etapas[i].Id} durante SaveOrderChangesAsync.");
                    // Podríamos decidir parar aquí o continuar guardando las demás.
                    // Por ahora, continuamos.
                }
            }

            if (guardadoExitoso)
            {
                HasPendingOrderChanges = false;
                await Shell.Current.DisplayAlert("Éxito", "El orden de las etapas ha sido guardado.", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlert("Error Parcial", "Algunas etapas no pudieron guardarse correctamente. Por favor, revise la lista.", "OK");
            }

            // Siempre recargar para asegurar consistencia, especialmente si hubo errores parciales.
            await CargarEtapasAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[RegistrarEtapaVM] Error en SaveOrderChangesAsync: {ex.Message}");
            await Shell.Current.DisplayAlert("Error", $"No se pudo guardar el orden de las etapas: {ex.Message}", "OK");
            await CargarEtapasAsync(); // Reintentar cargar para un estado consistente
        }
        finally
        {
            SetIsBusy(false);
        }
    }
}