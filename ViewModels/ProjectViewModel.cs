using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DelCorp.Models;
using DelCorp.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Maui.Networking;
using DelCorp.Views;

namespace DelCorp.ViewModels
{
    public partial class ProjectViewModel : ObservableObject, IDisposable
    {
        private readonly IDataService _dataService;
        private readonly IConnectivity _connectivity;

        // Campos privados para manejar la paginación y búsqueda
        private List<Project> _allProjectsCache = new();
        private List<Project> _displayedProjects = new();
        private int _currentPage = 1;
        private bool _hasMoreItems = true;
        private const int PageSize = 5;

        [ObservableProperty]
        private bool _isLoadingMore;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _syncStatus = "No sincronizado";

        [ObservableProperty]
        private bool _isConnected;
        [ObservableProperty]
        private string _searchTerm = string.Empty;
        // Propiedad para rastrear si estamos en modo de búsqueda
        [ObservableProperty]
        private bool _isSearching = false;

        // Usa una colección observable para que los cambios se reflejen automáticamente
        public ObservableCollection<Project> Projects { get; } = new();

        public ProjectViewModel(IDataService dataService, IConnectivity connectivity)
        {
            _dataService = dataService;
            _connectivity = connectivity;

            // Cargar proyectos al iniciar
            LoadMoreProjectsCommand.Execute(null);

            // Monitorear cambios de conectividad
            _connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;
            IsConnected = _connectivity.NetworkAccess == NetworkAccess.Internet;
        }

        private async void Connectivity_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            // Actualizar estado de conexión
            IsConnected = e.NetworkAccess == NetworkAccess.Internet;

            // Si se restaura la conexión, sincronizar automáticamente
            if (IsConnected)
            {
                try
                {
                    await SyncProjects();
                    await RefreshProjects();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error de sincronización: {ex.Message}");
                }
            }
        }

        // Comando para buscar en caliente
        partial void OnSearchTermChanged(string value)
        {
            // Debounce para evitar búsquedas excesivas
            SearchProjectsCommand.Execute(null);
        }

        [RelayCommand]
        private async Task SearchProjects()
        {
            // Cancelar si está ocupado
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                IsSearching = !string.IsNullOrWhiteSpace(SearchTerm);

                // Limpiar la colección actual
                Projects.Clear();
                _displayedProjects.Clear();

                if (string.IsNullOrWhiteSpace(SearchTerm))
                {
                    // Si no hay término de búsqueda, volver a cargar desde el caché
                    foreach (var project in _allProjectsCache.Take(PageSize))
                    {
                        Projects.Add(project);
                        _displayedProjects.Add(project);
                    }
                    _currentPage = 1;
                    _hasMoreItems = true;
                }
                else
                {
                    // Búsqueda con filtro
                    var searchTermLower = SearchTerm.ToLower();
                    var filteredProjects = _allProjectsCache.Where(p =>
                        (p.NombreProyecto?.ToLower().Contains(searchTermLower) ?? false) ||
                        (p.DireccionProyecto?.ToLower().Contains(searchTermLower) ?? false) ||
                        (p.DescripcionProyecto?.ToLower().Contains(searchTermLower) ?? false)
                    ).ToList();

                    // Agregar resultados
                    foreach (var project in filteredProjects.Take(PageSize))
                    {
                        Projects.Add(project);
                        _displayedProjects.Add(project);
                    }
                }
            }
            catch (Exception ex)
            {
                // Manejo de errores
                await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public async Task GetProjects(bool isRefresh)
        {
            if (IsBusy)
                return;

            // Si es un refresh, reiniciar la paginación
            if (isRefresh)
            {
                _currentPage = 1;
                _hasMoreItems = true;
                Projects.Clear();
            }

            // Si no hay más elementos, salir
            if (!_hasMoreItems)
                return;

            IsBusy = true;
            IsLoadingMore = true;

            try
            {
                // Obtener proyectos paginados
                var projects = await _dataService.GetPagedProjects(_currentPage, PageSize);

                if (projects != null && projects.Any())
                {
                    // Agregar proyectos a la colección
                    foreach (var project in projects)
                    {
                        Projects.Add(project);
                    }

                    // Incrementar página si hay resultados
                    _currentPage++;

                    // Verificar si hay más elementos
                    _hasMoreItems = projects.Count == PageSize;
                }
                else
                {
                    // No hay más elementos
                    _hasMoreItems = false;
                }

                // Actualizar estado de sincronización
                var anyUnsyncedProjects = Projects.Any(p => !p.IsSynced);
                SyncStatus = anyUnsyncedProjects
                    ? "Cambios pendientes de sincronizar"
                    : "Todo sincronizado";
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
                IsLoadingMore = false;
            }
        }

        // Comando para cargar más proyectos
        [RelayCommand]
        public async Task LoadMoreProjects()
        {
            if (IsBusy || !_hasMoreItems) return;

            try
            {
                IsBusy = true;

                // Cargar más proyectos
                var newProjects = await _dataService.GetPagedProjects(_currentPage, PageSize);

                if (newProjects == null || !newProjects.Any())
                {
                    _hasMoreItems = false;
                    return;
                }

                // Agregar al caché sin duplicados
                foreach (var project in newProjects)
                {
                    if (!_allProjectsCache.Any(p => p.Id == project.Id))
                    {
                        _allProjectsCache.Add(project);
                    }
                }

                // Si no está buscando, actualizar la vista
                if (!IsSearching)
                {
                    foreach (var project in newProjects)
                    {
                        // Evitar duplicados
                        if (!_displayedProjects.Any(p => p.Id == project.Id))
                        {
                            Projects.Add(project);
                            _displayedProjects.Add(project);
                        }
                    }
                }

                _currentPage++;
                _hasMoreItems = newProjects.Count == PageSize;
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        // Comando para refrescar la lista
        [RelayCommand]
        public async Task RefreshProjects()
        {
            // Reiniciar todo
            _currentPage = 1;
            _hasMoreItems = true;
            _allProjectsCache.Clear();
            _displayedProjects.Clear();
            Projects.Clear();
            SearchTerm = string.Empty;
            IsSearching = false;

            await LoadMoreProjects();
        }

        [RelayCommand]
        public async Task SyncProjects()
        {
            if (IsBusy || !IsConnected)
                return;

            IsBusy = true;
            SyncStatus = "Sincronizando...";

            try
            {
                // Intentar sincronizar
                var success = await _dataService.SyncProjects();
                SyncStatus = success ? "Sincronizado" : "Error al sincronizar";

                // Recargar para mostrar estado actualizado
                await GetProjects(true);
            }
            catch (Exception ex)
            {
                SyncStatus = "Error al sincronizar";
                await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public async Task DeleteProject(Project project)
        {
            if (IsBusy)
                return;

            var confirm = await Shell.Current.DisplayAlert("Confirmar",
                "¿Está seguro de que desea eliminar este proyecto?", "Sí", "No");

            if (!confirm)
                return;

            IsBusy = true;

            try
            {
                // Eliminar proyecto
                var success = await _dataService.DeleteProject(project.Id);

                if (success)
                {
                    Projects.Remove(project);
                    await GetProjects(true); // Recargar para asegurarnos de tener datos actualizados
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "No se pudo eliminar el proyecto", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        async Task RegistrarProyecto()
        {
            await Shell.Current.GoToAsync("AddProjectPage");
        }

        public void Dispose()
        {
            // Limpiar suscriptores de eventos
            _connectivity.ConnectivityChanged -= Connectivity_ConnectivityChanged;
        }

        // Método para navegar a los detalles del proyecto
        [RelayCommand]
        private async Task NavigateToProjectDetails(Project project)
        {
            if (project == null)
                return;

            try
            {
                var uri = $"{nameof(ProjectDetailPage)}?id={project.Id}";
                await Shell.Current.GoToAsync(uri);
            }
            catch (Exception ex)
            {
                // Manejar cualquier error de navegación
                await Shell.Current.DisplayAlert(
                    "Error",
                    $"No se pudo navegar: {ex.Message}",
                    "OK"
                );
            }
        }
    }
}
