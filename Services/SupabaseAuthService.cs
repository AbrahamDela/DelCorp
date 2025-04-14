using DelCorp.Views;
using Supabase.Gotrue;
using Client = Supabase.Client;


namespace DelCorp.Services;

public class SupabaseAuthService : IAuthService
{
    private readonly Client _supabaseClient;
    private readonly IConnectivity _connectivity;

    public SupabaseAuthService(Client supabaseClient, IConnectivity connectivity)
    {
        _supabaseClient = supabaseClient;
        _connectivity = connectivity;
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
            return response != null;
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
            return response != null;
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
        return Task.FromResult(_supabaseClient.Auth.CurrentUser != null);
    }

    public Task<string?> GetCurrentUserEmail()
    {
        return Task.FromResult(_supabaseClient.Auth.CurrentUser?.Email);
    }

    public async Task<UserProfile> GetCurrentUserProfileAsync()
    {
        var user = _supabaseClient.Auth.CurrentUser;

        if (user == null)
            throw new Exception("No hay usuario autenticado");

        try
        {
            // Usar From<UserProfile>() para consultar la tabla "profiles"
            var response = await _supabaseClient
                .From<UserProfile>()
                .Where(x => x.Id == user.Id)
                .Single();

            // Si no se encuentra perfil, crear uno básico
            return response ?? new UserProfile
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Email, // Usar email como nombre por defecto
                CreatedAt = user.CreatedAt
            };
        }
        catch (Exception ex)
        {
            // Manejo de error si no existe perfil
            return new UserProfile
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Email,
                CreatedAt = user.CreatedAt
            };
        }
    }

    // Método para verificar si el usuario está autenticado al iniciar la aplicación
    public async Task<bool> CheckAuthenticationAsync()
    {
        try
        {
            // Verificar si hay un usuario actual
            if (_supabaseClient.Auth.CurrentUser != null)
            {
                return true;
            }

            // Intentar restaurar la sesión si existe
            var session = await _supabaseClient.Auth.RetrieveSessionAsync();
            return session != null;
        }
        catch
        {
            return false;
        }
    }
}
