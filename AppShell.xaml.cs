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
            RegisterForRoute<EditProjectPage>();
            RegisterForRoute<RegistrarPresupuestoPage>();
            RegisterForRoute<EditPresupuestoPage>();
            RegisterForRoute<RegistrarEtapaPage>();
            RegisterForRoute<RegistrarSubEtapaPage>();
            RegisterForRoute<RegistrarRecursoUtiPage>();
            RegisterForRoute<RegistrarAvancePage>();
        }

        protected void RegisterForRoute<T>()
        {
            Routing.RegisterRoute(typeof(T).Name, typeof(T));
        }
    }
}
