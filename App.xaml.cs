using DelCorp.Services;
using DelCorp.Views;

namespace DelCorp
{
    public partial class App : Application
    {
        public App(LoginPage loginPage, IAuthService authService)
        {
            InitializeComponent();

            if (authService.CheckAuthenticationAsync().Result)
            {
                MainPage = new AppShell();
            }
            else
            {
                MainPage = loginPage;
            }
        }
    }
}
