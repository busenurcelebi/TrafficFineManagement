
using Microsoft.AspNetCore.Identity;

namespace TrafficFineManagement.Seed
{
    public static class IdentitySeeder
    {
        public const string AdminRole = "Admin";
        public const string ManagerRole = "Manager";
        public const string FinanceRole = "Finance";

        public static async Task SeedRolesAsync(
            IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider
                .GetRequiredService<RoleManager<IdentityRole>>();

            string[] roles =
            {
                AdminRole,
                ManagerRole,
                FinanceRole
            };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var result = await roleManager.CreateAsync(
                        new IdentityRole(role));

                    if (!result.Succeeded)
                    {
                        throw new InvalidOperationException(
                            string.Join(", ",
                                result.Errors.Select(e => e.Description)));
                    }
                }
            }
        }

        public static async Task SeedTestUserRolesAsync(
            IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider
                .GetRequiredService<UserManager<IdentityUser>>();

            var testUsers = new Dictionary<string, string>
            {
                { "manager@test.local", ManagerRole },
                { "finance@test.local", FinanceRole }
            };

            foreach (var testUser in testUsers)
            {
                var user = await userManager
                    .FindByEmailAsync(testUser.Key);

                if (user == null)
                {
                    throw new InvalidOperationException(
                        $"Test kullanıcısı bulunamadı: {testUser.Key}");
                }

                if (!await userManager.IsInRoleAsync(
                    user, testUser.Value))
                {
                    var result = await userManager
                        .AddToRoleAsync(user, testUser.Value);

                    if (!result.Succeeded)
                    {
                        throw new InvalidOperationException(
                            string.Join(", ",
                                result.Errors.Select(e => e.Description)));
                    }
                }
            }
        }

        public static async Task SeedDemoUsersAsync(
            IServiceProvider serviceProvider,
            IConfiguration configuration)
        {
            var password = configuration["DemoUsers:Password"];

            // Demo şifresi tanımlanmamışsa hesap oluşturma.
            if (string.IsNullOrWhiteSpace(password))
            {
                return;
            }

            var userManager = serviceProvider
                .GetRequiredService<UserManager<IdentityUser>>();

            var demoUsers = new Dictionary<string, string?>
    {
        { "manager@test.local", ManagerRole },
        { "finance@test.local", FinanceRole },
        { "user@test.local", null }
    };

            foreach (var demoUser in demoUsers)
            {
                var user = await userManager.FindByEmailAsync(demoUser.Key);

                if (user == null)
                {
                    user = new IdentityUser
                    {
                        UserName = demoUser.Key,
                        Email = demoUser.Key,
                        EmailConfirmed = true
                    };

                    var createResult = await userManager
                        .CreateAsync(user, password);

                    if (!createResult.Succeeded)
                    {
                        throw new InvalidOperationException(
                            string.Join(", ",
                                createResult.Errors.Select(e => e.Description)));
                    }
                }

                if (demoUser.Value != null &&
                    !await userManager.IsInRoleAsync(user, demoUser.Value))
                {
                    var roleResult = await userManager
                        .AddToRoleAsync(user, demoUser.Value);

                    if (!roleResult.Succeeded)
                    {
                        throw new InvalidOperationException(
                            string.Join(", ",
                                roleResult.Errors.Select(e => e.Description)));
                    }
                }
            }
        }

    }
}
