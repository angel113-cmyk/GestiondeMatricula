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
builder.Services.AddScoped<CursoCacheService>();

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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // ✅ AGREGAR ESTO
app.UseRouting();
app.UseSession();
app.UseAuthentication(); // ✅ AGREGAR ESTO (IMPORTANTE)
app.UseAuthorization();

// ✅ AGREGAR INICIALIZACIÓN DE DATOS
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
