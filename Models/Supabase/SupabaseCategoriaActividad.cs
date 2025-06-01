using Postgrest.Attributes;
using Postgrest.Models;
using System;

namespace DelCorp.Models.Supabase
{
    [Table("categorias_actividad")]
    public class SupabaseCategoriaActividad : BaseModel
    {
        [PrimaryKey("id_categoria_actividad", false)] // false si el ID no es autoincremental en el cliente
        public long IdCategoriaActividad { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("nombre_categoria_actividad")]
        public string NombreCategoriaActividad { get; set; }

        [Column("es_contable")]
        public bool EsContable { get; set; }
    }
}
