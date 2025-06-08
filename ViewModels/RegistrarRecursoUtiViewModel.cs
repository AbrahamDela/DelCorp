using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DelCorp.Models;
using DelCorp.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DelCorp.ViewModels
{
    [QueryProperty(nameof(IdSubEtapa), "idSubEtapa")]
    public partial class RegistrarRecursoUtiViewModel : ObservableObject
    {
        private readonly IDataService _dataService;

        [ObservableProperty]
        private long _idSubEtapa;

        [ObservableProperty]
        private SubEtapa _currentSubEtapa;

        [ObservableProperty]
        private ObservableCollection<RecursoUti> _recursosUtilizados;

        [ObservableProperty]
        private ObservableCollection<Recurso> _disponibleRecursos;

        [ObservableProperty]
        private ObservableCollection<UniMedRe> _disponibleUniMedRe;

        [ObservableProperty]
        private ObservableCollection<CategoriaRec> _disponibleCategoriasRec;


        // Form properties
        [ObservableProperty]
        private Recurso _selectedRecurso;
        [ObservableProperty]
        private UniMedRe _selectedUniMedRe;
        [ObservableProperty]
        private CategoriaRec _selectedCategoriaRec;

        [ObservableProperty]
        private decimal? _cantidad;
        [ObservableProperty]
        private decimal? _precioUnitario;
        [ObservableProperty]
        private decimal? _totalCalculado;

        [ObservableProperty]
        private System.DateTime? _fechaRecursoUti = System.DateTime.Today;

        [ObservableProperty]
        private bool _isBusy;

        public bool IsNotBusy => !IsBusy;

        [ObservableProperty]
        private string _errorMessage;

        public ICommand FilterRecursosCommand { get; }


        public RegistrarRecursoUtiViewModel(IDataService dataService)
        {
            _dataService = dataService;
            RecursosUtilizados = new ObservableCollection<RecursoUti>();
            DisponibleRecursos = new ObservableCollection<Recurso>();
            DisponibleUniMedRe = new ObservableCollection<UniMedRe>();
            DisponibleCategoriasRec = new ObservableCollection<CategoriaRec>();
            FilterRecursosCommand = new AsyncRelayCommand(LoadRecursosAsync); // Re-filter on category change
        }

        partial void OnIsBusyChanged(bool value)
        {
            OnPropertyChanged(nameof(IsNotBusy));
        }

        partial void OnIdSubEtapaChanged(long value)
        {
            if (value > 0)
            {
                _ = LoadInitialDataAsync();
            }
        }

        partial void OnSelectedCategoriaRecChanged(CategoriaRec value)
        {
            // When category changes, filter the resources list
            // This assumes LoadRecursosAsync can take category as a filter
            // or you filter client-side from a master list.
            // For simplicity, triggering reload which can be optimized.
            if (FilterRecursosCommand.CanExecute(null))
            {
                FilterRecursosCommand.Execute(null);
            }
        }

        partial void OnCantidadChanged(decimal? value) => CalculateTotal();
        partial void OnPrecioUnitarioChanged(decimal? value) => CalculateTotal();

        private void CalculateTotal()
        {
            if (Cantidad.HasValue && PrecioUnitario.HasValue)
                TotalCalculado = Cantidad.Value * PrecioUnitario.Value;
            else
                TotalCalculado = null;
        }

        private async Task LoadInitialDataAsync()
        {
            IsBusy = true;
            ErrorMessage = string.Empty;
            try
            {
                // Load current SubEtapa details (optional)
                // CurrentSubEtapa = await _dataService.GetSubEtapaByIdAsync(IdSubEtapa);

                await LoadRecursosUtilizadosAsync();
                await LoadCategoriasRecAsync(); // Load categories first
                await LoadRecursosAsync();    // Then load resources (can be filtered by category)
                await LoadUniMedReAsync();
                await UpdateParentSubEtapaTotalAsync();

                CurrentSubEtapa = await _dataService.GetSubEtapaByIdAsync(IdSubEtapa);
            }
            catch (System.Exception ex)
            {
                ErrorMessage = $"Error cargando datos: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LoadRecursosUtilizadosAsync()
        {
            var items = await _dataService.GetRecursosUtiBySubEtapaIdAsync(IdSubEtapa);
            RecursosUtilizados.Clear();
            foreach (var item in items)
                RecursosUtilizados.Add(item);
        }
        private async Task LoadCategoriasRecAsync()
        {
            var items = await _dataService.GetCategoriasRecAsync();
            DisponibleCategoriasRec.Clear();
            foreach (var item in items)
                DisponibleCategoriasRec.Add(item);
        }

        private async Task LoadRecursosAsync()
        {
            // Pass SelectedCategoriaRec?.Id to filter by category if supported by service
            var items = await _dataService.GetRecursosAsync(SelectedCategoriaRec?.Id);
            DisponibleRecursos.Clear();
            foreach (var item in items)
                DisponibleRecursos.Add(item);
        }

        private async Task LoadUniMedReAsync()
        {
            var items = await _dataService.GetUniMedReAsync();
            DisponibleUniMedRe.Clear();
            foreach (var item in items)
                DisponibleUniMedRe.Add(item);
        }

        [RelayCommand]
        private async Task AddRecursoUtiAsync()
        {
            if (SelectedRecurso == null || SelectedUniMedRe == null || !Cantidad.HasValue || !PrecioUnitario.HasValue || Cantidad <= 0 || PrecioUnitario < 0)
            {
                ErrorMessage = "Por favor, complete todos los campos del recurso.";
                //await Shell.Current.DisplayAlert("Error", "Complete todos los campos.", "OK");
                return;
            }

            IsBusy = true;
            ErrorMessage = string.Empty;
            try
            {
                long id = OfflineFirstDataService.GenerarIdAleatorio(); // Generar ID aleatorio

                var newRecursoUti = new RecursoUti
                {
                    Id = id, //Metodo de ultimo recurso ya que en supabase el id siempre es 0 IMPORTANTE: mejorarlo cuando tenga tiempo
                    IdSubEtapa = IdSubEtapa,
                    IdRecurso = SelectedRecurso.Id,
                    IdUniMedRe = SelectedUniMedRe.Id,
                    CantidadRecursosUti = Cantidad,
                    PrecioUniRecursosUti = PrecioUnitario,
                    TotalRecursosUti = TotalCalculado,
                    CreatedAt = System.DateTime.UtcNow // Service should handle this ideally
                };

                var registro = new RegistroRecursoUti
                {
                    Id = 0,
                    IdSubEtapa = IdSubEtapa,
                    IdRecurso = SelectedRecurso.Id,
                    IdUniMedida = SelectedUniMedRe.Id,
                    CantidadRecursosUti = Cantidad,
                    PrecioUniRecursosUti = PrecioUnitario,
                    FechaRecursoUti = FechaRecursoUti,
                };

                var savedItem = await _dataService.SaveRecursoUtiAsync(newRecursoUti);
                await _dataService.SaveRegistroRecursoUtiAsync(registro);

                // To show details like name, re-fetch or update the savedItem with navigation properties
                savedItem.Recurso = SelectedRecurso;
                savedItem.UniMedRe = SelectedUniMedRe;

                RecursosUtilizados.Add(savedItem);
                await UpdateParentSubEtapaTotalAsync();

                // Clear form
                SelectedRecurso = null;
                SelectedUniMedRe = null;
                Cantidad = null;
                PrecioUnitario = null;
                TotalCalculado = null;
                FechaRecursoUti = System.DateTime.Today;
                OnPropertyChanged(nameof(SelectedRecurso)); // Notify picker to reset
                OnPropertyChanged(nameof(SelectedUniMedRe));
            }
            catch (System.Exception ex)
            {
                ErrorMessage = $"Error guardando recurso: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task DeleteRecursoUtiAsync(RecursoUti recursoUti)
        {
            if (recursoUti == null) return;

            IsBusy = true;
            ErrorMessage = string.Empty;
            try
            {
                await _dataService.DeleteRecursoUtiAsync(recursoUti.Id);
                RecursosUtilizados.Remove(recursoUti);
                await UpdateParentSubEtapaTotalAsync();
            }
            catch (System.Exception ex)
            {
                ErrorMessage = $"Error eliminando recurso: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task UpdateParentSubEtapaTotalAsync()
        {
            if (IdSubEtapa == 0) return;

            try
            {
                // Obtener todos los recursos para la subetapa actual
                var recursosDeLaSubEtapa = await _dataService.GetRecursosUtiBySubEtapaIdAsync(IdSubEtapa);
                decimal nuevoTotalSubEtapa = recursosDeLaSubEtapa.Sum(r => r.TotalRecursosUti ?? 0M);

                // Obtener la subetapa padre
                var parentSubEtapaToUpdate = await _dataService.GetSubEtapaByIdAsync(IdSubEtapa);

                if (parentSubEtapaToUpdate != null)
                {
                    parentSubEtapaToUpdate.TotalSubEstapa = nuevoTotalSubEtapa;

                    // Recalcular PrecioUniSubEtapa si CantidadSubEtapa tiene valor
                    if (parentSubEtapaToUpdate.CantidadSubEtapa.HasValue && parentSubEtapaToUpdate.CantidadSubEtapa.Value > 0)
                    {
                        parentSubEtapaToUpdate.PrecioUniSubEtapa = nuevoTotalSubEtapa / parentSubEtapaToUpdate.CantidadSubEtapa.Value;
                    }
                    else
                    {
                        parentSubEtapaToUpdate.PrecioUniSubEtapa = null; // o 0
                    }

                    await _dataService.SaveSubEtapa(parentSubEtapaToUpdate); // Guardar la subetapa con los valores actualizados

                    // Actualizar la instancia local si es la misma
                    if (CurrentSubEtapa != null && CurrentSubEtapa.Id == parentSubEtapaToUpdate.Id)
                    {
                        CurrentSubEtapa.TotalSubEstapa = parentSubEtapaToUpdate.TotalSubEstapa;
                        CurrentSubEtapa.PrecioUniSubEtapa = parentSubEtapaToUpdate.PrecioUniSubEtapa;
                        OnPropertyChanged(nameof(CurrentSubEtapa)); // Notificar cambio para la UI
                    }
                    //Debug.WriteLine($"[RegistrarRecursoUtiVM] SubEtapa padre ID {parentSubEtapaToUpdate.Id} actualizada: Total={parentSubEtapaToUpdate.TotalSubEstapa}, PrecioUni={parentSubEtapaToUpdate.PrecioUniSubEtapa}");
                }
            }
            catch (System.Exception ex)
            {
                ErrorMessage = $"Error actualizando total y precio de la subetapa: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"[RegistrarRecursoUtiVM] Error actualizando subetapa padre: {ex.Message}");
            }
        }
    }
}