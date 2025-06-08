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

        public static Presupuesto ToDto(this SupabasePresupuesto supabasePresupuesto)
        {
            return new Presupuesto
            {
                Id = supabasePresupuesto.Id,
                CreatedAt = supabasePresupuesto.CreatedAt,
                NombrePresupuesto = supabasePresupuesto.NombrePresupuesto,
                TotalPresupuesto = supabasePresupuesto.TotalPresupuesto,
                MontoEjePresupuesto = supabasePresupuesto.MontoEjePresupuesto,
                FechaInicioPresupuesto = supabasePresupuesto.FechaInicioPresupuesto,
                FechaFinPresupuesto = supabasePresupuesto.FechaFinPresupuesto,
                DiasCalePresupuesto = supabasePresupuesto.DiasCalePresupuesto,
                DiasEjePresupuesto = supabasePresupuesto.DiasEjePresupuesto,
                ProgresoPresupuesto = supabasePresupuesto.ProgresoPresupuesto,
                IdProyecto = supabasePresupuesto.IdProyecto
            };
        }

        public static SupabasePresupuesto ToSupabase(this Presupuesto presupuesto)
        {
            return new SupabasePresupuesto
            {
                Id = presupuesto.Id,
                CreatedAt = presupuesto.CreatedAt,
                NombrePresupuesto = presupuesto.NombrePresupuesto,
                TotalPresupuesto = presupuesto.TotalPresupuesto,
                MontoEjePresupuesto = presupuesto.MontoEjePresupuesto,
                FechaInicioPresupuesto = presupuesto.FechaInicioPresupuesto,
                FechaFinPresupuesto = presupuesto.FechaFinPresupuesto,
                DiasCalePresupuesto = presupuesto.DiasCalePresupuesto,
                DiasEjePresupuesto = presupuesto.DiasEjePresupuesto,
                ProgresoPresupuesto = presupuesto.ProgresoPresupuesto,
                IdProyecto = presupuesto.IdProyecto
            };
        }

        public static Presupuesto ToDto(this LocalPresupuesto localPresupuesto)
        {
            return new Presupuesto
            {
                Id = localPresupuesto.ServerId ?? localPresupuesto.Id,
                CreatedAt = localPresupuesto.CreatedAt,
                NombrePresupuesto = localPresupuesto.NombrePresupuesto,
                TotalPresupuesto = localPresupuesto.TotalPresupuesto,
                MontoEjePresupuesto = localPresupuesto.MontoEjePresupuesto,
                FechaInicioPresupuesto = localPresupuesto.FechaInicioPresupuesto,
                FechaFinPresupuesto = localPresupuesto.FechaFinPresupuesto,
                DiasCalePresupuesto = localPresupuesto.DiasCalePresupuesto,
                DiasEjePresupuesto = localPresupuesto.DiasEjePresupuesto,
                ProgresoPresupuesto = localPresupuesto.ProgresoPresupuesto,
                IdProyecto = localPresupuesto.IdProyecto
            };
        }

        public static LocalPresupuesto ToLocal(this Presupuesto presupuesto)
        {
            return new LocalPresupuesto
            {
                ServerId = presupuesto.Id,
                CreatedAt = presupuesto.CreatedAt,
                NombrePresupuesto = presupuesto.NombrePresupuesto,
                TotalPresupuesto = presupuesto.TotalPresupuesto,
                MontoEjePresupuesto = presupuesto.MontoEjePresupuesto,
                FechaInicioPresupuesto = presupuesto.FechaInicioPresupuesto,
                FechaFinPresupuesto = presupuesto.FechaFinPresupuesto,
                DiasCalePresupuesto = presupuesto.DiasCalePresupuesto,
                DiasEjePresupuesto = presupuesto.DiasEjePresupuesto,
                ProgresoPresupuesto = presupuesto.ProgresoPresupuesto,
                IdProyecto = (int)presupuesto.IdProyecto,
                IsSynced = true
            };
        }

        // Metodo para convertir lista de LocalPresupuesto a lista de Presupuesto
        public static List<Presupuesto> ToDtoList(this IEnumerable<LocalPresupuesto> localPresupuestos)
        {
            return localPresupuestos?
                .Select(localPresupuesto => localPresupuesto.ToDto())
                .ToList() ?? new List<Presupuesto>();
        }

        // Sobrecarga para SupabasePresupuesto
        public static List<Presupuesto> ToDtoList(this IEnumerable<SupabasePresupuesto> supabasePresupuestos)
        {
            return supabasePresupuestos?
                .Select(supabasePresupuesto => supabasePresupuesto.ToDto())
                .ToList() ?? new List<Presupuesto>();
        }

        //Para etapas
        public static Etapa ToDto(this LocalEtapa local)
        {
            if (local == null) return null;

            return new Etapa
            {
                Id = local.ServerId ?? local.Id,
                NumeroEtapa = local.NumeroEtapa,
                IdActividadEtapa = local.IdActividadEtapa,
                CantidadEtapa = local.CantidadEtapa,
                MontoTotalEtapa = local.MontoTotalEtapa,
                MontoEjeEtapa = local.MontoEjeEtapa,
                DiasCalEtapa = local.DiasCalEtapa,
                DiasEjeEtapa = local.DiasEjeEtapa,
                ProgresoEtapa = local.ProgresoEtapa,
                IdPresupuesto = local.IdPresupuesto,
                CreatedAt = local.CreatedAt
            };
        }

        public static LocalEtapa ToLocal(this Etapa dto)
        {
            if (dto == null) return null;

            return new LocalEtapa
            {
                ServerId = dto.Id > 0 ? dto.Id : null,
                NumeroEtapa = dto.NumeroEtapa,
                IdActividadEtapa = dto.IdActividadEtapa,
                CantidadEtapa = dto.CantidadEtapa,
                MontoTotalEtapa = dto.MontoTotalEtapa,
                MontoEjeEtapa = dto.MontoEjeEtapa,
                DiasCalEtapa = dto.DiasCalEtapa,
                DiasEjeEtapa = dto.DiasEjeEtapa,
                ProgresoEtapa = dto.ProgresoEtapa,
                IdPresupuesto = dto.IdPresupuesto,
                CreatedAt = dto.CreatedAt != default(DateTime) ? dto.CreatedAt : DateTime.Now,
                IsSynced = true
            };
        }

        public static SupabaseEtapa ToSupabase(this Etapa dto)
        {
            if (dto == null) return null;

            return new SupabaseEtapa
            {
                Id = dto.Id,
                NumeroEtapa = dto.NumeroEtapa,
                IdActividadEtapa = dto.IdActividadEtapa,
                CantidadEtapa = dto.CantidadEtapa,
                MontoTotalEtapa = dto.MontoTotalEtapa,
                MontoEjeEtapa = dto.MontoEjeEtapa,
                DiasCalEtapa = dto.DiasCalEtapa,
                DiasEjeEtapa = dto.DiasEjeEtapa,
                ProgresoEtapa = dto.ProgresoEtapa,
                IdPresupuesto = dto.IdPresupuesto,
                //CreatedAt = dto.CreatedAt ?? DateTime.Now
            };
        }

        public static Etapa ToDto(this SupabaseEtapa supabase)
        {
            if (supabase == null) return null;

            return new Etapa
            {
                Id = supabase.Id,
                NumeroEtapa = supabase.NumeroEtapa,
                IdActividadEtapa = supabase.IdActividadEtapa,
                CantidadEtapa = supabase.CantidadEtapa,
                MontoTotalEtapa = supabase.MontoTotalEtapa,
                MontoEjeEtapa = supabase.MontoEjeEtapa,
                DiasCalEtapa = supabase.DiasCalEtapa,
                DiasEjeEtapa = supabase.DiasEjeEtapa,
                ProgresoEtapa = supabase.ProgresoEtapa,
                IdPresupuesto = supabase.IdPresupuesto,
                CreatedAt = supabase.CreatedAt
            };
        }

        // Método para convertir lista de LocalEtapa a lista de Etapa
        public static List<Etapa> ToDtoList(this IEnumerable<LocalEtapa> localEtapas)
        {
            return localEtapas?
                .Select(localEtapa => localEtapa.ToDto())
                .Where(etapa => etapa != null)
                //.GroupBy(etapa => etapa.Id)  // Eliminar duplicados por ID
                //.Select(g => g.First())
                .ToList() ?? new List<Etapa>();
        }

        // Método para convertir lista de SupabaseEtapa a lista de Etapa
        public static List<Etapa> ToDtoList(this IEnumerable<SupabaseEtapa> supabaseEtapas)
        {
            return supabaseEtapas?
                .Select(supabaseEtapa => supabaseEtapa.ToDto())
                .Where(etapa => etapa != null)
                //.GroupBy(etapa => etapa.Id)  // Eliminar duplicados por ID
                //.Select(g => g.First())
                .ToList() ?? new List<Etapa>();
        }
    }
}