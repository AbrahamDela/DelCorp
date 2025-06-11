using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DelCorp.Models;
using DelCorp.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace DelCorp.ViewModels
{
    [QueryProperty(nameof(IdSubEtapa), "idSubEtapa")]
    public partial class RegistrarAvanceViewModel : ObservableObject
    {
        private readonly IDataService _dataService;

        [ObservableProperty]
        private long _idSubEtapa;

        [ObservableProperty]
        private ObservableCollection<RecursoUti> _recursosPresupuestados;

        [ObservableProperty]
        private ObservableCollection<RegistroRecursoUti> _recursosYaRegistrados;

        [ObservableProperty]
        private ObservableCollection<Recurso> _recursosDisponibles;

        [ObservableProperty]
        private Recurso _recursoSeleccionado;

        [ObservableProperty]
        private DateTime _fechaDeUso = DateTime.Today;

        [ObservableProperty]
        private decimal? _cantidadUtilizada;

        [ObservableProperty]
        private decimal? _precioUnitario;

        [ObservableProperty]
        private bool _isBusy;

        public RegistrarAvanceViewModel(IDataService dataService)
        {
            _dataService = dataService;
            RecursosPresupuestados = new ObservableCollection<RecursoUti>();
            RecursosYaRegistrados = new ObservableCollection<RegistroRecursoUti>();
            RecursosDisponibles = new ObservableCollection<Recurso>();
        }

        partial void OnIdSubEtapaChanged(long value)
        {
            if (value > 0)
            {
                LoadDataCommand.ExecuteAsync(null);
            }
        }

        [RelayCommand]
        private async Task LoadDataAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                RecursosPresupuestados.Clear();
                var presupuestados = await _dataService.GetRecursosUtiBySubEtapaIdAsync(IdSubEtapa);
                foreach (var r in presupuestados) RecursosPresupuestados.Add(r);

                RecursosYaRegistrados.Clear();
                var registrados = await _dataService.GetRegistrosRecursosUtiBySubEtapaIdAsync(IdSubEtapa);
                foreach (var r in registrados) RecursosYaRegistrados.Add(r);

                RecursosDisponibles.Clear();
                var disponibles = await _dataService.GetRecursosAsync();
                foreach (var r in disponibles) RecursosDisponibles.Add(r);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task GuardarAvanceAsync()
        {
            if (RecursoSeleccionado == null || !CantidadUtilizada.HasValue || !PrecioUnitario.HasValue)
            {
                await Shell.Current.DisplayAlert("Error", "Complete todos los campos.", "OK");
                return;
            }

            var nuevoRegistro = new RegistroRecursoUti
            {
                IdSubEtapa = IdSubEtapa,
                IdRecurso = RecursoSeleccionado.Id,
                IdUniMedida = RecursosPresupuestados.FirstOrDefault(r => r.IdRecurso == RecursoSeleccionado.Id)?.IdUniMedRe,
                FechaRecursoUti = FechaDeUso,
                CantidadRecursosUti = CantidadUtilizada,
                PrecioUniRecursosUti = PrecioUnitario
            };

            var guardado = await _dataService.SaveRegistroRecursoUtiAsync(nuevoRegistro);

            if (guardado != null)
            {
                RecursosYaRegistrados.Add(guardado);
                RecursoSeleccionado = null;
                CantidadUtilizada = null;
                PrecioUnitario = null;
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", "No se pudo guardar el registro.", "OK");
            }
        }
    }
}
