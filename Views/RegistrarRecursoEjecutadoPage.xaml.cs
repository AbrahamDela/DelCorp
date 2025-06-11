using DelCorp.ViewModels;

namespace DelCorp.Views;

public partial class RegistrarRecursoEjecutadoPage : ContentPage
{
    public RegistrarRecursoEjecutadoPage(RegistrarRecursoEjecutadoViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
