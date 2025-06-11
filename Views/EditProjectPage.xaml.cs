using DelCorp.ViewModels;

namespace DelCorp.Views;

public partial class EditProjectPage : ContentPage
{
    public EditProjectPage(EditProjectViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
