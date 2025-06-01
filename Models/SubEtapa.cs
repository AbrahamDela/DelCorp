using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DelCorp.Models;

public class SubEtapa
{
    public long Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public long NumeroSubEtapa { get; set; }
    public long? ActividadSubEtapaId { get; set; } // FK a la tabla actividades
    public decimal? CantidadSubEtapa { get; set; }
    public decimal? PrecioUniSubEtapa { get; set; }
    public decimal? PrecioUniEjeSubEtapa { get; set; }
    public decimal? TotalSubEstapa { get; set; }
    public decimal? MontoEjeSubEtapa { get; set; }
    public long? DiasCalSubEtapa { get; set; }
    public long? DiasEjeSubEtapa { get; set; }
    public long IdEtapa { get; set; }

    public Actividad Actividad { get; set; }
}
