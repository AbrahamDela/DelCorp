using SQLite;
using System;

namespace DelCorp.Models.Local
{
    [Table("Projects")]
    public class LocalProject
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string NombreProyecto { get; set; }

        [Indexed]
        public DateTime CreatedAt { get; set; }

        public DateTime? FechaInicioProyecto { get; set; }

        public DateTime? FechaFinProyecto { get; set; }

        public string DireccionProyecto { get; set; }

        public string LatitudProyecto { get; set; }

        public string LongitudProyecto { get; set; }

        public string DescripcionProyecto { get; set; }

        // Campo para sincronización
        public bool IsSynced { get; set; } = true;

        // Servidor ID (para mapear con el ID de Supabase)
        public int? ServerId { get; set; }
    }
}