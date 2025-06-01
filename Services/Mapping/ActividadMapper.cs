using DelCorp.Models;
using DelCorp.Models.Local;
using DelCorp.Models.Supabase;
using System.Collections.Generic;
using System.Linq;

namespace DelCorp.Services.Mapping
{
    public static class ActividadMapper
    {
        // --- CategoriaActividad Mappers ---

        // SupabaseCategoriaActividad a DTO CategoriaActividad
        public static CategoriaActividad ToDto(this SupabaseCategoriaActividad supabase)
        {
            if (supabase == null) return null;
            return new CategoriaActividad
            {
                IdCategoriaActividad = supabase.IdCategoriaActividad,
                CreatedAt = supabase.CreatedAt,
                NombreCategoriaActividad = supabase.NombreCategoriaActividad,
                EsContable = supabase.EsContable
            };
        }

        // LocalCategoriaActividad a DTO CategoriaActividad
        public static CategoriaActividad ToDto(this LocalCategoriaActividad local)
        {
            if (local == null) return null;
            return new CategoriaActividad
            {
                IdCategoriaActividad = local.IdCategoriaActividad,
                CreatedAt = local.CreatedAt,
                NombreCategoriaActividad = local.NombreCategoriaActividad,
                EsContable = local.EsContable
            };
        }

        // DTO CategoriaActividad a SupabaseCategoriaActividad
        public static SupabaseCategoriaActividad ToSupabase(this CategoriaActividad dto)
        {
            if (dto == null) return null;
            return new SupabaseCategoriaActividad
            {
                IdCategoriaActividad = dto.IdCategoriaActividad,
                CreatedAt = dto.CreatedAt,
                NombreCategoriaActividad = dto.NombreCategoriaActividad,
                EsContable = dto.EsContable
            };
        }

        // DTO CategoriaActividad a LocalCategoriaActividad
        public static LocalCategoriaActividad ToLocal(this CategoriaActividad dto, bool isSynced = true)
        {
            if (dto == null) return null;
            return new LocalCategoriaActividad
            {
                IdCategoriaActividad = dto.IdCategoriaActividad,
                CreatedAt = dto.CreatedAt,
                NombreCategoriaActividad = dto.NombreCategoriaActividad,
                EsContable = dto.EsContable,
                IsSynced = isSynced
            };
        }

        public static List<CategoriaActividad> ToDtoList(this IEnumerable<SupabaseCategoriaActividad> supabaseList) =>
            supabaseList?.Select(s => s.ToDto()).ToList() ?? new List<CategoriaActividad>();

        public static List<CategoriaActividad> ToDtoList(this IEnumerable<LocalCategoriaActividad> localList) =>
            localList?.Select(l => l.ToDto()).ToList() ?? new List<CategoriaActividad>();

        // --- Actividad Mappers ---

        // SupabaseActividad a DTO Actividad
        public static Actividad ToDto(this SupabaseActividad supabase)
        {
            if (supabase == null) return null;
            return new Actividad
            {
                IdActividad = supabase.IdActividad,
                CreatedAt = supabase.CreatedAt,
                NombreActividad = supabase.NombreActividad,
                CategoriaActividadId = supabase.CategoriaActividadId
                // CategoriaActividad se poblará por separado si es necesario
            };
        }

        // LocalActividad a DTO Actividad
        public static Actividad ToDto(this LocalActividad local)
        {
            if (local == null) return null;
            return new Actividad
            {
                IdActividad = local.IdActividad,
                CreatedAt = local.CreatedAt,
                NombreActividad = local.NombreActividad,
                CategoriaActividadId = local.CategoriaActividadId
                // CategoriaActividad se poblará por separado
            };
        }

        // DTO Actividad a SupabaseActividad
        public static SupabaseActividad ToSupabase(this Actividad dto)
        {
            if (dto == null) return null;
            return new SupabaseActividad
            {
                IdActividad = (dto.IdActividad == 0 && dto.CreatedAt == default(DateTime)) ? default(long) : dto.IdActividad, // Para inserciones nuevas, el ID lo genera Supabase
                CreatedAt = dto.CreatedAt,
                NombreActividad = dto.NombreActividad,
                CategoriaActividadId = dto.CategoriaActividadId
            };
        }

        // DTO Actividad a SupabaseActividad para Upsert (asegura que Id sea 0 para nuevas)
        public static SupabaseActividad ToSupabaseForUpsert(this Actividad dto)
        {
            if (dto == null) return null;
            return new SupabaseActividad
            {
                // Si IdActividad es 0, significa que es una nueva actividad que el usuario escribió.
                // Supabase se encargará de generar el ID.
                // Si IdActividad no es 0, es una actividad existente.
                IdActividad = dto.IdActividad == 0 ? default : dto.IdActividad,
                // CreatedAt no se envía para inserciones nuevas si la DB lo autogenera y es default.
                // NombreActividad y CategoriaActividadId siempre se envían.
                NombreActividad = dto.NombreActividad,
                CategoriaActividadId = dto.CategoriaActividadId
            };
        }


        // DTO Actividad a LocalActividad
        public static LocalActividad ToLocal(this Actividad dto, bool isSynced = true)
        {
            if (dto == null) return null;
            return new LocalActividad
            {
                IdActividad = dto.IdActividad,
                CreatedAt = dto.CreatedAt,
                NombreActividad = dto.NombreActividad,
                CategoriaActividadId = dto.CategoriaActividadId,
                IsSynced = isSynced
            };
        }

        public static List<Actividad> ToDtoList(this IEnumerable<SupabaseActividad> supabaseList) =>
            supabaseList?.Select(s => s.ToDto()).ToList() ?? new List<Actividad>();

        public static List<Actividad> ToDtoList(this IEnumerable<LocalActividad> localList) =>
            localList?.Select(l => l.ToDto()).ToList() ?? new List<Actividad>();
    }
}