
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestiondeMatricula.Models;
using GestiondeMatricula.Data;
using GestiondeMatricula.Services;

namespace GestiondeMatricula.Controllers
{
    [Authorize(Roles = "Coordinador")]
    public class CoordinadorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ICursoCacheService _cursoCacheService;

        public CoordinadorController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            ICursoCacheService cursoCacheService)
        {
            _context = context;
            _userManager = userManager;
            _cursoCacheService = cursoCacheService;
        }

        
        public IActionResult Index()
        {
            return View();
        }
        
        public async Task<IActionResult> Cursos()
        {
            var cursos = await _context.Cursos.ToListAsync();
            return View(cursos);
        }

        
        public IActionResult CrearCurso()
        {
            return View();
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearCurso(Curso curso)
        {
            if (ModelState.IsValid)
            {
                
                if (curso.HorarioInicio >= curso.HorarioFin)
                {
                    ModelState.AddModelError("", "El horario de inicio debe ser anterior al horario de fin");
                    return View(curso);
                }

                if (curso.Creditos <= 0)
                {
                    ModelState.AddModelError("", "Los créditos deben ser mayores a 0");
                    return View(curso);
                }

                
                if (await _context.Cursos.AnyAsync(c => c.Codigo == curso.Codigo))
                {
                    ModelState.AddModelError("Codigo", "Ya existe un curso con este código");
                    return View(curso);
                }

                curso.Activo = true;
                _context.Add(curso);
                await _context.SaveChangesAsync();

                
                await _cursoCacheService.InvalidarCache();

                TempData["Success"] = "Curso creado exitosamente";
                return RedirectToAction(nameof(Cursos));
            }
            return View(curso);
        }

        
        public async Task<IActionResult> EditarCurso(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var curso = await _context.Cursos.FindAsync(id);
            if (curso == null)
            {
                return NotFound();
            }
            return View(curso);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarCurso(int id, Curso curso)
        {
            if (id != curso.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    
                    if (curso.HorarioInicio >= curso.HorarioFin)
                    {
                        ModelState.AddModelError("", "El horario de inicio debe ser anterior al horario de fin");
                        return View(curso);
                    }

                    if (curso.Creditos <= 0)
                    {
                        ModelState.AddModelError("", "Los créditos deben ser mayores a 0");
                        return View(curso);
                    }

                    
                    if (await _context.Cursos.AnyAsync(c => c.Codigo == curso.Codigo && c.Id != id))
                    {
                        ModelState.AddModelError("Codigo", "Ya existe un curso con este código");
                        return View(curso);
                    }

                    _context.Update(curso);
                    await _context.SaveChangesAsync();

                    
                    await _cursoCacheService.InvalidarCache();

                    TempData["Success"] = "Curso actualizado exitosamente";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CursoExists(curso.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Cursos));
            }
            return View(curso);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DesactivarCurso(int id)
        {
            var curso = await _context.Cursos.FindAsync(id);
            if (curso == null)
            {
                return NotFound();
            }

            curso.Activo = false;
            _context.Update(curso);
            await _context.SaveChangesAsync();

            
            await _cursoCacheService.InvalidarCache();

            TempData["Success"] = "Curso desactivado exitosamente";
            return RedirectToAction(nameof(Cursos));
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivarCurso(int id)
        {
            var curso = await _context.Cursos.FindAsync(id);
            if (curso == null)
            {
                return NotFound();
            }

            curso.Activo = true;
            _context.Update(curso);
            await _context.SaveChangesAsync();

            
            await _cursoCacheService.InvalidarCache();

            TempData["Success"] = "Curso activado exitosamente";
            return RedirectToAction(nameof(Cursos));
        }

        private bool CursoExists(int id)
        {
            return _context.Cursos.Any(e => e.Id == id);
        }
        
        public async Task<IActionResult> Matriculas()
        {
            var cursosConMatriculas = await _context.Cursos
                .Include(c => c.Matriculas)
                .ThenInclude(m => m.Usuario)
                .Where(c => c.Activo)
                .ToListAsync();

            return View(cursosConMatriculas);
        }

        
        public async Task<IActionResult> MatriculasPorCurso(int cursoId)
        {
            var curso = await _context.Cursos.FindAsync(cursoId);
            if (curso == null)
            {
                return NotFound();
            }

            var matriculas = await _context.Matriculas
                .Include(m => m.Usuario)
                .Where(m => m.CursoId == cursoId)
                .ToListAsync();

            ViewBag.Curso = curso;
            return View(matriculas);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarMatricula(int id)
        {
            var matricula = await _context.Matriculas
                .Include(m => m.Curso)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (matricula == null)
            {
                return NotFound();
            }

            
            var matriculasConfirmadas = await _context.Matriculas
                .CountAsync(m => m.CursoId == matricula.CursoId && m.Estado == EstadoMatricula.Confirmada);

            if (matriculasConfirmadas >= matricula.Curso.CupoMaximo)
            {
                TempData["Error"] = "No se puede confirmar la matrícula: cupo máximo alcanzado";
                return RedirectToAction(nameof(MatriculasPorCurso), new { cursoId = matricula.CursoId });
            }

            matricula.Estado = EstadoMatricula.Confirmada;
            _context.Update(matricula);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Matrícula confirmada exitosamente";
            return RedirectToAction(nameof(MatriculasPorCurso), new { cursoId = matricula.CursoId });
        }

        // POST: Coordinador/CancelarMatricula/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelarMatricula(int id)
        {
            var matricula = await _context.Matriculas.FindAsync(id);
            if (matricula == null)
            {
                return NotFound();
            }

            matricula.Estado = EstadoMatricula.Cancelada;
            _context.Update(matricula);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Matrícula cancelada exitosamente";
            return RedirectToAction(nameof(MatriculasPorCurso), new { cursoId = matricula.CursoId });
        }
        public async Task<IActionResult> Reportes()
        {
            
            ViewBag.TotalCursos = await _context.Cursos.CountAsync();
            ViewBag.CursosActivos = await _context.Cursos.CountAsync(c => c.Activo);
            ViewBag.TotalMatriculas = await _context.Matriculas.CountAsync();

            ViewBag.MatriculasConfirmadas = await _context.Matriculas
                .CountAsync(m => m.Estado == EstadoMatricula.Confirmada);
            ViewBag.MatriculasPendientes = await _context.Matriculas
                .CountAsync(m => m.Estado == EstadoMatricula.Pendiente);
            ViewBag.MatriculasCanceladas = await _context.Matriculas
                .CountAsync(m => m.Estado == EstadoMatricula.Cancelada);

            
            ViewBag.TopCursos = await _context.Cursos
                .Include(c => c.Matriculas)
                .Where(c => c.Activo)
                .Select(c => new {
                    c.Nombre,
                    MatriculasCount = c.Matriculas.Count(m => m.Estado != EstadoMatricula.Cancelada)
                })
                .OrderByDescending(x => x.MatriculasCount)
                .Take(5)
                .ToListAsync();

            return View();
        }
        
    }
}