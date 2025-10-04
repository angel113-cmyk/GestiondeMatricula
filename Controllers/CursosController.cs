using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestiondeMatricula.Data;
using GestiondeMatricula.ViewModels;
using GestiondeMatricula.Models;

namespace GestiondeMatricula.Controllers
{
    public class CursosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CursosController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Catalogo(CursoFiltroViewModel filtro)
        {
            // Obtener todos los cursos activos primero
            var cursosQuery = _context.Cursos
                .Include(c => c.Matriculas)
                .Where(c => c.Activo)
                .AsQueryable();

            // Aplicar filtros de manera segura
            if (!string.IsNullOrEmpty(filtro.Nombre))
            {
                // Convertir a minúsculas para búsqueda case-insensitive
                var nombreLower = filtro.Nombre.ToLower();
                cursosQuery = cursosQuery.Where(c => 
                    c.Nombre.ToLower().Contains(nombreLower) || 
                    c.Codigo.ToLower().Contains(nombreLower));
            }

            if (filtro.CreditosMin.HasValue)
            {
                cursosQuery = cursosQuery.Where(c => c.Creditos >= filtro.CreditosMin.Value);
            }

            if (filtro.CreditosMax.HasValue)
            {
                cursosQuery = cursosQuery.Where(c => c.Creditos <= filtro.CreditosMax.Value);
            }

            // Para filtros de horario, ejecutar en memoria después de obtener datos
            var cursos = await cursosQuery.OrderBy(c => c.Nombre).ToListAsync();

            // Aplicar filtros de horario en memoria
            if (filtro.HorarioDesde.HasValue)
            {
                cursos = cursos.Where(c => c.HorarioInicio >= filtro.HorarioDesde.Value).ToList();
            }

            if (filtro.HorarioHasta.HasValue)
            {
                cursos = cursos.Where(c => c.HorarioFin <= filtro.HorarioHasta.Value).ToList();
            }

            filtro.Cursos = cursos;
            return View(filtro);
        }

        public async Task<IActionResult> Detalle(int id)
        {
            var curso = await _context.Cursos
                .Include(c => c.Matriculas)
                .FirstOrDefaultAsync(c => c.Id == id && c.Activo);

            if (curso == null)
            {
                return NotFound();
            }

            return View(curso);
        }

        // Validaciones server-side (simplificadas)
        [AcceptVerbs("GET", "POST")]
        public IActionResult VerifyCreditos(int creditos)
        {
            if (creditos <= 0)
            {
                return Json("Los créditos deben ser mayores a 0.");
            }
            return Json(true);
        }

        [AcceptVerbs("GET", "POST")]
        public IActionResult VerifyHorario(TimeSpan horarioInicio, TimeSpan horarioFin)
        {
            if (horarioInicio >= horarioFin)
            {
                return Json("El horario de inicio debe ser anterior al horario de fin.");
            }
            return Json(true);
        }
    }
}