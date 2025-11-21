using CMCSWeb.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CMCSWeb.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // **CHANGED: Use Migrate instead of EnsureCreated for better migration handling**
            await context.Database.MigrateAsync();

            // Create roles
            string[] roles = { "HR", "Lecturer", "Coordinator", "Manager" };
            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                    Console.WriteLine($"Created role: {roleName}");
                }
            }

            // Create HR User
            var hrEmail = "hr@cmcs.com";
            var hrUser = await userManager.FindByEmailAsync(hrEmail);
            if (hrUser == null)
            {
                hrUser = new ApplicationUser
                {
                    UserName = hrEmail,
                    Email = hrEmail,
                    EmailConfirmed = true,
                    FullName = "HR Administrator",
                    Department = "Human Resources",
                    CreatedAt = DateTime.Now
                };
                var result = await userManager.CreateAsync(hrUser, "hr123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(hrUser, "HR");
                    Console.WriteLine("✓ Created HR user: hr@cmcs.com / hr123");
                }
                else
                {
                    Console.WriteLine($"✗ Failed to create HR user: {string.Join(", ", result.Errors)}");
                }
            }

            // Create Lecturer User
            var lecturerEmail = "lecturer@cmcs.com";
            var lecturerUser = await userManager.FindByEmailAsync(lecturerEmail);
            if (lecturerUser == null)
            {
                lecturerUser = new ApplicationUser
                {
                    UserName = lecturerEmail,
                    Email = lecturerEmail,
                    EmailConfirmed = true,
                    FullName = "John Lecturer",
                    Department = "Computer Science",
                    HourlyRate = 250.00m,
                    CreatedAt = DateTime.Now
                };
                var result = await userManager.CreateAsync(lecturerUser, "lecturer123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(lecturerUser, "Lecturer");
                    Console.WriteLine("✓ Created Lecturer user: lecturer@cmcs.com / lecturer123");
                }
            }

            // Create Coordinator User
            var coordinatorEmail = "coordinator@cmcs.com";
            var coordinatorUser = await userManager.FindByEmailAsync(coordinatorEmail);
            if (coordinatorUser == null)
            {
                coordinatorUser = new ApplicationUser
                {
                    UserName = coordinatorEmail,
                    Email = coordinatorEmail,
                    EmailConfirmed = true,
                    FullName = "Sarah Coordinator",
                    Department = "Engineering",
                    CreatedAt = DateTime.Now
                };
                var result = await userManager.CreateAsync(coordinatorUser, "coordinator123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(coordinatorUser, "Coordinator");
                    Console.WriteLine("✓ Created Coordinator user: coordinator@cmcs.com / coordinator123");
                }
            }

            // Create Manager User
            var managerEmail = "manager@cmcs.com";
            var managerUser = await userManager.FindByEmailAsync(managerEmail);
            if (managerUser == null)
            {
                managerUser = new ApplicationUser
                {
                    UserName = managerEmail,
                    Email = managerEmail,
                    EmailConfirmed = true,
                    FullName = "Mike Manager",
                    Department = "Management",
                    CreatedAt = DateTime.Now
                };
                var result = await userManager.CreateAsync(managerUser, "manager123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(managerUser, "Manager");
                    Console.WriteLine("✓ Created Manager user: manager@cmcs.com / manager123");
                }
            }

            // Create sample claims if none exist
            if (!await context.Claims.AnyAsync())
            {
                var claims = new List<Claim>
                {
                    new Claim
                    {
                        UserId = lecturerUser.Id,
                        HoursWorked = 40,
                        HourlyRate = 250.00m,
                        Documentation = "Advanced Programming lectures for March 2024",
                        Status = ClaimStatus.Approved,
                        SubmittedAt = DateTime.Now.AddDays(-10),
                        ClaimMonth = 3,
                        ClaimYear = 2024,
                        ApprovedAt = DateTime.Now.AddDays(-5),
                        ApprovedBy = "Manager"
                    },
                    new Claim
                    {
                        UserId = lecturerUser.Id,
                        HoursWorked = 35,
                        HourlyRate = 250.00m,
                        Documentation = "Student consultations and assignments marking",
                        Status = ClaimStatus.Pending,
                        SubmittedAt = DateTime.Now.AddDays(-3),
                        ClaimMonth = 4,
                        ClaimYear = 2024
                    },
                    new Claim
                    {
                        UserId = lecturerUser.Id,
                        HoursWorked = 42,
                        HourlyRate = 250.00m,
                        Documentation = "Course preparation and research",
                        Status = ClaimStatus.Verified,
                        SubmittedAt = DateTime.Now.AddDays(-7),
                        ClaimMonth = 4,
                        ClaimYear = 2024,
                        VerifiedAt = DateTime.Now.AddDays(-2),
                        VerifiedBy = "Coordinator"
                    }
                };
                await context.Claims.AddRangeAsync(claims);
                await context.SaveChangesAsync();
                Console.WriteLine("✓ Created sample claims");
            }

            Console.WriteLine("Seed data completed successfully!");
        }
    }
}