using DelCorp.ViewModels;

namespace DelCorp.Views;

public partial class RegistrarAvancePage : ContentPage
{
    public RegistrarAvancePage(RegistrarAvanceViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
