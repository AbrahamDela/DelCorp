using DelCorp.ViewModels;

namespace DelCorp.Views;

public partial class RegistrarEtapaPage : ContentPage
{
    public RegistrarEtapaPage(RegistrarEtapaViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
