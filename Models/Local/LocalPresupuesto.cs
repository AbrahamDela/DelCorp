using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DelCorp.Models.Local
{
    [Table("Presupuestos")]
    public class LocalPresupuesto
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public long? ServerId { get; set; }
        public bool IsSynced { get; set; }
        public DateTime CreatedAt { get; set; }
        public string NombrePresupuesto { get; set; }
        public decimal? TotalPresupuesto { get; set; }
        public decimal? MontoEjePresupuesto { get; set; }
        public DateTime? FechaInicioPresupuesto { get; set; }
        public DateTime? FechaFinPresupuesto { get; set; }
        public long? DiasCalePresupuesto { get; set; }
        public long? DiasEjePresupuesto { get; set; }
        public decimal? ProgresoPresupuesto { get; set; }
        public int IdProyecto { get; set; }
    }
}
