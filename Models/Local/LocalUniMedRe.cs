using SQLite;
using System;

namespace DelCorp.Models.Local
{
    [Table("LocalUniMedRe")]
    public class LocalUniMedRe
    {
        [PrimaryKey, AutoIncrement]
        public long LocalId { get; set; }
        public long ServerId { get; set; }
        [Indexed]
        public DateTime CreatedAt { get; set; }
        public string NombreUniMedRe { get; set; }
        public string AbreviaturaUniMedRe { get; set; }
        public bool IsSynced { get; set; }
    }
}