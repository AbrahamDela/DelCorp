namespace DelCorp.Models;

public class RegistroRecursoUti
{
    public long Id { get; set; }
    public System.DateTime CreatedAt { get; set; }
    public System.DateTime? FechaRecursoUti { get; set; }
    public decimal? CantidadRecursosUti { get; set; }
    public decimal? PrecioUniRecursosUti { get; set; }
    public decimal? TotalRecursosUti { get; set; }
    public long? IdRecurso { get; set; }
    public long? IdSubEtapa { get; set; }
    public long? IdUniMedida { get; set; }
    public Recurso Recurso { get; set; }
    public SubEtapa SubEtapa { get; set; }
    public UniMedRe UniMedRe { get; set; }
}
