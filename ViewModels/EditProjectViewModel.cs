using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DelCorp.Models;
using DelCorp.Services;

namespace DelCorp.ViewModels
{
    [QueryProperty(nameof(ProjectId), "id")]
    public partial class EditProjectViewModel : ObservableObject
    {
        private readonly IDataService _dataService;

        [ObservableProperty]
        private Project _projectToEdit;

        [ObservableProperty]
        private int _projectId;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private Dictionary<string, string> _validationErrors = new();

        public EditProjectViewModel(IDataService dataService)
        {
            _dataService = dataService;
        }

        async partial void OnProjectIdChanged(int value)
        {
            if (value > 0)
            {
                await LoadProject(value);
            }
        }

        private async Task LoadProject(int projectId)
        {
            IsLoading = true;
            ProjectToEdit = await _dataService.GetProject(projectId);
            IsLoading = false;
        }

        private bool ValidateProject()
        {
            ValidationErrors.Clear();
            if (string.IsNullOrWhiteSpace(ProjectToEdit.NombreProyecto))
                ValidationErrors[nameof(ProjectToEdit.NombreProyecto)] = "El nombre del proyecto es requerido";

            return ValidationErrors.Count == 0;
        }

        [RelayCommand]
        private async Task UpdateProject()
        {
            if (!ValidateProject())
            {
                var errorMessage = string.Join("\n", ValidationErrors.Values);
                await Shell.Current.DisplayAlert("Errores de Validación", errorMessage, "OK");
                return;
            }

            IsLoading = true;
            try
            {
                bool result = await _dataService.SaveProject(ProjectToEdit);

                if (result)
                {
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "No se pudo actualizar el proyecto", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Ocurrió un error: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
