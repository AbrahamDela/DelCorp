using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DelCorp.Models;
using DelCorp.Services;
using System.ComponentModel.DataAnnotations;

namespace DelCorp.ViewModels
{
    public partial class AddProjectViewModel : ObservableObject
    {
        private readonly IDataService _dataService;

        [ObservableProperty]
        private Project _newProject;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private Dictionary<string, string> _validationErrors = new();

        public DateTime MinStartDate { get; } = DateTime.Now;
        public DateTime MinEndDate => NewProject?.FechaInicioProyecto ?? DateTime.Now;

        public AddProjectViewModel(IDataService dataService)
        {
            _dataService = dataService;
            NewProject = new Project
            {
                Id = GenerateLocalProjectId(),
                CreatedAt = DateTime.Now
            };
        }

        private int GenerateLocalProjectId()
        {
            return (int)(DateTime.UtcNow.Ticks & 0x7FFFFFFF);
        }

        private bool ValidateProject()
        {
            // Limpiar errores previos
            ValidationErrors.Clear();

            // Validaciones
            if (string.IsNullOrWhiteSpace(NewProject.NombreProyecto))
                ValidationErrors[nameof(NewProject.NombreProyecto)] = "El nombre del proyecto es requerido";

            return ValidationErrors.Count == 0;
        }

        [RelayCommand]
        private async Task SaveProject()
        {
            // Validar antes de guardar
            if (!ValidateProject())
            {
                // Mostrar errores de validación
                var errorMessage = string.Join("\n", ValidationErrors.Values);
                await Shell.Current.DisplayAlert("Errores de Validación", errorMessage, "OK");
                return;
            }

            // Iniciar estado de carga
            IsLoading = true;

            try
            {
                // Intentar guardar el proyecto
                bool result = await _dataService.SaveProject(NewProject);

                if (result)
                {
                    // Navegar hacia atrás o limpiar el formulario
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "No se pudo guardar el proyecto", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Ocurrió un error: {ex.Message}", "OK");
            }
            finally
            {
                // Finalizar estado de carga
                IsLoading = false;
            }
        }
    }
}
