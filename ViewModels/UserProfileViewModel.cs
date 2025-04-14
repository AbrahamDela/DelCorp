using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DelCorp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DelCorp.ViewModels;

public partial class UserProfileViewModel : ObservableObject
{
    private readonly IAuthService _authService;
    private readonly IConnectivity _connectivity;

    [ObservableProperty]
    private UserProfile _userProfile;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage;

    public UserProfileViewModel(IAuthService authService, IConnectivity connectivity)
    {
        _authService = authService;
        _connectivity = connectivity;
    }

    [RelayCommand]
    private async Task LoadUserProfile()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            if (_connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                ErrorMessage = "No hay conexión a internet";
                return;
            }

            UserProfile = await _authService.GetCurrentUserProfileAsync();
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
    private async Task Logout()
    {
        try
        {
            IsLoading = true;

            // Cerrar sesión
            await _authService.LogoutAsync();

            // Navegar a la página de login
            await Shell.Current.GoToAsync("//login");
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
    private async Task EditProfile()
    {
        // Navegar a la página de edición de perfil
        await Shell.Current.GoToAsync("profile/edit");
    }

    [RelayCommand]
    private async Task ChangePassword()
    {
        // Navegar a la página de cambio de contraseña
        await Shell.Current.GoToAsync("profile/change-password");
    }
}
