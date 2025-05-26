using Postgrest.Attributes;
using Postgrest.Models;
using System;

namespace DelCorp.Models.Supabase
{
    [Table("uni_med_re")]
    public class SupabaseUniMedRe : BaseModel
    {
        [PrimaryKey("id", false)]
        public long Id { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("nombre_uni_med_re")]
        public string NombreUniMedRe { get; set; }

        [Column("abreviatura_uni_med_re")]
        public string AbreviaturaUniMedRe { get; set; }
    }
}