using CMCSWeb.Data;
using CMCSWeb.Models;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// Configure Entity Framework with SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;

    // SignIn settings
    options.SignIn.RequireConfirmedEmail = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Configure Application Cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(1);
});

// Allow large form submissions (important for file uploads)
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104857600; // 100 MB limit
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Add Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Add Session
app.UseSession();

// Seed default roles and users
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await SeedData.Initialize(services);
}

// Ensure database connection is valid at startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        db.Database.Migrate(); // Apply migrations automatically
        Console.WriteLine("✅ Database connection and migrations successful.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Database connection failed: {ex.Message}");
    }
}

// Default routing
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// Seed Data Class
public static class SeedData
{
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        string[] roleNames = { "HR", "Lecturer", "Coordinator", "Manager" };

        // Create roles
        foreach (var roleName in roleNames)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // Create default HR user
        var hrUser = await userManager.FindByEmailAsync("hr@cmcs.com");
        if (hrUser == null)
        {
            hrUser = new ApplicationUser
            {
                UserName = "hr@cmcs.com",
                Email = "hr@cmcs.com",
                FullName = "HR Administrator",
                Department = "Human Resources",
                Phone = "0123456789",
                UserRole = "HR",
                CreatedAt = DateTime.Now
            };

            var createPowerUser = await userManager.CreateAsync(hrUser, "Password123!");
            if (createPowerUser.Succeeded)
            {
                await userManager.AddToRoleAsync(hrUser, "HR");
            }
        }

        // Create default lecturer
        var lecturerUser = await userManager.FindByEmailAsync("lecturer@cmcs.com");
        if (lecturerUser == null)
        {
            lecturerUser = new ApplicationUser
            {
                UserName = "lecturer@cmcs.com",
                Email = "lecturer@cmcs.com",
                FullName = "John Lecturer",
                Department = "Computer Science",
                Phone = "0123456789",
                HourlyRate = 250.00m,
                UserRole = "Lecturer",
                CreatedAt = DateTime.Now
            };

            var createLecturer = await userManager.CreateAsync(lecturerUser, "Password123!");
            if (createLecturer.Succeeded)
            {
                await userManager.AddToRoleAsync(lecturerUser, "Lecturer");
            }
        }

        // Create default coordinator
        var coordinatorUser = await userManager.FindByEmailAsync("coordinator@cmcs.com");
        if (coordinatorUser == null)
        {
            coordinatorUser = new ApplicationUser
            {
                UserName = "coordinator@cmcs.com",
                Email = "coordinator@cmcs.com",
                FullName = "Sarah Coordinator",
                Department = "Academic Affairs",
                Phone = "0123456789",
                UserRole = "Coordinator",
                CreatedAt = DateTime.Now
            };

            var createCoordinator = await userManager.CreateAsync(coordinatorUser, "Password123!");
            if (createCoordinator.Succeeded)
            {
                await userManager.AddToRoleAsync(coordinatorUser, "Coordinator");
            }
        }

        // Create default manager
        var managerUser = await userManager.FindByEmailAsync("manager@cmcs.com");
        if (managerUser == null)
        {
            managerUser = new ApplicationUser
            {
                UserName = "manager@cmcs.com",
                Email = "manager@cmcs.com",
                FullName = "Michael Manager",
                Department = "Management",
                Phone = "0123456789",
                UserRole = "Manager",
                CreatedAt = DateTime.Now
            };

            var createManager = await userManager.CreateAsync(managerUser, "Password123!");
            if (createManager.Succeeded)
            {
                await userManager.AddToRoleAsync(managerUser, "Manager");
            }
        }
    }
}