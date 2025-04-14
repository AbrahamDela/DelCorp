using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DelCorp.Services;
using System.Text.RegularExpressions;

namespace DelCorp.ViewModels
{
    public partial class RegisterViewModel : ObservableObject
    {
        private readonly IAuthService _authService;
        private readonly IConnectivity _connectivity;

        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private string _email = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private string _confirmPassword = string.Empty;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        public RegisterViewModel(IAuthService authService, IConnectivity connectivity)
        {
            _authService = authService;
            _connectivity = connectivity;
        }

        [RelayCommand]
        private async Task Register()
        {
            // Validaciones
            if (!ValidateInputs())
                return;

            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var result = await _authService.RegisterAsync(Email, Password, Name);

                if (result)
                {
                    // Navegar a la página principal o mostrar mensaje de éxito
                    await Shell.Current.DisplayAlert(
                        "Registro Exitoso",
                        "Tu cuenta ha sido creada correctamente",
                        "Iniciar Sesión"
                    );
                    await Shell.Current.GoToAsync("//login");
                }
                else
                {
                    ErrorMessage = "No se pudo crear la cuenta. Intenta nuevamente.";
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
        private async Task NavigateToLogin()
        {
            await Shell.Current.GoToAsync("//login");
        }

        private bool ValidateInputs()
        {
            // Validar nombre
            if (string.IsNullOrWhiteSpace(Name))
            {
                ErrorMessage = "El nombre es obligatorio";
                return false;
            }

            // Validar email
            if (string.IsNullOrWhiteSpace(Email))
            {
                ErrorMessage = "El correo electrónico es obligatorio";
                return false;
            }

            if (!IsValidEmail(Email))
            {
                ErrorMessage = "Correo electrónico inválido";
                return false;
            }

            // Validar contraseña
            if (string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "La contraseña es obligatoria";
                return false;
            }

            if (Password.Length < 8)
            {
                ErrorMessage = "La contraseña debe tener al menos 8 caracteres";
                return false;
            }

            // Validar confirmación de contraseña
            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Las contraseñas no coinciden";
                return false;
            }

            // Verificar conectividad
            if (_connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                ErrorMessage = "No hay conexión a internet";
                return false;
            }

            return true;
        }

        private bool IsValidEmail(string email)
        {
            // Expresión regular para validar email
            string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
        }
    }
}
