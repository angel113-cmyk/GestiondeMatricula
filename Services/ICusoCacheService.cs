using GestiondeMatricula.Models;

namespace GestiondeMatricula.Services
{
    public interface ICursoCacheService
    {
        Task<List<Curso>> ObtenerCursosActivosCache();
        Task<List<Curso>> ObtenerCursosActivos();
        Task InvalidarCache();
    }
}