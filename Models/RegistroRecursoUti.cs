namespace DelCorp.Models
{
    public class RegistroRecursoUti
    {
        public long IdRegistroRecursoUti { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? FechaRecursoUti { get; set; }
        public decimal? CantidadRecursosUti { get; set; }
        public decimal? PrecioUniRecursosUti { get; set; }
        public decimal? TotalRecursosUti { get; set; }
        public long? IdRecurso { get; set; }
        public long? IdSubEtapa { get; set; }
        public long? IdUniMedida { get; set; }

        public Recurso? Recurso { get; set; }
        public UniMedRe? UniMedRe { get; set; }
    }
}
