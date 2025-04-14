using DelCorp.Views;

namespace DelCorp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            RegisterForRoute<AddProjectPage>();
        }

        protected void RegisterForRoute<T>()
        {
            Routing.RegisterRoute(typeof(T).Name, typeof(T));
        }
    }
}
