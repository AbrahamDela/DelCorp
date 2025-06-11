using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DelCorp.Models;
using DelCorp.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace DelCorp.ViewModels
{
    [QueryProperty(nameof(IdSubEtapa), "idSubEtapa")]
    public partial class RegistrarRecursoEjecutadoViewModel : ObservableObject
    {
        private readonly IDataService _dataService;

        [ObservableProperty] private long _idSubEtapa;
        [ObservableProperty] private bool _isBusy;
        [ObservableProperty] private string _errorMessage;

        // Form properties
        [ObservableProperty] private DateTime _fechaRecurso = DateTime.Today;
        [ObservableProperty] private CategoriaRec _selectedCategoriaRec;
        [ObservableProperty] private Recurso _selectedRecurso;
        [ObservableProperty] private UniMedRe _selectedUniMedRe;
        [ObservableProperty] private decimal? _cantidad;
        [ObservableProperty] private decimal? _precioUnitario;
        [ObservableProperty] private decimal? _totalCalculado;

        // Collections
        [ObservableProperty] private ObservableCollection<RegistroRecursoUti> _recursosEjecutados = new();
        [ObservableProperty] private ObservableCollection<CategoriaRec> _disponibleCategoriasRec = new();
        [ObservableProperty] private ObservableCollection<Recurso> _disponibleRecursos = new();
        [ObservableProperty] private ObservableCollection<UniMedRe> _disponibleUniMedRe = new();

        public bool IsNotBusy => !IsBusy;
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        public RegistrarRecursoEjecutadoViewModel(IDataService dataService)
        {
            _dataService = dataService;
        }

        async partial void OnIdSubEtapaChanged(long value)
        {
            if (value > 0)
            {
                await LoadInitialDataAsync();
            }
        }

        async partial void OnSelectedCategoriaRecChanged(CategoriaRec value)
        {
            await LoadRecursosAsync();
        }

        partial void OnCantidadChanged(decimal? value) => CalculateTotal();
        partial void OnPrecioUnitarioChanged(decimal? value) => CalculateTotal();
        partial void OnErrorMessageChanged(string value) => OnPropertyChanged(nameof(HasError));

        private void CalculateTotal()
        {
            TotalCalculado = Cantidad.HasValue && PrecioUnitario.HasValue ? Cantidad * PrecioUnitario : null;
        }

        private async Task LoadInitialDataAsync()
        {
            IsBusy = true;
            await LoadCategoriasRecAsync();
            await LoadRecursosAsync();
            await LoadUniMedReAsync();
            await LoadRecursosEjecutadosAsync();
            IsBusy = false;
        }

        private async Task LoadRecursosEjecutadosAsync()
        {
            var items = await _dataService.GetRegistrosRecursoUtiBySubEtapaIdAsync(IdSubEtapa);
            RecursosEjecutados.Clear();
            foreach (var item in items)
            {
                RecursosEjecutados.Add(item);
            }
        }

        private async Task LoadCategoriasRecAsync()
        {
            var items = await _dataService.GetCategoriasRecAsync();
            DisponibleCategoriasRec.Clear();
            foreach (var item in items) DisponibleCategoriasRec.Add(item);
        }

        private async Task LoadRecursosAsync()
        {
            var items = await _dataService.GetRecursosAsync(SelectedCategoriaRec?.Id);
            DisponibleRecursos.Clear();
            foreach (var item in items) DisponibleRecursos.Add(item);
        }

        private async Task LoadUniMedReAsync()
        {
            var items = await _dataService.GetUniMedReAsync();
            DisponibleUniMedRe.Clear();
            foreach (var item in items) DisponibleUniMedRe.Add(item);
        }


        [RelayCommand]
        private async Task AddRecurso()
        {
            if (SelectedRecurso == null || SelectedUniMedRe == null || !Cantidad.HasValue || !PrecioUnitario.HasValue)
            {
                ErrorMessage = "Por favor, complete todos los campos.";
                return;
            }

            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                var newRegistro = new RegistroRecursoUti
                {
                    FechaRecursoUti = FechaRecurso,
                    IdSubEtapa = IdSubEtapa,
                    IdRecurso = SelectedRecurso.Id,
                    IdUniMedida = SelectedUniMedRe.Id,
                    CantidadRecursosUti = Cantidad,
                    PrecioUniRecursosUti = PrecioUnitario,
                    TotalRecursosUti = TotalCalculado,
                    CreatedAt = DateTime.UtcNow
                };

                var savedItem = await _dataService.SaveRegistroRecursoUtiAsync(newRegistro);

                savedItem.Recurso = SelectedRecurso;
                savedItem.UniMedRe = SelectedUniMedRe;
                RecursosEjecutados.Add(savedItem);

                await UpdateExecutionTotalsAsync();

                // Clear form
                SelectedRecurso = null;
                SelectedUniMedRe = null;
                Cantidad = null;
                PrecioUnitario = null;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al guardar: {ex.Message}";
                Debug.WriteLine(ErrorMessage);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task DeleteRecurso(RegistroRecursoUti registro)
        {
            if (registro == null) return;
            IsBusy = true;
            try
            {
                await _dataService.DeleteRegistroRecursoUtiAsync(registro.Id);
                RecursosEjecutados.Remove(registro);
                await UpdateExecutionTotalsAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al eliminar: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task UpdateExecutionTotalsAsync()
        {
            await _dataService.UpdateExecutionTotalsForSubEtapaAsync(IdSubEtapa);
        }
    }
}
