using Postgrest.Attributes;
using Postgrest.Models;
using System;

namespace DelCorp.Models.Supabase
{
    [Table("registro_recursos_uti")]
    public class SupabaseRegistroRecursoUti : BaseModel
    {
        [PrimaryKey("id_registro_recurso_uti", false)]
        public long IdRegistroRecursoUti { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("fecha_recurso_uti")]
        public DateTime? FechaRecursoUti { get; set; }

        [Column("cantidad_recursos_uti")]
        public decimal? CantidadRecursosUti { get; set; }

        [Column("precio_uni_recursos_uti")]
        public decimal? PrecioUniRecursosUti { get; set; }

        [Column("total_recursos_uti")]
        public decimal? TotalRecursosUti { get; set; }

        [Column("id_recurso")]
        public long? IdRecurso { get; set; }

        [Column("id_sub_etapa")]
        public long? IdSubEtapa { get; set; }

        [Column("id_uni_medida")]
        public long? IdUniMedida { get; set; }
    }
}
