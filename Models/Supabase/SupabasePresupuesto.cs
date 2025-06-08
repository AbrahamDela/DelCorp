using Postgrest.Attributes;
using Postgrest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DelCorp.Models.Supabase;

[Table("presupuestos")]
public class SupabasePresupuesto : BaseModel
{
    [Column("id")]
    public long Id { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("nombre_presupuesto")]
    public string NombrePresupuesto { get; set; }

    [Column("total_presupuesto")]
    public decimal? TotalPresupuesto { get; set; }

    [Column("monto_eje_presupuesto")]
    public decimal? MontoEjePresupuesto { get; set; }

    [Column("fecha_inicio_presupuesto")]
    public DateTime? FechaInicioPresupuesto { get; set; }

    [Column("fecha_fin_presupuesto")]
    public DateTime? FechaFinPresupuesto { get; set; }

    [Column("dias_cal_presupuesto")]
    public long? DiasCalePresupuesto { get; set; }

    [Column("dias_eje_presupuesto")]
    public long? DiasEjePresupuesto { get; set; }

    [Column("progreso_presupuesto")]
    public decimal? ProgresoPresupuesto { get; set; }

    [Column("id_proyecto")]
    public long IdProyecto { get; set; }

    // Constructor vacío requerido por Postgrest
    public SupabasePresupuesto() { }
}
