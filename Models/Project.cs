using System;

namespace DelCorp.Models;

public class Project
{
    public int Id { get; set; }
    public string NombreProyecto { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? FechaInicioProyecto { get; set; }
    public DateTime? FechaFinProyecto { get; set; }
    public string DireccionProyecto { get; set; }
    public string LatitudProyecto { get; set; }
    public string LongitudProyecto { get; set; }
    public string DescripcionProyecto { get; set; }
    public bool IsSynced { get; set; }
}
