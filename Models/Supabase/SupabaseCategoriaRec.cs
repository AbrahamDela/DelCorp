using Postgrest.Attributes;
using Postgrest.Models;
using System;

namespace DelCorp.Models.Supabase
{
    [Table("categorias_rec")]
    public class SupabaseCategoriaRec : BaseModel
    {
        [PrimaryKey("id", false)]
        public long Id { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("nombre_cat_rec")]
        public string NombreCatRec { get; set; }
    }
}
