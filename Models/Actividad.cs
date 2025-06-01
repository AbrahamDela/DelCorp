using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DelCorp.Models
{
    public class Actividad
    {
        public long IdActividad { get; set; }
        public DateTime CreatedAt { get; set; }
        public string NombreActividad { get; set; }
        public long? CategoriaActividadId { get; set; }
        public CategoriaActividad CategoriaActividad { get; set; }
    }
}
