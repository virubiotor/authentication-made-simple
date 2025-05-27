namespace IdentityServer;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

public static class SeedData
{
    public static async Task EnsureSeedData(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

        var user = await userManager.FindByNameAsync("DotnetAdmin");
        if (user == null)
        {
            user = new IdentityUser("DotnetAdmin")
            {
                Email = "dotnet@example.org",
                EmailConfirmed = true
            };
            
            // WARNING: Never do this!! Only for demo purposes!
            var result = await userManager.CreateAsync(user, "Test1!");

            if (!result.Succeeded)
                throw new Exception("Failed to create user: " + string.Join(", ", result.Errors));
        }
    }
}