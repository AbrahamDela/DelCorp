using SQLite;

namespace DelCorp.Models.Local;

[Table("SubEtapas")]
public class LocalSubEtapa
{
    [PrimaryKey, AutoIncrement]
    public long Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public long NumeroSubEtapa { get; set; }
    public string ActividadSubEtapa { get; set; }
    public decimal? CantidadSubEtapa { get; set; }
    public decimal? PrecioUniSubEtapa { get; set; }
    public decimal? PrecioUniEjeSubEtapa { get; set; }
    public decimal? TotalSubEstapa { get; set; }
    public decimal? MontoEjeSubEtapa { get; set; }
    public long? DiasCalSubEtapa { get; set; }
    public long? DiasEjeSubEtapa { get; set; }
    public long IdEtapa { get; set; }
    public long? IdUniMedida { get; set; }
    public bool IsSynced { get; internal set; }
}
