using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using GestiondeMatricula.Data;
using GestiondeMatricula.Models;

namespace GestiondeMatricula.Controllers
{
    [Authorize]
    public class MatriculasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public MatriculasController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> Inscribir(int cursoId)
        {
            try
            {
                Console.WriteLine($"🔍 Inscribir llamado - cursoId: {cursoId}");

                var usuario = await _userManager.GetUserAsync(User);
                if (usuario == null)
                {
                    TempData["Error"] = "Debe iniciar sesión para inscribirse.";
                    return RedirectToAction("Detalle", "Cursos", new { id = cursoId });
                }

                Console.WriteLine($"🔍 Usuario: {usuario.UserName}");

                var curso = await _context.Cursos
                    .Include(c => c.Matriculas)
                    .FirstOrDefaultAsync(c => c.Id == cursoId);

                if (curso == null)
                {
                    TempData["Error"] = "Curso no encontrado.";
                    return RedirectToAction("Catalogo", "Cursos");
                }

                Console.WriteLine($"🔍 Curso encontrado: {curso.Nombre}");

                // Validación 1: No duplicar matrícula
                var yaMatriculado = await _context.Matriculas
                    .AnyAsync(m => m.CursoId == cursoId && m.UsuarioId == usuario.Id);

                if (yaMatriculado)
                {
                    TempData["Error"] = "Ya estás matriculado en este curso.";
                    return RedirectToAction("Detalle", "Cursos", new { id = cursoId });
                }

                // Validación 2: Cupo disponible
                var cupoOcupado = curso.Matriculas.Count(m => m.Estado == EstadoMatricula.Confirmada);
                if (cupoOcupado >= curso.CupoMaximo)
                {
                    TempData["Error"] = "No hay cupos disponibles.";
                    return RedirectToAction("Detalle", "Cursos", new { id = cursoId });
                }

                // Crear matrícula
                var matricula = new Matricula
                {
                    CursoId = cursoId,
                    UsuarioId = usuario.Id,
                    FechaRegistro = DateTime.Now,
                    Estado = EstadoMatricula.Pendiente
                };

                _context.Matriculas.Add(matricula);
                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ Matrícula creada exitosamente");

                TempData["Success"] = "¡Inscripción exitosa!";
                return RedirectToAction("Detalle", "Cursos", new { id = cursoId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR: {ex.Message}");
                TempData["Error"] = "Error al procesar la inscripción.";
                return RedirectToAction("Detalle", "Cursos", new { id = cursoId });
            }
        }

        public async Task<IActionResult> MisMatriculas()
        {
            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null) return RedirectToPage("/Account/Login");

            var matriculas = await _context.Matriculas
                .Include(m => m.Curso)
                .Where(m => m.UsuarioId == usuario.Id)
                .ToListAsync();

            return View(matriculas);
        }
        
        
        public async Task<IActionResult> Cancelar(int id)
        {
            var matricula = await _context.Matriculas
                .Include(m => m.Curso)
                .FirstOrDefaultAsync(m => m.Id == id && m.UsuarioId == _userManager.GetUserId(User));

            if (matricula == null)
            {
                return NotFound();
            }

            // Solo permitir cancelar matrículas pendientes o confirmadas
            if (matricula.Estado == EstadoMatricula.Pendiente || matricula.Estado == EstadoMatricula.Confirmada)
            {
                matricula.Estado = EstadoMatricula.Cancelada;
                _context.Update(matricula);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Matrícula cancelada exitosamente";
            }
            else
            {
                TempData["Error"] = "No se puede cancelar esta matrícula";
            }

            return RedirectToAction(nameof(MisMatriculas));
        }
    }
}