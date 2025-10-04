using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace GestiondeMatricula.Models
{
    public enum EstadoMatricula
    {
        Pendiente,
        Confirmada,
        Cancelada
    }

    public class Matricula
    {
        public int Id { get; set; }
        
        public int CursoId { get; set; }
        
        public string UsuarioId { get; set; } = string.Empty;
        
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
        
        public EstadoMatricula Estado { get; set; } = EstadoMatricula.Pendiente;
        
        public virtual Curso Curso { get; set; } = null!;
        public virtual IdentityUser Usuario { get; set; } = null!;
    }
}