using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DelCorp.Models.Local
{
    public class LocalEtapa
    {
        [PrimaryKey, AutoIncrement]
        public long Id { get; set; }
        public long? ServerId { get; set; }
        public bool IsSynced { get; set; }
        public DateTime CreatedAt { get; set; }
        public long NumeroEtapa { get; set; }
        public string ActividadEtapa { get; set; }
        public decimal? CantidadEtapa { get; set; }
        public decimal? MontoTotalEtapa { get; set; }
        public decimal? MontoEjeEtapa { get; set; }
        public long? DiasCalEtapa { get; set; }
        public long? DiasEjeEtapa { get; set; }
        public decimal? ProgresoEtapa { get; set; }
        public long IdPresupuesto { get; set; }
        public long? IdUniMedida { get; set; }
    }
}
