using Microsoft.AspNetCore.Identity;
using SaludPortal.Admin.Models;

namespace SaludPortal.Admin.Endpoints;

public static class AccountEndpoints
{
    public static void MapAccountEndpoints(this IEndpointRouteBuilder routes)
    {
        // Login must run outside the Blazor circuit so cookies can be written to HTTP headers.
        routes.MapPost("/account/login", async (
            HttpContext ctx,
            SignInManager<IdentityUser> signInManager) =>
        {
            var form = await ctx.Request.ReadFormAsync();
            var email = form["email"].ToString().Trim();
            var password = form["password"].ToString();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                return Results.Redirect("/?error=credentials");

            var result = await signInManager.PasswordSignInAsync(
                email, password, isPersistent: false, lockoutOnFailure: true);

            if (result.Succeeded)
                return Results.Redirect("/dashboard");

            if (result.IsLockedOut)
                return Results.Redirect("/?error=locked");

            return Results.Redirect("/?error=credentials");
        })
        .DisableAntiforgery()
        .WithName("Login")
        .WithSummary("Authenticate user")
        .WithDescription("Authenticates an admin user using form fields email/password and redirects to the appropriate page.")
        .Accepts<LoginForm>("application/x-www-form-urlencoded")
        .Produces(StatusCodes.Status302Found);

        // Legacy /login bookmarks → home (login form lives on "/").
        routes.MapGet("/login", () => Results.Redirect("/"))
            .WithName("LoginRedirect")
            .ExcludeFromDescription();

        routes.MapPost("/logout", async (HttpContext ctx, SignInManager<IdentityUser> signInManager) =>
        {
            await signInManager.SignOutAsync();
            return Results.Redirect("/");
        })
        .WithName("Logout")
        .WithSummary("Sign out current user")
        .WithDescription("Signs out the authenticated admin user and redirects to home.")
        .Produces(StatusCodes.Status302Found);
    }
}
