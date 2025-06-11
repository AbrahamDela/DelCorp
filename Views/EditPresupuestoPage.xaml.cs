using DelCorp.ViewModels;

namespace DelCorp.Views;

public partial class EditPresupuestoPage : ContentPage
{
    public EditPresupuestoPage(EditPresupuestoViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
