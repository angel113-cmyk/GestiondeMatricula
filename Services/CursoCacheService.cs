using Microsoft.Extensions.Caching.Distributed;
using GestiondeMatricula.Models;
using GestiondeMatricula.Data;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace GestiondeMatricula.Services
{
    public class CursoCacheService : ICursoCacheService
    {
        private readonly ApplicationDbContext _context;
        private readonly IDistributedCache _cache;

        public CursoCacheService(ApplicationDbContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // MÉTODO 1: Obtener cursos activos desde cache o base de datos
        public async Task<List<Curso>> ObtenerCursosActivosCache()
        {
            var cacheKey = "cursos_activos_cache";
            var cachedData = await _cache.GetStringAsync(cacheKey);
            
            if (!string.IsNullOrEmpty(cachedData))
            {
                return JsonSerializer.Deserialize<List<Curso>>(cachedData);
            }

            // Si no está en cache, obtener de la base de datos
            var cursos = await ObtenerCursosActivos();
            
            // Guardar en cache
            var options = new DistributedCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(2));
                
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(cursos), options);
            
            return cursos;
        }

        // MÉTODO 2: Obtener todos los cursos (sin filtro de estado)
        public async Task<List<Curso>> ObtenerCursosActivos()
        {
            // Como no tienes propiedad Estado, obtenemos todos los cursos
            return await _context.Cursos.ToListAsync();
        }

        // MÉTODO 3: Invalidar el cache
        public async Task InvalidarCache()
        {
            var cacheKey = "cursos_activos_cache";
            await _cache.RemoveAsync(cacheKey);
        }

        // SI LA INTERFAZ TIENE MÁS MÉTODOS, IMPLEMENTA TODOS AQUÍ
    }
}