using DelCorp.Models;
using DelCorp.Models.Local;
using DelCorp.Models.Supabase;
using System.Collections.Generic;

namespace DelCorp.Services.Mapping
{
    public static class ModelMapper
    {
        // Local a DTO
        public static Project ToDto(this LocalProject local)
        {
            return new Project
            {
                Id = local.ServerId ?? local.Id,  // Usamos ServerId si existe
                NombreProyecto = local.NombreProyecto,
                CreatedAt = local.CreatedAt,
                FechaInicioProyecto = local.FechaInicioProyecto,
                FechaFinProyecto = local.FechaFinProyecto,
                DireccionProyecto = local.DireccionProyecto,
                LatitudProyecto = local.LatitudProyecto,
                LongitudProyecto = local.LongitudProyecto,
                DescripcionProyecto = local.DescripcionProyecto,
                IsSynced = local.IsSynced
            };
        }

        // DTO a Local
        public static LocalProject ToLocal(this Project dto)
        {
            return new LocalProject
            {
                ServerId = dto.Id, // Guardar ID del servidor
                NombreProyecto = dto.NombreProyecto,
                CreatedAt = dto.CreatedAt,
                FechaInicioProyecto = dto.FechaInicioProyecto,
                FechaFinProyecto = dto.FechaFinProyecto,
                DireccionProyecto = dto.DireccionProyecto,
                LatitudProyecto = dto.LatitudProyecto,
                LongitudProyecto = dto.LongitudProyecto,
                DescripcionProyecto = dto.DescripcionProyecto,
                IsSynced = dto.IsSynced
            };
        }

        // DTO a Supabase
        public static SupabaseProject ToSupabase(this Project dto)
        {
            return new SupabaseProject
            {
                Id = dto.Id,
                NombreProyecto = dto.NombreProyecto,
                CreatedAt = dto.CreatedAt,
                FechaInicioProyecto = dto.FechaInicioProyecto,
                FechaFinProyecto = dto.FechaFinProyecto,
                DireccionProyecto = dto.DireccionProyecto,
                LatitudProyecto = dto.LatitudProyecto,
                LongitudProyecto = dto.LongitudProyecto,
                DescripcionProyecto = dto.DescripcionProyecto
            };
        }

        // Supabase a DTO
        public static Project ToDto(this SupabaseProject supabase)
        {
            return new Project
            {
                Id = supabase.Id,
                NombreProyecto = supabase.NombreProyecto,
                CreatedAt = supabase.CreatedAt,
                FechaInicioProyecto = supabase.FechaInicioProyecto,
                FechaFinProyecto = supabase.FechaFinProyecto,
                DireccionProyecto = supabase.DireccionProyecto,
                LatitudProyecto = supabase.LatitudProyecto,
                LongitudProyecto = supabase.LongitudProyecto,
                DescripcionProyecto = supabase.DescripcionProyecto,
                IsSynced = true // Asumimos que los datos del servidor están sincronizados
            };
        }

        // Colecciones
        public static List<Project> ToDtoList(this IEnumerable<LocalProject> locals)
        {
            var result = new List<Project>();
            foreach (var local in locals)
            {
                result.Add(local.ToDto());
            }
            return result;
        }

        public static List<Project> ToDtoList(this IEnumerable<SupabaseProject> supabaseProjects)
        {
            var result = new List<Project>();
            foreach (var supabase in supabaseProjects)
            {
                result.Add(supabase.ToDto());
            }
            return result;
        }
    }
}