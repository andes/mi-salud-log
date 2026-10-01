using Microsoft.AspNetCore.Identity;

namespace SaludPortal.Admin.Services;

public class UsuariosService
{
    private readonly UserManager<IdentityUser> _userManager;

    public UsuariosService(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    public Task<List<IdentityUser>> GetAllAsync()
        => Task.FromResult(_userManager.Users.OrderBy(u => u.Email).ToList());

    public Task<IdentityResult> CreateAsync(string email, string password)
    {
        var user = new IdentityUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };
        return _userManager.CreateAsync(user, password);
    }

    public async Task<IdentityResult> UpdateEmailAsync(string userId, string newEmail)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return IdentityResult.Failed(new IdentityError { Description = "Usuario no encontrado." });

        var setEmailResult = await _userManager.SetEmailAsync(user, newEmail);
        if (!setEmailResult.Succeeded) return setEmailResult;

        return await _userManager.SetUserNameAsync(user, newEmail);
    }

    public async Task<IdentityResult> ResetPasswordAsync(string userId, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return IdentityResult.Failed(new IdentityError { Description = "Usuario no encontrado." });

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        return await _userManager.ResetPasswordAsync(user, token, newPassword);
    }

    public async Task<IdentityResult> DeleteAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return IdentityResult.Failed(new IdentityError { Description = "Usuario no encontrado." });

        return await _userManager.DeleteAsync(user);
    }
}
