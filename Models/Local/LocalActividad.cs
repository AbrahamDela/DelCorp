using SQLite;
using System;

namespace DelCorp.Models.Local
{
    [Table("LocalActividades")]
    public class LocalActividad
    {
        [PrimaryKey]
        public long IdActividad { get; set; } // Asumimos que este es el ID de Supabase
        public DateTime CreatedAt { get; set; }
        public string NombreActividad { get; set; }
        public long? CategoriaActividadId { get; set; }
        public long? UnidadMedidaId { get; set; }
        public bool IsSynced { get; set; }
    }
}
