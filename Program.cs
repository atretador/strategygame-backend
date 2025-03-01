using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StrategyGame.Data;
using MongoDB.Driver;
using StrategyGame.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using StrategyGame.Models;
using StrategyGame.BackgroundServices;
using StrategyGame.Context;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.Cookies;
using StrategyGame.Mapping;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<UserManager<ApplicationUser>>();

builder.Services.AddRazorPages();

// Bind MongoDB settings from configuration
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDbSettings"));

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
    return new MongoClient(settings.ConnectionString);
});

builder.Services.AddSingleton(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
    return new MongoDbContext(client, settings.DatabaseName);
});

builder.Services.AddMemoryCache();

builder.Services.AddHostedService<CityContentsBackgroundService>();
builder.Services.AddScoped<IBattleSimulator, BattleSimulator>();
builder.Services.AddSingleton<IBuildingConstructionService, BuildingConstructionService>();
builder.Services.AddSingleton<IUnitManagerService, UnitManagerService>();
builder.Services.AddSingleton<IMilitaryUnitManagerService, MilitaryUnitManagerService>();
builder.Services.AddSingleton<IBuildingManagerService, BuildingManagerService>();
builder.Services.AddSingleton<IResearchManagerService, ResearchManagerService>();
builder.Services.AddSingleton<IResourceManagerService, ResourceManagerService>();
builder.Services.AddSingleton<IWorldManagerService, WorldManagerService>();
builder.Services.AddSingleton<IFactionManagerService, FactionManagerService>();
builder.Services.AddScoped<ICityService, CityService>();

// mapper
builder.Services.AddAutoMapper(typeof(FactionMappingProfile));
builder.Services.AddAutoMapper(typeof(WorldSettingsProfile));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", builder =>
    {
        builder.WithOrigins("http://localhost:3000") // Add your React frontend URL here
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials(); // Ensure cookies are sent with requests
    });
});


builder.Services.Configure<IdentityOptions>(options =>
{
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";

        // Set a long expiration time for persistent login
        options.ExpireTimeSpan = TimeSpan.FromDays(365); // 1 year duration

        // Ensure the cookie persists across sessions by setting the MaxAge property
        options.Cookie.MaxAge = TimeSpan.FromDays(365); // 1 year duration

        options.SlidingExpiration = false; // Optional: Disable sliding expiration to expire the cookie exactly after the set time
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
    {
        policy.RequireRole("Admin");
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();  // This is needed for Swagger to generate endpoints
builder.Services.AddSwaggerGen();  // This adds the Swagger generator

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
    app.UseSwagger();
    app.UseSwaggerUI();
    // Disable HTTPS redirection in development if you're not using HTTPS locally
    app.UseHttpsRedirection();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseCors("AllowSpecificOrigin");  // Use the specific policy
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Check and create the initial admin user at startup
var setupFlag = builder.Configuration["Setup"];
var adminPassword = builder.Configuration["AdminPassword"];

if (setupFlag == "true" && !string.IsNullOrEmpty(adminPassword))
{
    using (var scope = app.Services.CreateScope())
    {
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // Check if the Admin role exists, if not, create it
        var adminRole = await roleManager.FindByNameAsync("Admin");
        if (adminRole == null)
        {
            adminRole = new IdentityRole("Admin");
            await roleManager.CreateAsync(adminRole);
        }

        // Check if the admin user already exists
        var adminUser = await userManager.FindByEmailAsync("admin@example.com");
        if (adminUser == null)
        {
            // Create the admin user
            adminUser = new ApplicationUser
            {
                UserName = "admin",
                Email = "admin@example.com"
            };

            var createUserResult = await userManager.CreateAsync(adminUser, adminPassword);
            if (createUserResult.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
                Console.WriteLine("Admin user created successfully.");
            }
            else
            {
                Console.WriteLine("Failed to create admin user: " + string.Join(", ", createUserResult.Errors.Select(e => e.Description)));
            }
        }
        else
        {
            Console.WriteLine("Admin user already exists. Skipping setup.");
        }
    }
}

app.Run();
