using DelCorp.Views;
using Supabase.Gotrue;
using Client = Supabase.Client;
using Microsoft.Maui;

namespace DelCorp.Services;

public class SupabaseAuthService : IAuthService
{
    private const string UserIdKey = "LocalUserId";
    private readonly Client _supabaseClient;
    private readonly IConnectivity _connectivity;

    public SupabaseAuthService(Client supabaseClient, IConnectivity connectivity)
    {
        _supabaseClient = supabaseClient;
        _connectivity = connectivity;
        LoadLocalUserId(); // Intentar cargar el ID al inicializar el servicio
    }

    private void SaveLocalUserId(string? userId)
    {
        if (userId != null)
        {
            Preferences.Set(UserIdKey, userId);
        }
        else
        {
            Preferences.Remove(UserIdKey);
        }
    }

    private void LoadLocalUserId()
    {
        var localId = Preferences.Get(UserIdKey, (string?)null);
        if (!string.IsNullOrEmpty(localId) && _supabaseClient.Auth.CurrentUser == null)
        {
            System.Diagnostics.Debug.WriteLine($"ID de usuario local cargado: {localId}");
        }
    }

    public async Task<bool> LoginAsync(string email, string password)
    {
        if (_connectivity.NetworkAccess != NetworkAccess.Internet)
        {
            throw new Exception("No hay conexión a internet");
        }

        try
        {
            var response = await _supabaseClient.Auth.SignIn(email, password);
            if (response?.User?.Id != null)
            {
                SaveLocalUserId(response.User.Id);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Login Error: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> RegisterAsync(string email, string password, string name)
    {
        if (_connectivity.NetworkAccess != NetworkAccess.Internet)
        {
            throw new Exception("No hay conexión a internet");
        }

        try
        {
            var options = new SignUpOptions
            {
                Data = new Dictionary<string, object>
                {
                    { "name", name }
                }
            };

            var response = await _supabaseClient.Auth.SignUp(email, password, options);
            if (response?.User?.Id != null)
            {
                SaveLocalUserId(response.User.Id);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Register Error: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> SendPasswordResetAsync(string email)
    {
        if (_connectivity.NetworkAccess != NetworkAccess.Internet)
        {
            throw new Exception("No hay conexión a internet");
        }

        try
        {
            await _supabaseClient.Auth.ResetPasswordForEmail(email);
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Password Reset Error: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> LogoutAsync()
    {
        try
        {
            await _supabaseClient.Auth.SignOut();
            SaveLocalUserId(null); // Limpiar el ID local al cerrar sesión
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Logout Error: {ex.Message}");
            return false;
        }
    }

    public Task<bool> IsAuthenticatedAsync()
    {
        return Task.FromResult(_supabaseClient.Auth.CurrentUser != null || !string.IsNullOrEmpty(Preferences.Get(UserIdKey, (string?)null)));
    }

    public Task<string?> GetCurrentUserEmail()
    {
        return Task.FromResult(_supabaseClient.Auth.CurrentUser?.Email);
    }

    public async Task<UserProfile> GetCurrentUserProfileAsync()
    {
        var user = _supabaseClient.Auth.CurrentUser;
        string? localId = Preferences.Get(UserIdKey, (string?)null);

        if (user == null && string.IsNullOrEmpty(localId))
            throw new Exception("No hay usuario autenticado local o remotamente");

        string userIdToFetch = user?.Id ?? localId!; // Usar el ID del usuario actual si existe, sino el ID local

        try
        {
            var response = await _supabaseClient
                .From<UserProfile>()
                .Where(x => x.Id == userIdToFetch)
                .Single();

            return response ?? new UserProfile
            {
                Id = userIdToFetch,
                Email = user?.Email ?? "email_desconocido", // Puedes manejar esto mejor si es necesario
                Name = user?.Email ?? "nombre_desconocido",
                CreatedAt = user?.CreatedAt ?? DateTime.UtcNow // O un valor por defecto apropiado
            };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al obtener el perfil: {ex.Message}");
            return new UserProfile
            {
                Id = userIdToFetch,
                Email = user?.Email ?? "email_desconocido",
                Name = user?.Email ?? "nombre_desconocido",
                CreatedAt = user?.CreatedAt ?? DateTime.UtcNow
            };
        }
    }

    // Método para verificar si existe un ID de usuario local
    public Task<bool> CheckAuthenticationAsync()
    {
        return Task.FromResult(!string.IsNullOrEmpty(Preferences.Get(UserIdKey, (string?)null)));
    }
}