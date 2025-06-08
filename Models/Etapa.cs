using CommunityToolkit.Mvvm.ComponentModel; // Asegúrate de tener este paquete NuGet
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DelCorp.Models
{
    // Convierte la clase Etapa a partial y hereda de ObservableObject
    public partial class Etapa : ObservableObject
    {
        [ObservableProperty]
        private long _id;

        [ObservableProperty]
        private DateTime _createdAt;

        [ObservableProperty]
        private long _numeroEtapa; // Esta es la propiedad clave para la UI

        [ObservableProperty]
        private long? _idActividadEtapa;

        [ObservableProperty]
        private decimal? _cantidadEtapa;

        [ObservableProperty]
        private decimal? _montoTotalEtapa;

        [ObservableProperty]
        private decimal? _montoEjeEtapa;

        [ObservableProperty]
        private long? _diasCalEtapa;

        [ObservableProperty]
        private long? _diasEjeEtapa;

        [ObservableProperty]
        private decimal? _progresoEtapa;

        [ObservableProperty]
        private long _idPresupuesto;

        [ObservableProperty]
        private Actividad _actividad;

        [ObservableProperty]
        private List<SubEtapa> _subEtapas = new List<SubEtapa>();
    }
}