using DelCorp.Models;
using DelCorp.Models.Local;
using DelCorp.Models.Supabase;
using System.Collections.Generic;
using System.Linq;

namespace DelCorp.Services.Mapping
{
    public static class RecursosMapper
    {
        // CategoriaRec Mappers
        public static CategoriaRec ToDto(this SupabaseCategoriaRec supabase) => supabase == null ? null : new CategoriaRec { Id = supabase.Id, CreatedAt = supabase.CreatedAt, NombreCatRec = supabase.NombreCatRec };
        public static CategoriaRec ToDto(this LocalCategoriaRec local) => local == null ? null : new CategoriaRec { Id = local.ServerId, CreatedAt = local.CreatedAt, NombreCatRec = local.NombreCatRec };
        public static SupabaseCategoriaRec ToSupabase(this CategoriaRec dto) => dto == null ? null : new SupabaseCategoriaRec { Id = dto.Id, CreatedAt = dto.CreatedAt, NombreCatRec = dto.NombreCatRec };
        public static LocalCategoriaRec ToLocal(this CategoriaRec dto, bool isSynced = true) => dto == null ? null : new LocalCategoriaRec { ServerId = dto.Id, CreatedAt = dto.CreatedAt, NombreCatRec = dto.NombreCatRec, IsSynced = isSynced };
        public static List<CategoriaRec> ToDtoList(this IEnumerable<SupabaseCategoriaRec> supabaseList) => supabaseList.Select(s => s.ToDto()).ToList();
        public static List<CategoriaRec> ToDtoList(this IEnumerable<LocalCategoriaRec> localList) => localList.Select(l => l.ToDto()).ToList();

        // UniMedRe Mappers
        public static UniMedRe ToDto(this SupabaseUniMedRe supabase) => supabase == null ? null : new UniMedRe { Id = supabase.Id, CreatedAt = supabase.CreatedAt, NombreUniMedRe = supabase.NombreUniMedRe, AbreviaturaUniMedRe = supabase.AbreviaturaUniMedRe };
        public static UniMedRe ToDto(this LocalUniMedRe local) => local == null ? null : new UniMedRe { Id = local.ServerId, CreatedAt = local.CreatedAt, NombreUniMedRe = local.NombreUniMedRe, AbreviaturaUniMedRe = local.AbreviaturaUniMedRe };
        public static SupabaseUniMedRe ToSupabase(this UniMedRe dto) => dto == null ? null : new SupabaseUniMedRe { Id = dto.Id, CreatedAt = dto.CreatedAt, NombreUniMedRe = dto.NombreUniMedRe, AbreviaturaUniMedRe = dto.AbreviaturaUniMedRe };
        public static LocalUniMedRe ToLocal(this UniMedRe dto, bool isSynced = true) => dto == null ? null : new LocalUniMedRe { ServerId = dto.Id, CreatedAt = dto.CreatedAt, NombreUniMedRe = dto.NombreUniMedRe, AbreviaturaUniMedRe = dto.AbreviaturaUniMedRe, IsSynced = isSynced };
        public static List<UniMedRe> ToDtoList(this IEnumerable<SupabaseUniMedRe> supabaseList) => supabaseList.Select(s => s.ToDto()).ToList();
        public static List<UniMedRe> ToDtoList(this IEnumerable<LocalUniMedRe> localList) => localList.Select(l => l.ToDto()).ToList();

        // Recurso Mappers
        public static Recurso ToDto(this SupabaseRecurso supabase) => supabase == null ? null : new Recurso { Id = supabase.Id, CreatedAt = supabase.CreatedAt, NombreRecurso = supabase.NombreRecurso, IdCatRec = supabase.IdCatRec };
        public static Recurso ToDto(this LocalRecurso local) => local == null ? null : new Recurso { Id = local.ServerId, CreatedAt = local.CreatedAt, NombreRecurso = local.NombreRecurso, IdCatRec = local.IdCatRec };
        public static SupabaseRecurso ToSupabase(this Recurso dto) => dto == null ? null : new SupabaseRecurso { Id = dto.Id, CreatedAt = dto.CreatedAt, NombreRecurso = dto.NombreRecurso, IdCatRec = dto.IdCatRec };
        public static LocalRecurso ToLocal(this Recurso dto, bool isSynced = true) => dto == null ? null : new LocalRecurso { ServerId = dto.Id, CreatedAt = dto.CreatedAt, NombreRecurso = dto.NombreRecurso, IdCatRec = dto.IdCatRec, IsSynced = isSynced };
        public static List<Recurso> ToDtoList(this IEnumerable<SupabaseRecurso> supabaseList) => supabaseList.Select(s => s.ToDto()).ToList();
        public static List<Recurso> ToDtoList(this IEnumerable<LocalRecurso> localList) => localList.Select(l => l.ToDto()).ToList();

        // RecursoUti Mappers
        public static RecursoUti ToDto(this SupabaseRecursoUti supabase) => supabase == null ? null : new RecursoUti { Id = supabase.Id, CreatedAt = supabase.CreatedAt, CantidadRecursosUti = supabase.CantidadRecursosUti, PrecioUniRecursosUti = supabase.PrecioUniRecursosUti, TotalRecursosUti = supabase.TotalRecursosUti, IdSubEtapa = supabase.IdSubEtapa, IdRecurso = supabase.IdRecurso, IdUniMedRe = supabase.IdUniMedRe };
        public static RecursoUti ToDto(this LocalRecursoUti local) => local == null ? null : new RecursoUti { Id = local.ServerId ?? local.LocalId, CreatedAt = local.CreatedAt, CantidadRecursosUti = local.CantidadRecursosUti, PrecioUniRecursosUti = local.PrecioUniRecursosUti, TotalRecursosUti = local.TotalRecursosUti, IdSubEtapa = local.IdSubEtapa, IdRecurso = local.IdRecurso, IdUniMedRe = local.IdUniMedRe };
        public static SupabaseRecursoUti ToSupabase(this RecursoUti dto) => dto == null ? null : new SupabaseRecursoUti { Id = dto.Id, CreatedAt = dto.CreatedAt, CantidadRecursosUti = dto.CantidadRecursosUti, PrecioUniRecursosUti = dto.PrecioUniRecursosUti, TotalRecursosUti = dto.TotalRecursosUti, IdSubEtapa = dto.IdSubEtapa, IdRecurso = dto.IdRecurso, IdUniMedRe = dto.IdUniMedRe };
        public static LocalRecursoUti ToLocal(this RecursoUti dto, bool isSynced = true) => dto == null ? null : new LocalRecursoUti { ServerId = dto.Id, CreatedAt = dto.CreatedAt, CantidadRecursosUti = dto.CantidadRecursosUti, PrecioUniRecursosUti = dto.PrecioUniRecursosUti, TotalRecursosUti = dto.TotalRecursosUti, IdSubEtapa = dto.IdSubEtapa, IdRecurso = dto.IdRecurso, IdUniMedRe = dto.IdUniMedRe, IsSynced = isSynced };
        public static List<RecursoUti> ToDtoList(this IEnumerable<SupabaseRecursoUti> supabaseList) => supabaseList.Select(s => s.ToDto()).ToList();
        public static List<RecursoUti> ToDtoList(this IEnumerable<LocalRecursoUti> localList) => localList.Select(l => l.ToDto()).ToList();

        // RegistroRecursoUti Mappers
        public static RegistroRecursoUti ToDto(this SupabaseRegistroRecursoUti supabase) => supabase == null ? null : new RegistroRecursoUti
        {
            Id = supabase.Id,
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
            Id = local.ServerId ?? local.LocalId,
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
            Id = dto.Id,
            CreatedAt = dto.CreatedAt,
            FechaRecursoUti = dto.FechaRecursoUti,
            CantidadRecursosUti = dto.CantidadRecursosUti,
            PrecioUniRecursosUti = dto.PrecioUniRecursosUti,
            TotalRecursosUti = dto.TotalRecursosUti,
            IdRecurso = dto.IdRecurso,
            IdSubEtapa = dto.IdSubEtapa,
            IdUniMedida = dto.IdUniMedida
        };

        public static LocalRegistroRecursoUti ToLocal(this RegistroRecursoUti dto, bool isSynced = true) => dto == null ? null : new LocalRegistroRecursoUti
        {
            ServerId = dto.Id,
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