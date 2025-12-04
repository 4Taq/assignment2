using COMP_2139_Assignment_1.Data;
using COMP_2139_Assignment_1.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);


Log.Logger = new LoggerConfiguration().WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateBootstrapLogger();
builder.Host.UseSerilog();


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedEmail = true;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages(); 

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    string[] roleNames = { "Admin", "Organizer", "Attendee" };
    foreach (var role in roleNames)
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));

    var adminEmail = "admin@event.com";
    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var admin = new ApplicationUser { UserName = adminEmail, Email = adminEmail, FullName = "Site Admin", EmailConfirmed = true };
        await userManager.CreateAsync(admin, "Admin123!");
        await userManager.AddToRoleAsync(admin, "Admin");
    }
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    
    var categories = new[] { "Concert", "Sports", "Conference", "Workshop", "Comedy" };
    foreach (var catName in categories)
    {
        if (!await context.Categories.AnyAsync(c => c.Name == catName))
        {
            context.Categories.Add(new Category { Name = catName });
        }
    }
    await context.SaveChangesAsync();

// Seed 5 sample events if none exist
    if (!await context.Events.AnyAsync())
    {
        var adminUser = await userManager.FindByEmailAsync("admin@event.com");
        
        var today = DateTime.Today; 
        var utcToday = today; 

        var sampleEvents = new[]
        {
            new Event 
            { 
                Name = "Concert", 
                StartDate = utcToday.AddDays(30), 
                EndDate = utcToday.AddDays(31), 
                Capacity = 5000, 
                Price = 149.99, 
                CategoryId = 1, 
                OrganizerId = adminUser.Id 
            },
            new Event 
            { 
                Name = "Football Finals Watch Party", 
                StartDate = utcToday.AddDays(10), 
                EndDate = utcToday.AddDays(10), 
                Capacity = 200, 
                Price = 49.99, 
                CategoryId = 2, 
                OrganizerId = adminUser.Id 
            },
            new Event 
            { 
                Name = ".NET Conference", 
                StartDate = utcToday.AddDays(60), 
                EndDate = utcToday.AddDays(62), 
                Capacity = 300, 
                Price = 299.99, 
                CategoryId = 3, 
                OrganizerId = adminUser.Id 
            }
        };

        
        foreach (var ev in sampleEvents)
        {
            ev.StartDate = DateTime.SpecifyKind(ev.StartDate, DateTimeKind.Utc);
            ev.EndDate = DateTime.SpecifyKind(ev.EndDate, DateTimeKind.Utc);
        }

        context.Events.AddRange(sampleEvents);
        await context.SaveChangesAsync();
    }
    
}


// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();