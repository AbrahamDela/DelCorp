using SQLite;
using System;

namespace DelCorp.Models.Local
{
    [Table("LocalCategoriasRec")]
    public class LocalCategoriaRec
    {
        [PrimaryKey, AutoIncrement]
        public long LocalId { get; set; } // Local DB Id especifico
        public long ServerId { get; set; } // Original Id de Supabase
        [Indexed]
        public DateTime CreatedAt { get; set; }
        public string NombreCatRec { get; set; }
        public bool IsSynced { get; set; }
    }
}
