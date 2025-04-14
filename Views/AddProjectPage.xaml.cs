using DelCorp.ViewModels;

namespace DelCorp.Views;

public partial class AddProjectPage : ContentPage
{
	public AddProjectPage(AddProjectViewModel addProjectViewModel)
	{
		InitializeComponent();
		BindingContext = addProjectViewModel;
	}
}