using Microsoft.AspNetCore.Identity;

namespace SaludPortal.Admin.Data;

public static class AdminSeeder
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration config)
    {
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

        var adminUsers = config.GetSection("AdminUsers").Get<List<AdminUserEntry>>();

        if (adminUsers is null || adminUsers.Count == 0)
            return;

        foreach (var entry in adminUsers)
        {
            if (string.IsNullOrWhiteSpace(entry.Email) || string.IsNullOrWhiteSpace(entry.Password))
                continue;

            if (await userManager.FindByEmailAsync(entry.Email) is not null)
                continue;

            var user = new IdentityUser
            {
                UserName = entry.Email,
                Email = entry.Email,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, entry.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"No se pudo crear el usuario admin '{entry.Email}': {errors}");
            }
        }
    }

    private sealed class AdminUserEntry
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
