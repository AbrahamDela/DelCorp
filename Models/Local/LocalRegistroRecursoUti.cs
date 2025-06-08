using SQLite;

namespace DelCorp.Models.Local;

[Table("RegistroRecursosUti")]
public class LocalRegistroRecursoUti
{
    [PrimaryKey, AutoIncrement]
    public long LocalId { get; set; }
    public long? ServerId { get; set; }
    [Indexed]
    public System.DateTime CreatedAt { get; set; }
    public System.DateTime? FechaRecursoUti { get; set; }
    public decimal? CantidadRecursosUti { get; set; }
    public decimal? PrecioUniRecursosUti { get; set; }
    public decimal? TotalRecursosUti { get; set; }
    public long? IdRecurso { get; set; }
    public long? IdSubEtapa { get; set; }
    public long? IdUniMedida { get; set; }
    public bool IsSynced { get; set; }
}
