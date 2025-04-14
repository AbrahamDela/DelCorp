using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DelCorp.Models;
using DelCorp.Services;

namespace DelCorp.ViewModels;

public partial class ProjectDetailViewModel : ObservableObject
{
    private readonly IDataService _dataService;

    [ObservableProperty]
    private Project _project;

    public ProjectDetailViewModel(IDataService dataService)
    {
        _dataService = dataService;
    }

    [RelayCommand]
    private async Task NavigateBack()
    {
        await Shell.Current.GoToAsync("..");
    }

    // Método para abrir ubicación en mapa si está disponible
    [RelayCommand]
    private async Task OpenLocation()
    {
        if (!string.IsNullOrWhiteSpace(Project.LatitudProyecto) &&
            !string.IsNullOrWhiteSpace(Project.LongitudProyecto))
        {
            try
            {
                var location = new Location(
                    double.Parse(Project.LatitudProyecto),
                    double.Parse(Project.LongitudProyecto)
                );

                await Launcher.OpenAsync(new Uri(
                    $"https://www.google.com/maps/search/?api=1&query={location.Latitude},{location.Longitude}"
                ));
            }
            catch
            {
                await Shell.Current.DisplayAlert(
                    "Error",
                    "No se puede abrir la ubicación",
                    "OK"
                );
            }
        }
    }
}
