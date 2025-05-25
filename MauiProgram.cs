using CommunityToolkit.Maui;
using DelCorp.Models;
using DelCorp.Services;
using DelCorp.ViewModels;
using DelCorp.Views;
using Microsoft.Extensions.Logging;
using System.IO;

namespace DelCorp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Configurar ruta de base de datos local
            string dbPath = Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "delcorp.db3");

            // Supabase
            var url = AppConfig.SUPABASE_URL;
            var key = AppConfig.SUPABASE_KEY;
            builder.Services.AddSingleton(provider => new Supabase.Client(url, key));

            // Servicios de MAUI
            builder.Services.AddSingleton<IConnectivity>(Connectivity.Current);

            // Base de datos local
            builder.Services.AddSingleton<LocalDatabaseService>();
            builder.Services.AddSingleton<IAuthService, SupabaseAuthService>();

            // ViewModels
            builder.Services.AddSingleton<ProjectViewModel>();
            builder.Services.AddTransient<AddProjectViewModel>();
            builder.Services.AddTransient<ProjectDetailViewModel>();
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<RegisterViewModel>();
            builder.Services.AddTransient<UserProfileViewModel>();
            builder.Services.AddTransient<ManageBudgetViewModel>();
            builder.Services.AddTransient<PresupuestoViewModel>();
            builder.Services.AddTransient<RegistrarPresupuestoViewModel>();
            builder.Services.AddTransient<RegistrarEtapaViewModel>();
            builder.Services.AddTransient<RegistrarSubEtapaViewModel>();

            // Views
            builder.Services.AddSingleton<ProjectPage>();
            builder.Services.AddTransient<AddProjectPage>();
            builder.Services.AddTransient<ProjectDetailPage>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<RegisterPage>();
            builder.Services.AddTransient<UserProfilePage>();
            builder.Services.AddTransient<ManageBudget>();
            builder.Services.AddTransient<PresupuestoPage>();
            builder.Services.AddTransient<RegistrarPresupuestoPage>();
            builder.Services.AddTransient<RegistrarEtapaPage>();
            builder.Services.AddTransient<RegistrarSubEtapaPage>();

            // Servicios
            builder.Services.AddSingleton<IDataService, OfflineFirstDataService>();
            builder.Services.AddSingleton<SupabaseAuthService>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}