using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DelCorp.Services;

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
        // Validar campos
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Por favor, ingrese correo y contraseña";
            IsForgotPasswordVisible = false;
            return;
        }

        // Verificar conexión a internet
        if (_connectivity.NetworkAccess != NetworkAccess.Internet)
        {
            ErrorMessage = "No hay conexión a internet";
            IsForgotPasswordVisible = false;
            return;
        }

        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            IsForgotPasswordVisible = false;

            // Intentar iniciar sesión
            var result = await _authService.LoginAsync(Email, Password);

            if (result)
            {
                // Navegar a la página principal
                Application.Current.MainPage = new AppShell();
            }
            else
            {
                HandleLoginError();
            }
        }
        catch (Exception ex)
        {
            HandleLoginError(ex);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void HandleLoginError(Exception ex = null)
    {
        // Diagnostico detallado del error
        if (ex != null)
        {
            // Imprime todos los detalles del error
            System.Diagnostics.Debug.WriteLine($"Login Error - Type: {ex.GetType().Name}");
            System.Diagnostics.Debug.WriteLine($"Error Message: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Full Exception: {ex}");

            // Si es una excepción de HttpRequestException, intenta obtener más detalles
            if (ex is HttpRequestException httpEx)
            {
                System.Diagnostics.Debug.WriteLine($"HTTP Error Details: {httpEx.StatusCode}");
            }
        }

        // Mensajes de error más detallados
        string errorMessage = "Error al iniciar sesión";

        try
        {
            // Intentar parsear el mensaje de error como JSON
            var errorDetails = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(ex.Message);

            if (errorDetails != null)
            {
                // Revisar si hay claves específicas en el error
                if (errorDetails.TryGetValue("msg", out var msg))
                {
                    errorMessage = msg.ToString();
                }
                else if (errorDetails.TryGetValue("message", out var message))
                {
                    errorMessage = message.ToString();
                }
            }
        }
        catch
        {
            // Si la deserialización falla, usar el mensaje original
            if (ex != null)
            {
                // Revisar diferentes escenarios de error
                if (ex.Message.Contains("Invalid login credentials"))
                {
                    errorMessage = "Correo o contraseña incorrectos";
                }
                else if (ex.Message.Contains("400"))
                {
                    errorMessage = "Solicitud inválida. Verifique sus credenciales.";
                }
                else if (ex.Message.Contains("401"))
                {
                    errorMessage = "No autorizado. Credenciales incorrectas.";
                }
                else if (ex.Message.Contains("network"))
                {
                    errorMessage = "Error de conexión. Verifique su red.";
                }
            }
        }

        ErrorMessage = errorMessage;
        IsForgotPasswordVisible = true;
    }

    [RelayCommand]
    private async Task ForgotPassword()
    {
        if (string.IsNullOrWhiteSpace(Email))
        {
            ErrorMessage = "Por favor, ingrese su correo electrónico";
            return;
        }

        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var result = await _authService.SendPasswordResetAsync(Email);

            if (result)
            {
                await Shell.Current.DisplayAlert(
                    "Recuperar Contraseña",
                    "Se ha enviado un enlace de recuperación a su correo",
                    "OK"
                );
                IsForgotPasswordVisible = false;
            }
            else
            {
                ErrorMessage = "No se pudo enviar el correo de recuperación";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task NavigateToRegister()
    {
        await Shell.Current.GoToAsync("//register");
    }
}