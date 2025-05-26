using Postgrest.Attributes;
using Postgrest.Models;
using System;

namespace DelCorp.Models.Supabase
{
    [Table("recursos_uti")]
    public class SupabaseRecursoUti : BaseModel
    {
        [PrimaryKey("id", false)]
        public long Id { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("cantidad_recursos_uti")]
        public decimal? CantidadRecursosUti { get; set; }

        [Column("precio_uni_recursos_uti")]
        public decimal? PrecioUniRecursosUti { get; set; }

        [Column("total_recursos_uti")]
        public decimal? TotalRecursosUti { get; set; }

        [Column("id_sub_etapa")]
        public long? IdSubEtapa { get; set; }

        [Column("id_recurso")]
        public long? IdRecurso { get; set; }

        [Column("id_uni_med_re")]
        public long? IdUniMedRe { get; set; }
    }
}