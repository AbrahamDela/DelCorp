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
    [ObservableProperty] private string actividadSubEtapa;
    [ObservableProperty] private decimal? cantidadSubEtapa;
    [ObservableProperty] private decimal? precioUniSubEtapa;
    [ObservableProperty] private decimal? precioUniEjeSubEtapa;
    [ObservableProperty] private decimal? totalSubEstapa;
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
    public record SubEtapaRegistradaMessage(long IdEtapa);


    public bool IsNotBusy => !IsBusy;

    public RegistrarSubEtapaViewModel(IDataService dataService)
    {
        _dataService = dataService;
    }

    public async void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("idEtapa", out var idObj) && long.TryParse(idObj.ToString(), out var etapaIdVal)) //
        {
            if (IdEtapa != etapaIdVal || !SubEtapas.Any())
            {
                IdEtapa = etapaIdVal;
                await CargarDatosInicialesAsync();
            }
        }
    }

    private async Task CargarDatosInicialesAsync()
    {
        await CargarUniMedReAsync();
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
        if (string.IsNullOrWhiteSpace(ActividadSubEtapa) || CantidadSubEtapa == null)
        {
            await Shell.Current.DisplayAlert("Error", "Completa todos los campos obligatorios.", "OK");
            return;
        }

        IsBusy = true;
        try
        {
            long id = OfflineFirstDataService.GenerarIdAleatorio(); // Generar ID aleatorio

            var subEtapa = new SubEtapa
            {
                Id = id, //Metodo de ultimo recurso ya que en supabase el id siempre es 0 IMPORTANTE: mejorarlo cuando tenga tiempo
                ActividadSubEtapa = ActividadSubEtapa,
                CantidadSubEtapa = CantidadSubEtapa,
                PrecioUniSubEtapa = PrecioUniSubEtapa,
                PrecioUniEjeSubEtapa = PrecioUniEjeSubEtapa,
                //TotalSubEstapa = TotalSubEstapa,
                MontoEjeSubEtapa = MontoEjeSubEtapa,
                DiasCalSubEtapa = DiasCalSubEtapa,
                DiasEjeSubEtapa = DiasEjeSubEtapa,
                IdEtapa = IdEtapa,
                IdUniMedida = IdUniMedida,
                CreatedAt = System.DateTime.UtcNow
            };

            await _dataService.SaveSubEtapa(subEtapa);
            await Shell.Current.DisplayAlert("Éxito", "Subetapa guardada correctamente.", "OK");
            ActividadSubEtapa = string.Empty;
            CantidadSubEtapa = null;
            PrecioUniSubEtapa = null;
            PrecioUniEjeSubEtapa = null;
            TotalSubEstapa = null;
            MontoEjeSubEtapa = null;
            DiasCalSubEtapa = null;
            DiasEjeSubEtapa = null;
            IdUniMedida = null;
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

    public async Task CargarSubEtapasAsync()
    {
        IsBusy = true;
        try
        {
            var lista = await _dataService.GetSubEtapasByEtapaId(IdEtapa); //
            System.Diagnostics.Debug.WriteLine($"[CargarSubEtapasAsync] Total de sub etapas: {lista.Count()}"); //
            SubEtapas.Clear();
            foreach (var sub in lista.OrderBy(s => s.CreatedAt)) // Order by CreatedAt or NumeroSubEtapa
                SubEtapas.Add(sub);
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
        if (subEtapa == null || subEtapa.Id == 0) // subEtapa.Id deberia ser el ServerId si está sincronizado
        {
            // Si subEtapa.Id es un LocalId porque aún no se ha sincronizado,
            // no poras crear FKs en Supabase. Deberia sincronizar la subEtapa primero.
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