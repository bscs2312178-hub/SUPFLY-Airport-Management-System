using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SUPFLY.Areas.Identity.Data;
using SUPFLY.Data;
using SUPFLY.Utilities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ✅ FIXED: Identity Configuration to explicitly ADD ROLES
builder.Services.AddDefaultIdentity<SUPFLYUser>(options =>
{
    // Sign-in settings 
    options.SignIn.RequireConfirmedAccount = false;

    // Password settings (Loosening for development guarantee)
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 4;
})
// 🔑 CRITICAL FIX: Add this line to enable Role management and loading
.AddRoles<IdentityRole>()
// -------------------------------------------------------------------------
.AddEntityFrameworkStores<ApplicationDbContext>();

// 🔑 FINAL, OBSCURE FIX: ADD THIS LINE. This explicitly tells Identity to use the 
// standard Role Claims Principal Factory, which resolves role-loading failures.
builder.Services.AddScoped<IUserClaimsPrincipalFactory<SUPFLYUser>, UserClaimsPrincipalFactory<SUPFLYUser, IdentityRole>>();
// ------------------------------------------------------------------------------------------------------------------------
builder.Services.AddRazorPages();
var app = builder.Build();

// Seeding the database (DbInitializer is fine here)
using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    try
    {
        await DbInitializer.InitializeAsync(serviceProvider);
    }
    catch (Exception ex)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database with roles.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 🔑 Authentication MUST be called before Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();
app.Run();