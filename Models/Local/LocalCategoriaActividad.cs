using SQLite;
using System;

namespace DelCorp.Models.Local
{
    [Table("LocalCategoriasActividad")]
    public class LocalCategoriaActividad
    {
        [PrimaryKey] // Podrías dejar que Supabase genere el ID y usarlo aquí, o tener un LocalId autoincremental y un ServerId
        public long IdCategoriaActividad { get; set; } // Asumimos que este es el ID de Supabase
        public DateTime CreatedAt { get; set; }
        public string NombreCategoriaActividad { get; set; }
        public bool EsContable { get; set; }
        public bool IsSynced { get; set; }
    }
}
