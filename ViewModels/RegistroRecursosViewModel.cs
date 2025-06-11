using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DelCorp.Models;
using DelCorp.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace DelCorp.ViewModels
{
    [QueryProperty(nameof(IdSubEtapa), "idSubEtapa")]
    public partial class RegistroRecursosViewModel : ObservableObject
    {
        private readonly IDataService _dataService;

        [ObservableProperty] private long _idSubEtapa;
        [ObservableProperty] private SubEtapa _currentSubEtapa;
        [ObservableProperty] private bool _isBusy;

        [ObservableProperty] private ObservableCollection<RegistroRecursoUti> _recursosRegistrados;
        [ObservableProperty] private ObservableCollection<Recurso> _disponibleRecursos;
        [ObservableProperty] private ObservableCollection<UniMedRe> _disponibleUniMedRe;

        [ObservableProperty] private DateTime _fecha = DateTime.Today;
        [ObservableProperty] private Recurso _selectedRecurso;
        [ObservableProperty] private UniMedRe _selectedUniMedRe;
        [ObservableProperty] private decimal? _cantidad;
        [ObservableProperty] private decimal? _precioUnitario;
        [ObservableProperty] private decimal? _total;

        public RegistroRecursosViewModel(IDataService dataService)
        {
            _dataService = dataService;
            RecursosRegistrados = new();
            DisponibleRecursos = new();
            DisponibleUniMedRe = new();
        }

        partial void OnIdSubEtapaChanged(long value) { _ = LoadDataAsync(); }
        partial void OnCantidadChanged(decimal? value) => CalculateTotal();
        partial void OnPrecioUnitarioChanged(decimal? value) => CalculateTotal();

        private void CalculateTotal()
        {
            Total = (Cantidad ?? 0) * (PrecioUnitario ?? 0);
        }

        private async Task LoadDataAsync()
        {
            IsBusy = true;
            CurrentSubEtapa = await _dataService.GetSubEtapaByIdAsync(IdSubEtapa);
            var recursos = await _dataService.GetRecursosAsync();
            var unidades = await _dataService.GetUniMedReAsync();

            DisponibleRecursos.Clear();
            foreach (var r in recursos) DisponibleRecursos.Add(r);

            DisponibleUniMedRe.Clear();
            foreach (var u in unidades) DisponibleUniMedRe.Add(u);

            await LoadRegistrosAsync();
            IsBusy = false;
        }

        private async Task LoadRegistrosAsync()
        {
            var registros = await _dataService.GetRegistroRecursosBySubEtapaIdAsync(IdSubEtapa);
            RecursosRegistrados.Clear();
            foreach (var r in registros) RecursosRegistrados.Add(r);
        }

        [RelayCommand]
        private async Task GuardarRegistro()
        {
            if (SelectedRecurso == null || SelectedUniMedRe == null || Cantidad == null || PrecioUnitario == null)
            {
                await Shell.Current.DisplayAlert("Error", "Todos los campos son requeridos.", "OK");
                return;
            }
            IsBusy = true;
            var registro = new RegistroRecursoUti
            {
                IdSubEtapa = IdSubEtapa,
                IdRecurso = SelectedRecurso.Id,
                IdUniMedida = SelectedUniMedRe.Id,
                FechaRecursoUti = Fecha,
                CantidadRecursosUti = Cantidad,
                PrecioUniRecursosUti = PrecioUnitario,
                TotalRecursosUti = Total,
                CreatedAt = DateTime.UtcNow
            };
            await _dataService.SaveRegistroRecursoAsync(registro);
            await LoadRegistrosAsync();
            IsBusy = false;
        }
    }
}
