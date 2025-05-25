using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DelCorp.Services;
using DelCorp.Models;
using System.Diagnostics;

namespace DelCorp.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthService _authService;
    private readonly IConnectivity _connectivity;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private bool _isLoginEnabled = true;

    [ObservableProperty]
    private bool _isForgotPasswordVisible;

    partial void OnErrorMessageChanged(string value)
    {
        HasError = !string.IsNullOrWhiteSpace(value);
    }

    partial void OnIsLoadingChanged(bool value)
    {
        IsLoginEnabled = !value;
    }

    public LoginViewModel(IAuthService authService, IConnectivity connectivity)
    {
        _authService = authService;
        _connectivity = connectivity;
    }

    [RelayCommand]
    private async Task Login()
    {
        // Validaciones iniciales
        if (string.IsNullOrWhiteSpace(Email))
        {
            ErrorMessage = "Por favor, ingrese su correo electrónico";
            return;
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Por favor, ingrese su contraseña";
            return;
        }

        try
        {
            // Restablecer estado de error
            ErrorMessage = string.Empty;
            IsLoading = true;

            // Verificar conectividad
            bool hasInternet = _connectivity.NetworkAccess == NetworkAccess.Internet;

            // Intentar login
            var loginResult = await PerformLogin(hasInternet);

            if (loginResult)
            {
                // Navegación a página principal
                await NavigateToMainPage();
            }
        }
        catch (Exception ex)
        {
            // Diagnóstico detallado de errores
            HandleLoginError(ex);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task<bool> PerformLogin(bool hasInternet)
    {
        try
        {

            if (!hasInternet)
            {
                ErrorMessage = "Se requiere conexión a internet para iniciar sesión";
                return false;
            }

            // Intentar login
            var result = await _authService.LoginAsync(Email, Password);

            Debug.WriteLine($"Login Result: {result}");

            return result;
        }
        catch (Exception ex)
        {
            // Loguear error detallado
            Debug.WriteLine($"Login Error: {ex.Message}");
            Debug.WriteLine($"Full Exception: {ex}");
            throw;
        }
    }

    private async Task NavigateToMainPage()
    {
        try
        {
            // Navegación segura
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Application.Current.MainPage = new AppShell();
            });
        }
        catch (Exception ex)
        {
            ErrorMessage = "Error al navegar a la página principal";
        }
    }

    private void HandleLoginError(Exception ex)
    {
        // Diagnóstico detallado de errores
        Debug.WriteLine($"Login Error Type: {ex.GetType().Name}");
        Debug.WriteLine($"Error Message: {ex.Message}");
        Debug.WriteLine($"Stack Trace: {ex.StackTrace}");

        // Mensajes de error específicos
        string errorMessage = "Error al iniciar sesión";

        if (ex.Message.Contains("Invalid login credentials"))
        {
            errorMessage = "Correo o contraseña incorrectos";
        }
        else if (ex.Message.Contains("Network is unreachable"))
        {
            errorMessage = "No hay conexión a internet";
        }
        else if (ex.Message.Contains("No hay usuario local registrado"))
        {
            errorMessage = "Requiere conexión a internet para el primer inicio de sesión";
        }

        // Mostrar mensaje de error
        ErrorMessage = errorMessage;
    }

    [RelayCommand]
    private async Task OnAppearing()
    {
        try
        {
            // Verificar autenticación al aparecer la página
            bool isAuthenticated = await _authService.CheckAuthenticationAsync();

            Debug.WriteLine($"Authentication Check: {isAuthenticated}");

            if (isAuthenticated)
            {
                await NavigateToMainPage();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Authentication Check Error: {ex.Message}");
        }
    }
}