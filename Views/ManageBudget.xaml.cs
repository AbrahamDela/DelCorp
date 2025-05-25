using DelCorp.ViewModels;

namespace DelCorp.Views;

public partial class ManageBudget : ContentPage
{
	public ManageBudget(ManageBudgetViewModel manageBudgetViewModel)
	{
		InitializeComponent();
		BindingContext = manageBudgetViewModel;
	}
}