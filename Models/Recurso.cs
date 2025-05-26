using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DelCorp.Models
{
    public class Recurso
    {
        public long Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string NombreRecurso { get; set; }
        public long? IdCatRec { get; set; }

        // poropiedad de navegacion (para mayor comodidad)
        public CategoriaRec CategoriaRec { get; set; }
    }
}
