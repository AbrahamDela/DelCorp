using Postgrest.Attributes;
using Postgrest.Models;
using System;

namespace DelCorp.Models.Supabase
{
    [Table("actividades")]
    public class SupabaseActividad : BaseModel
    {
        [PrimaryKey("id_actividad", false)]
        public long IdActividad { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("nombre_actividad")]
        public string NombreActividad { get; set; }

        [Column("categoria_actividad_id")]
        public long? CategoriaActividadId { get; set; }

        [Column("unidad_medida_id")]
        public long? UnidadMedidaId { get; set; }
    }
}