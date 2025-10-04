using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestiondeMatricula.Data;
using GestiondeMatricula.ViewModels;
using GestiondeMatricula.Models;
using GestiondeMatricula.Services;

namespace GestiondeMatricula.Controllers
{
    public class CursosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICursoCacheService _cacheService;
        private readonly ILogger<CursosController> _logger;

        public CursosController(ApplicationDbContext context, ICursoCacheService cacheService, ILogger<CursosController> logger)
        {
            _context = context;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<IActionResult> Catalogo(CursoFiltroViewModel filtro)
        {
            // ✅ USAR CACHE para obtener cursos
            var cursos = await _cacheService.ObtenerCursosActivosCache();

            // Aplicar filtros en memoria
            var cursosFiltrados = cursos.AsQueryable();

            if (!string.IsNullOrEmpty(filtro.Nombre))
            {
                var nombreLower = filtro.Nombre.ToLower();
                cursosFiltrados = cursosFiltrados.Where(c => 
                    c.Nombre.ToLower().Contains(nombreLower) || 
                    c.Codigo.ToLower().Contains(nombreLower));
            }

            if (filtro.CreditosMin.HasValue)
            {
                cursosFiltrados = cursosFiltrados.Where(c => c.Creditos >= filtro.CreditosMin.Value);
            }

            if (filtro.CreditosMax.HasValue)
            {
                cursosFiltrados = cursosFiltrados.Where(c => c.Creditos <= filtro.CreditosMax.Value);
            }

            if (filtro.HorarioDesde.HasValue)
            {
                cursosFiltrados = cursosFiltrados.Where(c => c.HorarioInicio >= filtro.HorarioDesde.Value);
            }

            if (filtro.HorarioHasta.HasValue)
            {
                cursosFiltrados = cursosFiltrados.Where(c => c.HorarioFin <= filtro.HorarioHasta.Value);
            }

            filtro.Cursos = cursosFiltrados.ToList();
            return View(filtro);
        }

        public async Task<IActionResult> Detalle(int id)
        {
            // ✅ GUARDAR EN SESSION el último curso visitado
            var curso = await _context.Cursos
                .Include(c => c.Matriculas)
                .FirstOrDefaultAsync(c => c.Id == id && c.Activo);

            if (curso == null)
            {
                return NotFound();
            }

            // Guardar en session
            HttpContext.Session.SetString("UltimoCursoVisitado", curso.Nombre);
            HttpContext.Session.SetInt32("UltimoCursoId", curso.Id);

            _logger.LogInformation($"✅ Session UPDATED - Último curso: {curso.Nombre}");

            return View(curso);
        }


        public IActionResult ObtenerUltimoCursoVisitado()
        {
            var ultimoCurso = HttpContext.Session.GetString("UltimoCursoVisitado");
            var ultimoCursoId = HttpContext.Session.GetInt32("UltimoCursoId");

            if (string.IsNullOrEmpty(ultimoCurso))
            {
                return Json(new { existe = false });
            }

            return Json(new { 
                existe = true,
                nombre = ultimoCurso,
                id = ultimoCursoId,
                url = Url.Action("Detalle", "Cursos", new { id = ultimoCursoId })
            });
        }
    }
}