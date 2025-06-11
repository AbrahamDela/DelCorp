using DelCorp.ViewModels;

namespace DelCorp.Views;

public partial class RegistroRecursosPage : ContentPage
{
    public RegistroRecursosPage(RegistroRecursosViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
