using DelCorp.Models;
using DelCorp.Models.Local;
using DelCorp.Models.Supabase;

namespace DelCorp.Services.Mapping
{
    public static class RegistroRecursoUtiMapper
    {
        public static RegistroRecursoUti ToDto(this SupabaseRegistroRecursoUti s) => s == null ? null : new RegistroRecursoUti
        {
            IdRegistroRecursoUti = s.IdRegistroRecursoUti,
            CreatedAt = s.CreatedAt,
            FechaRecursoUti = s.FechaRecursoUti,
            CantidadRecursosUti = s.CantidadRecursosUti,
            PrecioUniRecursosUti = s.PrecioUniRecursosUti,
            TotalRecursosUti = s.TotalRecursosUti,
            IdRecurso = s.IdRecurso,
            IdSubEtapa = s.IdSubEtapa,
            IdUniMedida = s.IdUniMedida
        };

        public static RegistroRecursoUti ToDto(this LocalRegistroRecursoUti l) => l == null ? null : new RegistroRecursoUti
        {
            IdRegistroRecursoUti = l.ServerId ?? l.LocalId,
            CreatedAt = l.CreatedAt,
            FechaRecursoUti = l.FechaRecursoUti,
            CantidadRecursosUti = l.CantidadRecursosUti,
            PrecioUniRecursosUti = l.PrecioUniRecursosUti,
            TotalRecursosUti = l.TotalRecursosUti,
            IdRecurso = l.IdRecurso,
            IdSubEtapa = l.IdSubEtapa,
            IdUniMedida = l.IdUniMedida
        };

        public static SupabaseRegistroRecursoUti ToSupabase(this RegistroRecursoUti d) => d == null ? null : new SupabaseRegistroRecursoUti
        {
            IdRegistroRecursoUti = d.IdRegistroRecursoUti,
            CreatedAt = d.CreatedAt,
            FechaRecursoUti = d.FechaRecursoUti,
            CantidadRecursosUti = d.CantidadRecursosUti,
            PrecioUniRecursosUti = d.PrecioUniRecursosUti,
            TotalRecursosUti = d.TotalRecursosUti,
            IdRecurso = d.IdRecurso,
            IdSubEtapa = d.IdSubEtapa,
            IdUniMedida = d.IdUniMedida
        };

        public static LocalRegistroRecursoUti ToLocal(this RegistroRecursoUti d, bool isSynced = true) => d == null ? null : new LocalRegistroRecursoUti
        {
            ServerId = d.IdRegistroRecursoUti,
            CreatedAt = d.CreatedAt,
            FechaRecursoUti = d.FechaRecursoUti,
            CantidadRecursosUti = d.CantidadRecursosUti,
            PrecioUniRecursosUti = d.PrecioUniRecursosUti,
            TotalRecursosUti = d.TotalRecursosUti,
            IdRecurso = d.IdRecurso,
            IdSubEtapa = d.IdSubEtapa,
            IdUniMedida = d.IdUniMedida,
            IsSynced = isSynced
        };
    }
}
