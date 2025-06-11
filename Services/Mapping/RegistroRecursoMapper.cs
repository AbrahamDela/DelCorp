using DelCorp.Models;
using DelCorp.Models.Local;
using DelCorp.Models.Supabase;
using System.Collections.Generic;
using System.Linq;

namespace DelCorp.Services.Mapping
{
    public static class RegistroRecursoMapper
    {
        public static RegistroRecursoUti ToDto(this SupabaseRegistroRecursoUti supabase) => supabase == null ? null : new RegistroRecursoUti
        {
            IdRegistroRecursoUti = supabase.IdRegistroRecursoUti,
            CreatedAt = supabase.CreatedAt,
            FechaRecursoUti = supabase.FechaRecursoUti,
            CantidadRecursosUti = supabase.CantidadRecursosUti,
            PrecioUniRecursosUti = supabase.PrecioUniRecursosUti,
            TotalRecursosUti = supabase.TotalRecursosUti,
            IdRecurso = supabase.IdRecurso,
            IdSubEtapa = supabase.IdSubEtapa,
            IdUniMedida = supabase.IdUniMedida
        };

        public static RegistroRecursoUti ToDto(this LocalRegistroRecursoUti local) => local == null ? null : new RegistroRecursoUti
        {
            IdRegistroRecursoUti = local.ServerId ?? 0,
            CreatedAt = local.CreatedAt,
            FechaRecursoUti = local.FechaRecursoUti,
            CantidadRecursosUti = local.CantidadRecursosUti,
            PrecioUniRecursosUti = local.PrecioUniRecursosUti,
            TotalRecursosUti = local.TotalRecursosUti,
            IdRecurso = local.IdRecurso,
            IdSubEtapa = local.IdSubEtapa,
            IdUniMedida = local.IdUniMedida
        };

        public static SupabaseRegistroRecursoUti ToSupabase(this RegistroRecursoUti dto) => dto == null ? null : new SupabaseRegistroRecursoUti
        {
            IdRegistroRecursoUti = dto.IdRegistroRecursoUti,
            CreatedAt = dto.CreatedAt,
            FechaRecursoUti = dto.FechaRecursoUti,
            CantidadRecursosUti = dto.CantidadRecursosUti,
            PrecioUniRecursosUti = dto.PrecioUniRecursosUti,
            TotalRecursosUti = dto.TotalRecursosUti,
            IdRecurso = dto.IdRecurso,
            IdSubEtapa = dto.IdSubEtapa,
            IdUniMedida = dto.IdUniMedida
        };

        public static LocalRegistroRecursoUti ToLocal(this RegistroRecursoUti dto, bool isSynced = false) => dto == null ? null : new LocalRegistroRecursoUti
        {
            ServerId = dto.IdRegistroRecursoUti,
            CreatedAt = dto.CreatedAt,
            FechaRecursoUti = dto.FechaRecursoUti,
            CantidadRecursosUti = dto.CantidadRecursosUti,
            PrecioUniRecursosUti = dto.PrecioUniRecursosUti,
            TotalRecursosUti = dto.TotalRecursosUti,
            IdRecurso = dto.IdRecurso,
            IdSubEtapa = dto.IdSubEtapa,
            IdUniMedida = dto.IdUniMedida,
            IsSynced = isSynced
        };

        public static List<RegistroRecursoUti> ToDtoList(this IEnumerable<SupabaseRegistroRecursoUti> supabaseList) => supabaseList.Select(s => s.ToDto()).ToList();
        public static List<RegistroRecursoUti> ToDtoList(this IEnumerable<LocalRegistroRecursoUti> localList) => localList.Select(l => l.ToDto()).ToList();
    }
}
