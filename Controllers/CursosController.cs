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
            var query = _context.Cursos
                .Include(c => c.Matriculas)
                .Where(c => c.Activo);

            // Aplicar filtros
            if (!string.IsNullOrEmpty(filtro.Nombre))
            {
                query = query.Where(c => c.Nombre.Contains(filtro.Nombre) || c.Codigo.Contains(filtro.Nombre));
            }

            if (filtro.CreditosMin.HasValue)
            {
                query = query.Where(c => c.Creditos >= filtro.CreditosMin.Value);
            }

            if (filtro.CreditosMax.HasValue)
            {
                query = query.Where(c => c.Creditos <= filtro.CreditosMax.Value);
            }

            if (filtro.HorarioDesde.HasValue)
            {
                query = query.Where(c => c.HorarioInicio >= filtro.HorarioDesde.Value);
            }

            if (filtro.HorarioHasta.HasValue)
            {
                query = query.Where(c => c.HorarioFin <= filtro.HorarioHasta.Value);
            }

            filtro.Cursos = await query.OrderBy(c => c.Nombre).ToListAsync();
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

        // Validaciones server-side
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