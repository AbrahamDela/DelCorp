using DelCorp.Models;
using DelCorp.Models.Local;
using DelCorp.Models.Supabase;

namespace DelCorp.Services.Mapping;

public static class SubEtapaMapper
{
    public static LocalSubEtapa ToLocal(this SubEtapa subEtapa, bool isSynced = false)
    {
        if (subEtapa == null)
            throw new ArgumentNullException(nameof(subEtapa));

        return new LocalSubEtapa
        {
            Id = subEtapa.Id,
            CreatedAt = subEtapa.CreatedAt,
            NumeroSubEtapa = subEtapa.NumeroSubEtapa,
            ActividadSubEtapaId = subEtapa.ActividadSubEtapaId,
            CantidadSubEtapa = subEtapa.CantidadSubEtapa,
            PrecioUniSubEtapa = subEtapa.PrecioUniSubEtapa,
            PrecioUniEjeSubEtapa = subEtapa.PrecioUniEjeSubEtapa,
            TotalSubEstapa = subEtapa.TotalSubEstapa,
            MontoEjeSubEtapa = subEtapa.MontoEjeSubEtapa,
            DiasCalSubEtapa = subEtapa.DiasCalSubEtapa,
            DiasEjeSubEtapa = subEtapa.DiasEjeSubEtapa,
            IdEtapa = subEtapa.IdEtapa,
            IsSynced = isSynced
        };
    }

    public static LocalSubEtapa ToLocal(this SubEtapa subEtapaDto)
    {
        if (subEtapaDto == null) return null;
        return new LocalSubEtapa
        {
            // LocalId se generará automáticamente por SQLite si es 0 en una inserción.
            // Si estamos actualizando un LocalSubEtapa existente, su LocalId ya estaría establecido.
            ServerId = subEtapaDto.Id, // El Id del DTO es el ServerId
            CreatedAt = subEtapaDto.CreatedAt,
            NumeroSubEtapa = subEtapaDto.NumeroSubEtapa,
            ActividadSubEtapaId = subEtapaDto.ActividadSubEtapaId,
            CantidadSubEtapa = subEtapaDto.CantidadSubEtapa,
            PrecioUniSubEtapa = subEtapaDto.PrecioUniSubEtapa,
            PrecioUniEjeSubEtapa = subEtapaDto.PrecioUniEjeSubEtapa,
            TotalSubEstapa = subEtapaDto.TotalSubEstapa,
            MontoEjeSubEtapa = subEtapaDto.MontoEjeSubEtapa,
            DiasCalSubEtapa = subEtapaDto.DiasCalSubEtapa,
            DiasEjeSubEtapa = subEtapaDto.DiasEjeSubEtapa,
            IdEtapa = subEtapaDto.IdEtapa,
            IsSynced = (subEtapaDto.Id != 0) // Si el DTO tiene un Id (ServerId), se considera sincronizado al mapear a local desde una fuente autoritativa.
                                             // O maneja IsSynced explícitamente donde se llame a ToLocal.
        };
    }

    // LocalSubEtapa a DTO
    public static SubEtapa ToDto(this LocalSubEtapa local)
    {
        if (local == null) return null;
        return new SubEtapa
        {
            Id = local.ServerId ?? local.Id, // Prioriza ServerId para el DTO. Si es null, usa LocalId (item offline).
                                                  // Es importante ser consistente con qué ID representa el DTO.
                                                  // Para la navegación y FKs, se prefiere el ServerId.
            CreatedAt = local.CreatedAt,
            NumeroSubEtapa = local.NumeroSubEtapa,
            ActividadSubEtapaId = local.ActividadSubEtapaId,
            CantidadSubEtapa = local.CantidadSubEtapa,
            PrecioUniSubEtapa = local.PrecioUniSubEtapa,
            PrecioUniEjeSubEtapa = local.PrecioUniEjeSubEtapa,
            TotalSubEstapa = local.TotalSubEstapa,
            MontoEjeSubEtapa = local.MontoEjeSubEtapa,
            DiasCalSubEtapa = local.DiasCalSubEtapa,
            DiasEjeSubEtapa = local.DiasEjeSubEtapa,
            IdEtapa = local.IdEtapa,
            // IsSynced no suele estar en el DTO a menos que la UI lo necesite.
        };
    }

    // DTO a SupabaseSubEtapa
    public static SupabaseSubEtapa ToSupabase(this SubEtapa subEtapaDto)
    {
        if (subEtapaDto == null) return null;
        return new SupabaseSubEtapa
        {
            Id = (subEtapaDto.Id <= 0) ? default(long) : subEtapaDto.Id,                                                                                   // Lo ideal es que el DTO sepa si su Id es un ServerId o un LocalId.
            CreatedAt = subEtapaDto.CreatedAt,
            NumeroSubEtapa = subEtapaDto.NumeroSubEtapa,
            ActividadSubEtapaId = subEtapaDto.ActividadSubEtapaId,
            CantidadSubEtapa = subEtapaDto.CantidadSubEtapa,
            PrecioUniSubEtapa = subEtapaDto.PrecioUniSubEtapa,
            PrecioUniEjeSubEtapa = subEtapaDto.PrecioUniEjeSubEtapa,
            TotalSubEstapa = subEtapaDto.TotalSubEstapa,
            MontoEjeSubEtapa = subEtapaDto.MontoEjeSubEtapa,
            DiasCalSubEtapa = subEtapaDto.DiasCalSubEtapa,
            DiasEjeSubEtapa = subEtapaDto.DiasEjeSubEtapa,
            IdEtapa = subEtapaDto.IdEtapa
        };
    }

    // SupabaseSubEtapa a DTO
    public static SubEtapa ToDto(this SupabaseSubEtapa supabase)
    {
        if (supabase == null) return null;
        return new SubEtapa
        {
            Id = supabase.Id, // Este es el ServerId
            CreatedAt = supabase.CreatedAt,
            NumeroSubEtapa = supabase.NumeroSubEtapa,
            ActividadSubEtapaId = supabase.ActividadSubEtapaId,
            CantidadSubEtapa = supabase.CantidadSubEtapa,
            PrecioUniSubEtapa = supabase.PrecioUniSubEtapa,
            PrecioUniEjeSubEtapa = supabase.PrecioUniEjeSubEtapa,
            TotalSubEstapa = supabase.TotalSubEstapa,
            MontoEjeSubEtapa = supabase.MontoEjeSubEtapa,
            DiasCalSubEtapa = supabase.DiasCalSubEtapa,
            DiasEjeSubEtapa = supabase.DiasEjeSubEtapa,
            IdEtapa = supabase.IdEtapa
        };
    }

    public static List<SubEtapa> ToDtoList(this IEnumerable<LocalSubEtapa> localSubEtapas)
    {
        return localSubEtapas.Select(local => local.ToDto()).ToList();
    }

    // METODO PARA SUPABASE LISTA
    public static List<SubEtapa> ToDtoList(this IEnumerable<SupabaseSubEtapa> supabaseSubEtapas)
    {
        return supabaseSubEtapas?.Select(supabase => supabase.ToDto()).ToList() ?? new List<SubEtapa>();
    }
}

