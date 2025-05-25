using DelCorp.ViewModels;

namespace DelCorp.Views;

public partial class PresupuestoPage : ContentPage
{
	public PresupuestoPage(PresupuestoViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}