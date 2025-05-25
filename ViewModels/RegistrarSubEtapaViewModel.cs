using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DelCorp.Models;
using DelCorp.Services;
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
            var subEtapa = new SubEtapa
            {
                ActividadSubEtapa = ActividadSubEtapa,
                CantidadSubEtapa = CantidadSubEtapa,
                PrecioUniSubEtapa = PrecioUniSubEtapa,
                PrecioUniEjeSubEtapa = PrecioUniEjeSubEtapa,
                TotalSubEstapa = TotalSubEstapa,
                MontoEjeSubEtapa = MontoEjeSubEtapa,
                DiasCalSubEtapa = DiasCalSubEtapa,
                DiasEjeSubEtapa = DiasEjeSubEtapa,
                IdEtapa = IdEtapa,
                IdUniMedida = IdUniMedida
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
            var lista = await _dataService.GetSubEtapasByEtapaId(IdEtapa);
            System.Diagnostics.Debug.WriteLine($"[CargarSubEtapasAsync] Total de sub etapas: {lista.Count()}");
            SubEtapas.Clear();
            foreach (var sub in lista)
                SubEtapas.Add(sub);
        }
        finally
        {
            IsBusy = false;
        }
    }
}

