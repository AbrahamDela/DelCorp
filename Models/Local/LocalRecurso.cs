using SQLite;
using System;

namespace DelCorp.Models.Local
{
    [Table("LocalRecursos")]
    public class LocalRecurso
    {
        [PrimaryKey, AutoIncrement]
        public long LocalId { get; set; }
        public long ServerId { get; set; }
        [Indexed]
        public DateTime CreatedAt { get; set; }
        public string NombreRecurso { get; set; }
        public long? IdCatRec { get; set; }
        public bool IsSynced { get; set; }
    }
}