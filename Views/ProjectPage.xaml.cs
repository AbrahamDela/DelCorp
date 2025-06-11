using DelCorp.ViewModels;

namespace DelCorp.Views;

public partial class ProjectPage : ContentPage
{
    private readonly ProjectViewModel _viewModel;

    public ProjectPage(ProjectViewModel projectViewModel)
    {
        InitializeComponent();
        _viewModel = projectViewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_viewModel != null && _viewModel.RefreshProjectsCommand.CanExecute(null))
        {
            await _viewModel.RefreshProjectsCommand.ExecuteAsync(null);
        }
    }
}