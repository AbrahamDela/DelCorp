using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DelCorp.Models;
using DelCorp.Services;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using static DelCorp.ViewModels.RegistrarSubEtapaViewModel;

namespace DelCorp.ViewModels;

public partial class RegistrarEtapaViewModel : ObservableObject, IQueryAttributable
{
    private readonly IDataService _dataService;

    [ObservableProperty]
    private decimal? cantidadEtapa;

    [ObservableProperty]
    private decimal? montoTotalEtapa;

    [ObservableProperty]
    private int idPresupuesto;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private ObservableCollection<Etapa> etapas = new();

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

    public bool IsNotBusy => !IsBusy;

    public RegistrarEtapaViewModel(IDataService dataService)
    {
        _dataService = dataService;
    }

    public async Task ActualizarEtapaConSubetapas(long idEtapa)
    {
        // Si tienes una propiedad de subetapas en la etapa, recárgala aquí.
        // O recarga la lista de etapas si es necesario.
        await CargarEtapasAsync();
    }

    private async Task Initialize(int presupuestoId)
    {
        IdPresupuesto = presupuestoId;
        System.Diagnostics.Debug.WriteLine($"Se inicio con el id: {IdPresupuesto}");
        await CargarUniMedReAsync();
        await CargarCategoriasActividadAsync(); // Cargar categorías primero
        // CargarActividadesAsync se llamará cuando cambie SelectedCategoriaActividad
        // o podrías cargar todas inicialmente si no hay categorías o si es preferible
        // await CargarActividadesAsync(null); // Para cargar todas inicialmente
        await CargarEtapasAsync();
    }

    private async Task CargarUniMedReAsync()
    {
        IsBusy = true;
        try
        {
            var unidades = await _dataService.GetUniMedReAsync(); //
            DisponibleUniMedRe.Clear();
            foreach (var unidad in unidades)
            {
                DisponibleUniMedRe.Add(unidad);
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"No se pudo cargar las unidades de medida: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false; // Asegúrate de que IsBusy se establezca en false en todos los caminos
        }
    }

    public async Task CargarEtapasAsync()
    {
        IsBusy = true;
        try
        {
            var lista = await _dataService.GetEtapasByPresupuestoId(IdPresupuesto);
            Etapas.Clear();
            foreach (var etapa in lista.OrderBy(e => e.CreatedAt))
            {
                RecalcularTotalesParaEtapa(etapa); // Asegurar que los totales estén bien después de cargar
                Etapas.Add(etapa);
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"No se pudo cargar las etapas: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
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
            else // Si el ID es el mismo, pero podríamos necesitar refrescar (ej. al volver)
            {
                await CargarEtapasAsync(); // Podría ser una opción si OnAppearing no es suficiente
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

    // Método invocado cuando SelectedCategoriaActividad cambia
    async partial void OnSelectedCategoriaActividadChanged(CategoriaActividad value)
    {
        await CargarActividadesAsync(value?.IdCategoriaActividad);
        SelectedActividad = null; // Resetea la actividad seleccionada
        NombreNuevaActividad = string.Empty; // Limpia el campo de nueva actividad
        MostrarCampoNuevaActividad = false; // Oculta el campo
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
                NombreNuevaActividad = value.NombreActividad; // Opcional: rellenar el campo de texto
            }
        }
    }

    private async Task CargarCategoriasActividadAsync()
    {
        if (IsBusy && CategoriasActividad.Any()) return;
        IsBusy = true;
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
            IsBusy = false;
        }
    }

    private async Task CargarActividadesAsync(long? categoriaId)
    {
        // No establecer IsBusy aquí si es llamado por OnSelectedCategoriaActividadChanged,
        // ya que CargarCategoriasActividadAsync ya maneja IsBusy.
        // Si se llama independientemente, sí gestionar IsBusy.
        // bool DueloDeIsBusy = IsBusy; if (!DueloDeIsBusy) IsBusy = true;
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
            // Añadir una opción placeholder para "Otra actividad..." si lo deseas
            // Esto requiere que tu Picker pueda manejar un tipo diferente o un valor especial.
            // Por simplicidad, asumiremos que si no se selecciona nada y se escribe en NombreNuevaActividad, es nueva.
            // O puedes añadir un objeto Actividad con un ID especial (ej. -1) a la lista.
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

    public async Task ActualizarCalculosEtapa(long idEtapa)
    {
        var etapaAActualizar = Etapas.FirstOrDefault(e => e.Id == idEtapa);
        if (etapaAActualizar != null)
        {
            var subEtapasActualizadas = await _dataService.GetSubEtapasByEtapaId(idEtapa);
            etapaAActualizar.SubEtapas = subEtapasActualizadas.ToList();
            RecalcularTotalesParaEtapa(etapaAActualizar);
            // Forzar actualización de UI si es necesario
        }
    }

    private void RecalcularTotalesParaEtapa(Etapa etapa)
    {
        if (etapa == null) return;
        etapa.SubEtapas ??= new List<SubEtapa>();

        etapa.MontoTotalEtapa = etapa.SubEtapas.Sum(s => s.TotalSubEstapa ?? 0M);
        etapa.CantidadEtapa = etapa.SubEtapas.Sum(s => s.CantidadSubEtapa ?? 0M); // Ajustar si se implementa selectividad
    }

    [RelayCommand]
    public async Task GuardarEtapaAsync()
    {
        long? idActividadParaGuardar = SelectedActividad?.IdActividad;

        if (SelectedActividad != null && SelectedActividad.IdActividad == -1) // "Registrar nueva actividad..."
        {
            if (string.IsNullOrWhiteSpace(NombreNuevaActividad))
            {
                await Shell.Current.DisplayAlert("Error", "El nombre de la nueva actividad es obligatorio.", "OK");
                return;
            }
            // Crear y guardar la nueva actividad
            var nuevaActividadDto = new Actividad
            {
                NombreActividad = NombreNuevaActividad,
                CategoriaActividadId = SelectedCategoriaActividad?.IdCategoriaActividad
                // Asegúrate que SelectedCategoriaActividad esté disponible y sea correcto
            };
            var actividadGuardada = await _dataService.SaveActividadAsync(nuevaActividadDto);
            if (actividadGuardada == null || actividadGuardada.IdActividad == 0)
            {
                await Shell.Current.DisplayAlert("Error", "No se pudo guardar la nueva actividad. Intente seleccionar una existente o verifique la conexión.", "OK");
                return;
            }
            idActividadParaGuardar = actividadGuardada.IdActividad;
            // Opcional: Recargar actividades para incluir la nueva en el picker
            // await CargarActividadesAsync(SelectedCategoriaActividad?.IdCategoriaActividad);
        }
        else if (SelectedActividad == null && !string.IsNullOrWhiteSpace(NombreNuevaActividad))
        {
            // El usuario no seleccionó del picker pero escribió un nombre.
            // Podrías buscar si ya existe una actividad con ese nombre o directamente crearla.
            // Por simplicidad, si escribió y no seleccionó "Otra", asumimos que quiere una actividad existente
            // o que el flujo es seleccionar "Otra" para crear.
            // Aquí, si SelectedActividad es null pero hay texto, es ambiguo.
            // Para que funcione como "escribirla si no existe", el flujo debería ser:
            // 1. Escribe en NombreNuevaActividad
            // 2. (Opcional) Picker de Actividad se filtra o muestra "Crear nueva: [NombreNuevaActividad]"
            // 3. Al guardar, si no hay SelectedActividad pero hay NombreNuevaActividad, se intenta crear.
            // La lógica actual con el placeholder IdActividad=-1 es más explícita.
            await Shell.Current.DisplayAlert("Error", "Seleccione una actividad de la lista o elija 'Registrar nueva actividad...' y escriba el nombre.", "OK");
            return;
        }


        if (!idActividadParaGuardar.HasValue || idActividadParaGuardar.Value == 0 || (idActividadParaGuardar.Value == -1 && string.IsNullOrWhiteSpace(NombreNuevaActividad)))
        {
            await Shell.Current.DisplayAlert("Error", "La actividad es obligatoria. Seleccione una o registre una nueva.", "OK");
            return;
        }

        if (SelectedUniMedRe == null)
        {
            await Shell.Current.DisplayAlert("Error", "Debe seleccionar una Unidad de Medida.", "OK");
            return;
        }

        IsBusy = true;
        try
        {
            // ... (lógica para generar nuevoId de etapa) ...
            var etapasExistentes = await _dataService.GetEtapasByPresupuestoId(IdPresupuesto);
            var idsExistentes = etapasExistentes.Select(e => e.Id).ToHashSet();
            long nuevoId;
            var random = new Random();
            do
            {
                nuevoId = random.NextInt64(1_000_000, 9_999_999_999);
            } while (idsExistentes.Contains(nuevoId));

            var etapa = new Etapa
            {
                Id = nuevoId,
                IdActividadEtapa = idActividadParaGuardar, // Usar el ID de la actividad
                IdPresupuesto = this.IdPresupuesto,
                IdUniMedida = this.SelectedUniMedRe.Id,
                CreatedAt = DateTime.UtcNow,
                MontoTotalEtapa = 0, // Inicialmente 0
                CantidadEtapa = 0   // Inicialmente 0
            };

            await _dataService.SaveEtapa(etapa);

            // Limpiar campos
            SelectedCategoriaActividad = null; // Esto debería disparar OnSelectedCategoriaActividadChanged y limpiar actividades
            // SelectedActividad ya se limpia en OnSelectedCategoriaActividadChanged
            // NombreNuevaActividad ya se limpia en OnSelectedCategoriaActividadChanged
            SelectedUniMedRe = null;

            await CargarEtapasAsync();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"No se pudo guardar la etapa: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
