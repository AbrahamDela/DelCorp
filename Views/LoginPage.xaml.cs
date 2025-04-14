using DelCorp.ViewModels;

namespace DelCorp.Views;

public partial class LoginPage : ContentPage
{
	public LoginPage(LoginViewModel loginViewModel)
	{
        InitializeComponent();
        BindingContext = loginViewModel;
    }
}