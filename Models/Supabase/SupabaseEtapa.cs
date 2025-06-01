using Postgrest.Attributes;
using Postgrest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DelCorp.Models.Supabase;

[Table("etapas")]
public class SupabaseEtapa : BaseModel
{
    [Column("id")]
    public long Id { get; set; }
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
    [Column("numero_etapa")]
    public long NumeroEtapa { get; set; }
    [Column("id_actividad_etapa")]
    public long? IdActividadEtapa { get; set; }
    [Column("cantidad_etapa")]
    public decimal? CantidadEtapa { get; set; }
    [Column("monto_total_etapa")]
    public decimal? MontoTotalEtapa { get; set; }
    [Column("monto_eje_etapa")]
    public decimal? MontoEjeEtapa { get; set; }
    [Column("dias_cal_etapa")]
    public long? DiasCalEtapa { get; set; }
    [Column("dias_eje_etapa")]
    public long? DiasEjeEtapa { get; set; }
    [Column("progreso_etapa")]
    public decimal? ProgresoEtapa { get; set; }
    [Column("id_presupuesto")]
    public long IdPresupuesto { get; set; }
    [Column("id_uni_medida")]
    public long? IdUniMedida { get; set; }
}
