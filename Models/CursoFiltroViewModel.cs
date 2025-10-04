using System.ComponentModel.DataAnnotations;
using GestiondeMatricula.Models;

namespace GestiondeMatricula.ViewModels
{
    public class CursoFiltroViewModel
    {
        [Display(Name = "Nombre del curso")]
        public string? Nombre { get; set; }

        [Display(Name = "Créditos mínimos")]
        [Range(1, 10, ErrorMessage = "Los créditos deben estar entre 1 y 10")]
        public int? CreditosMin { get; set; }

        [Display(Name = "Créditos máximos")]
        [Range(1, 10, ErrorMessage = "Los créditos deben estar entre 1 y 10")]
        public int? CreditosMax { get; set; }

        [Display(Name = "Horario desde")]
        [DataType(DataType.Time)]
        public TimeSpan? HorarioDesde { get; set; }

        [Display(Name = "Horario hasta")]
        [DataType(DataType.Time)]
        public TimeSpan? HorarioHasta { get; set; }

        public List<Curso> Cursos { get; set; } = new List<Curso>();
    }
}