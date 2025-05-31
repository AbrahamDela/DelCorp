using DelCorp.ViewModels;

namespace DelCorp.Views;

public partial class RegistrarSubEtapaPage : ContentPage
{
	public RegistrarSubEtapaPage(RegistrarSubEtapaViewModel registrarSubEtapaViewModel)
	{
		InitializeComponent();
        BindingContext = registrarSubEtapaViewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is RegistrarSubEtapaViewModel viewModel)
        {
            // Llama al método que carga/refresca las subetapas.
            // Asumiendo que ApplyQueryAttributes ya contiene la lógica de carga inicial
            // o tienes un método dedicado para recargar.
            // Si IdEtapa ya está seteado, puedes llamar directamente a CargarSubEtapasAsync.
            if (viewModel.IdEtapa != 0) // Asegúrate que el IdEtapa necesario esté disponible
            {
                await viewModel.CargarSubEtapasAsync();
            }
            // Si necesitas que ApplyQueryAttributes se ejecute de nuevo para recargar,
            // podrías necesitar una lógica más específica o un método de refresco dedicado
            // en el ViewModel que ApplyQueryAttributes pueda llamar o que OnAppearing llame.
        }
    }
}