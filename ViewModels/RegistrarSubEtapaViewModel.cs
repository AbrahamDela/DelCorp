using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DelCorp.Models;
using DelCorp.Services;
using DelCorp.Views; // Required for nameof(RegistrarRecursoUtiPage)
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
    public record SubEtapaRegistradaMessage(long IdEtapa);


    public bool IsNotBusy => !IsBusy;

    public RegistrarSubEtapaViewModel(IDataService dataService)
    {
        _dataService = dataService;
    }

    public async void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("idEtapa", out var idObj) && long.TryParse(idObj.ToString(), out var idEtapa))
        {
            IdEtapa = idEtapa;
            await CargarSubEtapasAsync();
        }
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
                TotalSubEstapa = TotalSubEstapa, // This should be calculated
                MontoEjeSubEtapa = MontoEjeSubEtapa,
                DiasCalSubEtapa = DiasCalSubEtapa,
                DiasEjeSubEtapa = DiasEjeSubEtapa,
                IdEtapa = IdEtapa,
                IdUniMedida = IdUniMedida,
                CreatedAt = System.DateTime.UtcNow // Ensure CreatedAt is set
            };
            // Calculate TotalSubEstapa if applicable
            if (subEtapa.CantidadSubEtapa.HasValue && subEtapa.PrecioUniSubEtapa.HasValue)
            {
                subEtapa.TotalSubEstapa = subEtapa.CantidadSubEtapa.Value * subEtapa.PrecioUniSubEtapa.Value;
            }


            await _dataService.SaveSubEtapa(subEtapa); //
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
        if (subEtapa == null || subEtapa.Id == 0) // subEtapa.Id debería ser el ServerId si está sincronizado
        {
            // Si subEtapa.Id es un LocalId porque aún no se ha sincronizado,
            // no podrás crear FKs en Supabase. Deberías sincronizar la subEtapa primero.
            await Shell.Current.DisplayAlert("Error", "La SubEtapa no es válida o necesita ser sincronizada primero para agregar recursos.", "OK");
            return;
        }
        IsBusy = true;
        try
        {
            // Ensure AppShell has RegistrarRecursoUtiPage registered
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