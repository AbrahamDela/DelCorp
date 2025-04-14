using DelCorp.ViewModels;

namespace DelCorp.Views;

public partial class ProjectPage : ContentPage
{
	public ProjectPage(ProjectViewModel projectViewModel)
	{
		InitializeComponent();
        BindingContext = projectViewModel;
    }
}