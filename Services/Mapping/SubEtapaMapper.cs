using DelCorp.Models;
using DelCorp.Models.Local;
using DelCorp.Models.Supabase;

namespace DelCorp.Services.Mapping;

public static class SubEtapaMapper
{
    public static LocalSubEtapa ToLocal(this SubEtapa subEtapa)
    {
        return new LocalSubEtapa
        {
            Id = subEtapa.Id,
            CreatedAt = subEtapa.CreatedAt,
            NumeroSubEtapa = subEtapa.NumeroSubEtapa,
            ActividadSubEtapa = subEtapa.ActividadSubEtapa,
            CantidadSubEtapa = subEtapa.CantidadSubEtapa,
            PrecioUniSubEtapa = subEtapa.PrecioUniSubEtapa,
            PrecioUniEjeSubEtapa = subEtapa.PrecioUniEjeSubEtapa,
            TotalSubEstapa = subEtapa.TotalSubEstapa,
            MontoEjeSubEtapa = subEtapa.MontoEjeSubEtapa,
            DiasCalSubEtapa = subEtapa.DiasCalSubEtapa,
            DiasEjeSubEtapa = subEtapa.DiasEjeSubEtapa,
            IdEtapa = subEtapa.IdEtapa,
            IdUniMedida = subEtapa.IdUniMedida
        };
    }

    public static SubEtapa ToDto(this LocalSubEtapa local)
    {
        return new SubEtapa
        {
            Id = local.Id,
            CreatedAt = local.CreatedAt,
            NumeroSubEtapa = local.NumeroSubEtapa,
            ActividadSubEtapa = local.ActividadSubEtapa,
            CantidadSubEtapa = local.CantidadSubEtapa,
            PrecioUniSubEtapa = local.PrecioUniSubEtapa,
            PrecioUniEjeSubEtapa = local.PrecioUniEjeSubEtapa,
            TotalSubEstapa = local.TotalSubEstapa,
            MontoEjeSubEtapa = local.MontoEjeSubEtapa,
            DiasCalSubEtapa = local.DiasCalSubEtapa,
            DiasEjeSubEtapa = local.DiasEjeSubEtapa,
            IdEtapa = local.IdEtapa,
            IdUniMedida = local.IdUniMedida
        };
    }

    public static SupabaseSubEtapa ToSupabase(this SubEtapa subEtapa)
    {
        return new SupabaseSubEtapa
        {
            Id = subEtapa.Id,
            CreatedAt = subEtapa.CreatedAt,
            NumeroSubEtapa = subEtapa.NumeroSubEtapa,
            ActividadSubEtapa = subEtapa.ActividadSubEtapa,
            CantidadSubEtapa = subEtapa.CantidadSubEtapa,
            PrecioUniSubEtapa = subEtapa.PrecioUniSubEtapa,
            PrecioUniEjeSubEtapa = subEtapa.PrecioUniEjeSubEtapa,
            TotalSubEstapa = subEtapa.TotalSubEstapa,
            MontoEjeSubEtapa = subEtapa.MontoEjeSubEtapa,
            DiasCalSubEtapa = subEtapa.DiasCalSubEtapa,
            DiasEjeSubEtapa = subEtapa.DiasEjeSubEtapa,
            IdEtapa = subEtapa.IdEtapa,
            IdUniMedida = subEtapa.IdUniMedida
        };
    }

    public static SubEtapa ToDto(this SupabaseSubEtapa supabase)
    {
        return new SubEtapa
        {
            Id = supabase.Id,
            CreatedAt = supabase.CreatedAt,
            NumeroSubEtapa = supabase.NumeroSubEtapa,
            ActividadSubEtapa = supabase.ActividadSubEtapa,
            CantidadSubEtapa = supabase.CantidadSubEtapa,
            PrecioUniSubEtapa = supabase.PrecioUniSubEtapa,
            PrecioUniEjeSubEtapa = supabase.PrecioUniEjeSubEtapa,
            TotalSubEstapa = supabase.TotalSubEstapa,
            MontoEjeSubEtapa = supabase.MontoEjeSubEtapa,
            DiasCalSubEtapa = supabase.DiasCalSubEtapa,
            DiasEjeSubEtapa = supabase.DiasEjeSubEtapa,
            IdEtapa = supabase.IdEtapa,
            IdUniMedida = supabase.IdUniMedida
        };
    }

    public static List<SubEtapa> ToDtoList(this IEnumerable<LocalSubEtapa> localSubEtapas)
    {
        return localSubEtapas.Select(local => new SubEtapa
        {
            Id = local.Id,
            CreatedAt = local.CreatedAt,
            NumeroSubEtapa = local.NumeroSubEtapa,
            ActividadSubEtapa = local.ActividadSubEtapa,
            CantidadSubEtapa = local.CantidadSubEtapa,
            PrecioUniSubEtapa = local.PrecioUniSubEtapa,
            PrecioUniEjeSubEtapa = local.PrecioUniEjeSubEtapa,
            TotalSubEstapa = local.TotalSubEstapa,
            MontoEjeSubEtapa = local.MontoEjeSubEtapa,
            DiasCalSubEtapa = local.DiasCalSubEtapa,
            DiasEjeSubEtapa = local.DiasEjeSubEtapa,
            IdEtapa = local.IdEtapa,
            IdUniMedida = local.IdUniMedida
        }).ToList();
    }
}

