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
    private string actividadEtapa;

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


    [RelayCommand]
    public async Task GuardarEtapaAsync()
    {
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
        if (SelectedUniMedRe == null)
        {
            await Shell.Current.DisplayAlert("Error", "Debe seleccionar una Unidad de Medida.", "OK");
            return;
        }

        IsBusy = true;
        try
        {
            // Generar un id aleatorio unico  CORREGIR EN EL FUTURO
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
                ActividadEtapa = ActividadEtapa,
                CantidadEtapa = CantidadEtapa,
                IdPresupuesto = IdPresupuesto,
                IdUniMedida = SelectedUniMedRe.Id
            };

            await _dataService.SaveEtapa(etapa);
            Etapas.Add(etapa);

            // Limpiar campos
            ActividadEtapa = string.Empty;
            CantidadEtapa = null;
            MontoTotalEtapa = null;
            SelectedUniMedRe = null;
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


    private async Task Initialize(int presupuestoId)
    {
        IdPresupuesto = presupuestoId;
        System.Diagnostics.Debug.WriteLine($"Se inicio con el id: {IdPresupuesto}");
        await CargarUniMedReAsync();
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
            System.Diagnostics.Debug.WriteLine($"Numero de etapas: {lista.Count()}");
            Etapas.Clear();
            foreach (var etapa in lista.OrderBy(e => e.NumeroEtapa))
            {
                System.Diagnostics.Debug.WriteLine($"Etapa agregada a la lista: {etapa.Id}");
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
        if (query.ContainsKey("id"))
        {
            var presupuestoId = int.Parse(query["id"].ToString());
            await Initialize(presupuestoId);
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

}
