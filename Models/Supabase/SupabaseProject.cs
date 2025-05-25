using Postgrest.Attributes;
using Postgrest.Models;

namespace DelCorp.Models.Supabase;

[Table("proyectos")]
public class SupabaseProject : BaseModel
{
    [PrimaryKey("id", false)]
    public int Id { get; set; }

    [Column("nombre_proyecto")]
    public string NombreProyecto { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("fecha_inicio_proyecto")]
    public DateTime? FechaInicioProyecto { get; set; }

    [Column("fecha_fin_proyecto")]
    public DateTime? FechaFinProyecto { get; set; }

    [Column("direccion_proyecto")]
    public string DireccionProyecto { get; set; }

    [Column("latitud_proyecto")]
    public string LatitudProyecto { get; set; }

    [Column("longitud_proyecto")]
    public string LongitudProyecto { get; set; }

    [Column("descripcion_proyecto")]
    public string DescripcionProyecto { get; set; }
}
