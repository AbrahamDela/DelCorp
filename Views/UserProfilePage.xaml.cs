using DelCorp.ViewModels;

namespace DelCorp.Views;

public partial class UserProfilePage : ContentPage
{
	public UserProfilePage(UserProfileViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Cargar perfil de usuario cuando la página aparece
        if (BindingContext is UserProfileViewModel viewModel)
        {
            await viewModel.LoadUserProfileCommand.ExecuteAsync(null);
        }
    }
}