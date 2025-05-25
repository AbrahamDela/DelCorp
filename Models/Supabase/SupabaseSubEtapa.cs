using Postgrest.Models;
using Postgrest.Attributes;

namespace DelCorp.Models.Supabase;

[Table("subetapas")]
public class SupabaseSubEtapa : BaseModel
{
    [PrimaryKey("id", false)]
    public long Id { get; set; }
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
    [Column("numero_sub_etapa")]
    public long NumeroSubEtapa { get; set; }
    [Column("actividad_sub_etapa")]
    public string ActividadSubEtapa { get; set; }
    [Column("cantidad_sub_etapa")]
    public decimal? CantidadSubEtapa { get; set; }
    [Column("precio_uni_sub_etapa")]
    public decimal? PrecioUniSubEtapa { get; set; }
    [Column("precio_uni_eje_sub_etapa")]
    public decimal? PrecioUniEjeSubEtapa { get; set; }
    [Column("total_sub_estapa")]
    public decimal? TotalSubEstapa { get; set; }
    [Column("monto_eje_sub_etapa")]
    public decimal? MontoEjeSubEtapa { get; set; }
    [Column("dias_cal_sub_etapa")]
    public long? DiasCalSubEtapa { get; set; }
    [Column("dias_eje_sub_etapa")]
    public long? DiasEjeSubEtapa { get; set; }
    [Column("id_etapa")]
    public long IdEtapa { get; set; }
    [Column("id_uni_medida")]
    public long? IdUniMedida { get; set; }
}
