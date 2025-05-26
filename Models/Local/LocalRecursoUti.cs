using SQLite;
using System;

namespace DelCorp.Models.Local
{
    [Table("LocalRecursosUti")]
    public class LocalRecursoUti
    {
        [PrimaryKey, AutoIncrement]
        public long LocalId { get; set; }
        public long? ServerId { get; set; } // Null si se crear primero offline
        [Indexed]
        public DateTime CreatedAt { get; set; }
        public decimal? CantidadRecursosUti { get; set; }
        public decimal? PrecioUniRecursosUti { get; set; }
        public decimal? TotalRecursosUti { get; set; }
        public long? IdSubEtapa { get; set; } // ServerId de SubEtapa
        public long? IdRecurso { get; set; }  // ServerId de Recurso
        public long? IdUniMedRe { get; set; } // ServerId de UniMedRe
        public bool IsSynced { get; set; }
    }
}