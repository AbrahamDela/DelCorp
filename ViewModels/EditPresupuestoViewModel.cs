using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DelCorp.Models;
using DelCorp.Services;

namespace DelCorp.ViewModels
{
    [QueryProperty(nameof(PresupuestoId), "id")]
    public partial class EditPresupuestoViewModel : ObservableObject
    {
        private readonly IDataService _dataService;

        [ObservableProperty]
        private Presupuesto _presupuestoToEdit;

        [ObservableProperty]
        private long _presupuestoId;

        [ObservableProperty]
        private bool _isLoading;

        public EditPresupuestoViewModel(IDataService dataService)
        {
            _dataService = dataService;
        }

        async partial void OnPresupuestoIdChanged(long value)
        {
            if (value > 0)
            {
                IsLoading = true;
                PresupuestoToEdit = await _dataService.GetPresupuestoByIdAsync(value);
                if (PresupuestoToEdit != null)
                {
                    PresupuestoToEdit.MontoEjePresupuesto = await _dataService.GetTotalEjecutadoForPresupuestoAsync(PresupuestoToEdit.Id);
                }
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task UpdatePresupuesto()
        {
            if (string.IsNullOrWhiteSpace(PresupuestoToEdit.NombrePresupuesto))
            {
                await Shell.Current.DisplayAlert("Error", "El nombre del presupuesto es requerido.", "OK");
                return;
            }

            IsLoading = true;
            try
            {
                var result = await _dataService.SavePresupuesto(PresupuestoToEdit);
                if (result != null)
                {
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "No se pudo actualizar el presupuesto.", "OK");
                }
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
