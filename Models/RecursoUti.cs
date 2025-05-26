using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DelCorp.Models
{
    public class RecursoUti
    {
        public long Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal? CantidadRecursosUti { get; set; }
        public decimal? PrecioUniRecursosUti { get; set; }
        public decimal? TotalRecursosUti { get; set; }
        public long? IdSubEtapa { get; set; }
        public long? IdRecurso { get; set; }
        public long? IdUniMedRe { get; set; }
        public Recurso Recurso { get; set; }
        public UniMedRe UniMedRe { get; set; }
        public SubEtapa SubEtapa { get; set; }
    }
}
