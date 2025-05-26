using DelCorp.ViewModels;

namespace DelCorp.Views;

public partial class RegistrarRecursoUtiPage : ContentPage
{
	public RegistrarRecursoUtiPage(RegistrarRecursoUtiViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}