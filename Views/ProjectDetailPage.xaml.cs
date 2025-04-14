namespace DelCorp.Views;

using DelCorp.Models;
using DelCorp.Services;
using DelCorp.ViewModels;
using Microsoft.Maui.Controls;

public partial class ProjectDetailPage : ContentPage
{
    private readonly IDataService _dataService;

    public ProjectDetailPage(ProjectDetailViewModel viewModel, IDataService dataService)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _dataService = dataService;
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        try
        {
            // Obtener el identificador del proyecto directamente de los parámetros de navegación
            var projectId = GetProjectIdFromRoute();

            if (projectId.HasValue)
            {
                await LoadProject(projectId.Value);
            }
            else
            {
                await HandleNavigationError("No se proporcionó un identificador de proyecto válido");
            }
        }
        catch (Exception ex)
        {
            await HandleNavigationError(ex.Message);
        }
    }

    private int? GetProjectIdFromRoute()
    {
        // Método para extraer el ID del proyecto de manera más segura
        if (Shell.Current.CurrentState?.Location?.OriginalString is string route)
        {
            // Ejemplo de ruta: ProjectDetailPage?project=123
            var queryParams = System.Web.HttpUtility.ParseQueryString(
                new Uri(route, UriKind.Relative).Query
            );

            if (int.TryParse(queryParams["project"], out int projectId))
            {
                return projectId;
            }
        }

        return null;
    }

    private async Task LoadProject(int projectId)
    {
        try
        {
            // Cargar proyecto usando tu servicio de datos
            var project = await _dataService.GetProject(projectId);

            if (project != null && BindingContext is ProjectDetailViewModel viewModel)
            {
                viewModel.Project = project;
            }
            else
            {
                await HandleNavigationError("No se encontró el proyecto");
            }
        }
        catch (Exception ex)
        {
            await HandleNavigationError($"Error al cargar el proyecto: {ex.Message}");
        }
    }

    private async Task HandleNavigationError(string message)
    {
        // Mostrar alerta de error
        await Shell.Current.DisplayAlert(
            "Error de Navegación",
            message,
            "OK"
        );

        // Intentar volver atrás
        try
        {
            await Shell.Current.GoToAsync("..");
        }
        catch
        {
            // Si no se puede volver atrás, navegar a la página principal
            await Shell.Current.GoToAsync("//main");
        }
    }
}