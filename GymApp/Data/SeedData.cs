using GymApp.Models;
using Microsoft.AspNetCore.Identity;

namespace GymApp.Data
{
    public class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider services)
        {
            var roleMgr = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userMgr = services.GetRequiredService<UserManager<Member>>();

            if (!await roleMgr.RoleExistsAsync("Admin"))
                await roleMgr.CreateAsync(new IdentityRole("Admin"));
            if (!await roleMgr.RoleExistsAsync("Member"))
                await roleMgr.CreateAsync(new IdentityRole("Member"));

            var email = "b231210016@sakarya.edu.tr";
            var admin = await userMgr.FindByEmailAsync(email);
            if (admin == null)
            {
                admin = new Member
                {
                    UserName = email,
                    Email = email,
                    FullName = "Admin Kullanıcı",
                    CreatedAt = DateTime.UtcNow
                };

                await userMgr.CreateAsync(admin, "sau");
                await userMgr.AddToRoleAsync(admin, "Admin");
            }
        }
    }
}
