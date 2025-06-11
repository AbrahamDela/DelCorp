using SQLite;
using System;

namespace DelCorp.Models.Local
{
    [Table("LocalRegistroRecursoUti")]
    public class LocalRegistroRecursoUti
    {
        [PrimaryKey, AutoIncrement]
        public long LocalId { get; set; }

        public long? ServerId { get; set; }
        public bool IsSynced { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? FechaRecursoUti { get; set; }
        public decimal? CantidadRecursosUti { get; set; }
        public decimal? PrecioUniRecursosUti { get; set; }
        public decimal? TotalRecursosUti { get; set; }
        public long? IdRecurso { get; set; }
        public long? IdSubEtapa { get; set; }
        public long? IdUniMedida { get; set; }
    }
}
