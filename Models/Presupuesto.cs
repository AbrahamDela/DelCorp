using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DelCorp.Models
{
    public class Presupuesto
    {
        public long Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string NombrePresupuesto { get; set; }
        public decimal? TotalPresupuesto { get; set; }
        public decimal? MontoEjePresupuesto { get; set; }
        public DateTime? FechaInicioPresupuesto { get; set; }
        public DateTime? FechaFinPresupuesto { get; set; }
        public long? DiasCalePresupuesto { get; set; }
        public long? DiasEjePresupuesto { get; set; }
        public decimal? ProgresoPresupuesto { get; set; }
        public long IdProyecto { get; set; }
        public bool IsSynced { get; set; }
    }
}
