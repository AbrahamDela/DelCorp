using DelCorp.ViewModels;

namespace DelCorp.Views;

public partial class RegistrarPresupuestoPage : ContentPage
{
	public RegistrarPresupuestoPage(RegistrarPresupuestoViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}