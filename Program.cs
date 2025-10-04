using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GestiondeMatricula.Data;
using GestiondeMatricula.Services;
using GestiondeMatricula.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>() // ✅ AGREGAR ESTO
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

// ✅ REGISTRAR SERVICIOS DE CACHE
builder.Services.AddScoped<ICursoCacheService, CursoCacheService>();

// ✅ AGREGAR LOGGING PARA DEBUG
builder.Services.AddLogging();

// ✅ CONFIGURAR REDIS PARA SESSION Y CACHE
var redisConnection = builder.Configuration.GetConnectionString("RedisConnection");

if (!string.IsNullOrEmpty(redisConnection))
{
    // Configurar Redis Cache
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnection;
        options.InstanceName = "GestionMatriculas_";
    });

    // Configurar Session con Redis
    builder.Services.AddSession(options =>
    {
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.IdleTimeout = TimeSpan.FromMinutes(2);
    });
}
else
{
    // Fallback a memoria si Redis no está configurado
    builder.Services.AddDistributedMemoryCache();
    builder.Services.AddSession(options =>
    {
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.IdleTimeout = TimeSpan.FromMinutes(2);
    });
}

var app = builder.Build();

// ✅ CONFIGURACIÓN DE ROLES Y USUARIO COORDINADOR
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    
    string[] roleNames = { "Coordinador" };
    foreach (var roleName in roleNames)
    {
        var roleExist = await roleManager.RoleExistsAsync(roleName);
        if (!roleExist)
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

   
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    var coordinadorEmail = "coordinador@universidad.edu";

    var coordinadorUser = await userManager.FindByEmailAsync(coordinadorEmail);
    if (coordinadorUser == null)
    {
        coordinadorUser = new IdentityUser { 
            UserName = coordinadorEmail, 
            Email = coordinadorEmail,
            EmailConfirmed = true // ✅ Importante para poder loguearse
        };
        var result = await userManager.CreateAsync(coordinadorUser, "Coordinador123!");
        
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(coordinadorUser, "Coordinador");
            Console.WriteLine("✅ Usuario coordinador creado exitosamente");
        }
        else
        {
            Console.WriteLine($"❌ Error creando usuario coordinador: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
    }
    else
    {
        
        if (!await userManager.IsInRoleAsync(coordinadorUser, "Coordinador"))
        {
            await userManager.AddToRoleAsync(coordinadorUser, "Coordinador");
        }
        Console.WriteLine("✅ Usuario coordinador ya existe");
    }
}


if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); 
app.UseRouting();
app.UseSession();
app.UseAuthentication(); 
app.UseAuthorization();


try
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    
    Console.WriteLine("🚀 INICIANDO DbInitializer...");
    await DbInitializer.Initialize(context, userManager, roleManager);
    Console.WriteLine("✅ DbInitializer COMPLETADO");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ ERROR en DbInitializer: {ex.Message}");
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.Run();
