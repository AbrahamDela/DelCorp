using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DelCorp.Models;
using DelCorp.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using DelCorp.Views;

namespace DelCorp.ViewModels
{
    public partial class PresupuestoViewModel : ObservableObject
    {
        private readonly IDataService _dataService;
        private readonly IConnectivity _connectivity;

        [ObservableProperty]
        private ObservableCollection<Presupuesto> presupuestos = new();

        [ObservableProperty]
        private bool isLoadingMore;

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private string searchTerm = string.Empty;

        [ObservableProperty]
        private bool isSearching;

        [ObservableProperty]
        private string errorMessage;

        private List<Presupuesto> _allPresupuestosCache = new();
        private int _currentPage = 1;
        private const int PageSize = 10;
        private bool _hasMoreItems = true;

        public PresupuestoViewModel(IDataService dataService, IConnectivity connectivity)
        {
            _dataService = dataService;
            _connectivity = connectivity;

            MessagingCenter.Subscribe<RegistrarPresupuestoViewModel>(this, "ActualizarPresupuestos", async (sender) =>
            {
                await RefreshPresupuestos();
            });
        }

        [RelayCommand]
        public async Task RefreshPresupuestos()
        {
            if (isBusy) return;
            isBusy = true;
            try
            {
                _currentPage = 1;
                _hasMoreItems = true;
                _allPresupuestosCache.Clear();
                Presupuestos.Clear();
                await LoadMorePresupuestos();
            }
            finally
            {
                isBusy = false;
                isLoadingMore = false;
            }
        }


        [RelayCommand]
        public async Task LoadMorePresupuestos()
        {
            if (isBusy || !_hasMoreItems) return;
            isLoadingMore = true;
            try
            {
                var allList = (await _dataService.GetAllPresupuestos())
                    .OrderByDescending(p => p.CreatedAt)
                    .ToList();

                var paged = allList.Skip((_currentPage - 1) * PageSize).Take(PageSize).ToList();

                if (_currentPage == 1)
                    _allPresupuestosCache = allList;

                if (!paged.Any())
                {
                    _hasMoreItems = false;
                    return;
                }
                foreach (var item in paged)
                {
                    item.MontoEjePresupuesto = await _dataService.GetTotalEjecutadoForPresupuestoAsync(item.Id);
                    if (!Presupuestos.Any(p => p.Id == item.Id))
                        Presupuestos.Add(item);
                }
                _currentPage++;
                _hasMoreItems = paged.Count == PageSize;
            }
            catch (Exception ex)
            {
                //ErrorMessage = $"No se pudieron cargar los presupuestos: {ex.Message}";
            }
            finally
            {
                isLoadingMore = false;
            }
        }

        [RelayCommand]
        public async Task SearchPresupuestos()
        {
            if (isBusy)
                return;

            var term = SearchTerm?.ToLowerInvariant() ?? "";
            isSearching = !string.IsNullOrWhiteSpace(term);
            Presupuestos.Clear();

            var filtered = string.IsNullOrWhiteSpace(term)
                ? _allPresupuestosCache
                : _allPresupuestosCache.Where(p =>
                        (p.NombrePresupuesto?.ToLowerInvariant().Contains(term) ?? false)
                     || p.Id.ToString().Contains(term)
                     || (p.IdProyecto.ToString().Contains(term))
                   ).ToList();

            foreach (var item in filtered)
                Presupuestos.Add(item);
        }

        [RelayCommand]
        public async Task RegistrarPresupuesto()
        {
            await Shell.Current.GoToAsync("RegistrarPresupuestoPage");
        }

        [RelayCommand]
        private async Task NavigateToPresupuestoDetails(Presupuesto presupuesto)
        {
            try
            {
                var uri = $"{nameof(RegistrarEtapaPage)}?id={presupuesto.Id}";
                await Shell.Current.GoToAsync(uri);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"No se pudo navegar a los detalles del presupuesto: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        public async Task DeletePresupuesto(Presupuesto presupuesto)
        {
            if (presupuesto == null) return;
            var confirm = await Shell.Current.DisplayAlert("Confirmar",
                $"¿Desea eliminar el presupuesto '{presupuesto.NombrePresupuesto}'?",
                "Sí", "No");

            if (!confirm) return;

            try
            {
                var result = await _dataService.DeletePresupuesto(presupuesto.Id);
                if (result)
                {
                    Presupuestos.Remove(presupuesto);
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "No se pudo eliminar el presupuesto.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", "No se pudo eliminar el presupuesto: " + ex.Message, "OK");
            }
        }
    }
}