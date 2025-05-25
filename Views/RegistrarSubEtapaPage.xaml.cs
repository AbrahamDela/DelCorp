using DelCorp.ViewModels;

namespace DelCorp.Views;

public partial class RegistrarSubEtapaPage : ContentPage
{
	public RegistrarSubEtapaPage(RegistrarSubEtapaViewModel registrarSubEtapaViewModel)
	{
		InitializeComponent();
        BindingContext = registrarSubEtapaViewModel;
    }
}