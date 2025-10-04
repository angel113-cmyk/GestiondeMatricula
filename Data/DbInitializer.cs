using Microsoft.AspNetCore.Identity;
using GestiondeMatricula.Models;
using Microsoft.EntityFrameworkCore;

namespace GestiondeMatricula.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(ApplicationDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Aplicar migraciones
            await context.Database.MigrateAsync();

            // Verificar si ya existen datos
            if (context.Cursos.Any())
            {
                return;
            }

            // Crear rol Coordinador
            if (!await roleManager.RoleExistsAsync("Coordinador"))
            {
                await roleManager.CreateAsync(new IdentityRole("Coordinador"));
            }

            // Crear usuario coordinador
            var coordinador = new IdentityUser
            {
                UserName = "coordinador@universidad.edu",
                Email = "coordinador@universidad.edu",
                EmailConfirmed = true
            };

            if (await userManager.FindByEmailAsync(coordinador.Email) == null)
            {
                var result = await userManager.CreateAsync(coordinador, "Coordinador123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(coordinador, "Coordinador");
                }
            }

            // Crear cursos
            var cursos = new Curso[]
            {
                new Curso
                {
                    Codigo = "MAT101",
                    Nombre = "Matemáticas Básicas",
                    Creditos = 4,
                    CupoMaximo = 30,
                    HorarioInicio = new TimeSpan(8, 0, 0),
                    HorarioFin = new TimeSpan(10, 0, 0),
                    Activo = true
                },
                new Curso
                {
                    Codigo = "PROG202",
                    Nombre = "Programación I",
                    Creditos = 5,
                    CupoMaximo = 25,
                    HorarioInicio = new TimeSpan(10, 0, 0),
                    HorarioFin = new TimeSpan(12, 0, 0),
                    Activo = true
                },
                new Curso
                {
                    Codigo = "BD303",
                    Nombre = "Bases de Datos",
                    Creditos = 4,
                    CupoMaximo = 20,
                    HorarioInicio = new TimeSpan(14, 0, 0),
                    HorarioFin = new TimeSpan(16, 0, 0),
                    Activo = true
                }
            };

            context.Cursos.AddRange(cursos);
            await context.SaveChangesAsync();
        }
    }
}