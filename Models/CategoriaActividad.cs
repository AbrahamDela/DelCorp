using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DelCorp.Models
{
    public class CategoriaActividad
    {
        public long IdCategoriaActividad { get; set; }
        public DateTime CreatedAt { get; set; }
        public string NombreCategoriaActividad { get; set; }
        public bool EsContable { get; set; } = true;
    }
}
