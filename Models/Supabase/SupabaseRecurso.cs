using Postgrest.Attributes;
using Postgrest.Models;
using System;

namespace DelCorp.Models.Supabase
{
    [Table("recursos")]
    public class SupabaseRecurso : BaseModel
    {
        [PrimaryKey("id", false)]
        public long Id { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("nombre_recurso")]
        public string NombreRecurso { get; set; }

        [Column("id_cat_rec")]
        public long? IdCatRec { get; set; }
    }
}