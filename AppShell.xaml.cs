using DelCorp.Views;

namespace DelCorp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            RegisterForRoute<AddProjectPage>();
            RegisterForRoute<ProjectDetailPage>();
            RegisterForRoute<RegistrarPresupuestoPage>();
            RegisterForRoute<RegistrarEtapaPage>();
            RegisterForRoute<RegistrarSubEtapaPage>();
            RegisterForRoute<RegistrarRecursoUtiPage>();
            RegisterForRoute<RegistroRecursosPage>();
        }

        protected void RegisterForRoute<T>()
        {
            Routing.RegisterRoute(typeof(T).Name, typeof(T));
        }
    }
}
