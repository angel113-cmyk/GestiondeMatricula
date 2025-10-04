using Microsoft.Extensions.Caching.Distributed;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using GestiondeMatricula.Data;
using GestiondeMatricula.Models;

namespace GestiondeMatricula.Services
{
    public class CursoCacheService
    {
        private readonly ApplicationDbContext _context;
        private readonly IDistributedCache _cache;

        public CursoCacheService(ApplicationDbContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<List<Curso>> GetCursosCacheAsync()
        {
            // Intentar obtener del cache
            var cachedCursos = await _cache.GetStringAsync("cursos_activos");
            if (cachedCursos != null)
            {
                return JsonSerializer.Deserialize<List<Curso>>(cachedCursos);
            }

            // Si no está en cache, obtener de BD
            var cursos = await _context.Cursos
                .Include(c => c.Matriculas)
                .Where(c => c.Activo)
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            // Guardar en cache por 60 segundos
            var options = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(60));
                
            await _cache.SetStringAsync("cursos_activos", JsonSerializer.Serialize(cursos), options);

            return cursos;
        }

        public async Task ClearCacheAsync()
        {
            await _cache.RemoveAsync("cursos_activos");
        }
    }
}