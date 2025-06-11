using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace DelCorp.Models
{
    public partial class RegistroRecursoUti : ObservableObject
    {
        [ObservableProperty]
        private long _id;

        [ObservableProperty]
        private DateTime _createdAt;

        [ObservableProperty]
        private DateTime? _fechaRecursoUti;

        [ObservableProperty]
        private decimal? _cantidadRecursosUti;

        [ObservableProperty]
        private decimal? _precioUniRecursosUti;

        [ObservableProperty]
        private decimal? _totalRecursosUti;

        [ObservableProperty]
        private long? _idRecurso;

        [ObservableProperty]
        private long? _idSubEtapa;

        [ObservableProperty]
        private long? _idUniMedida;

        // Navigation properties for display
        [ObservableProperty]
        private Recurso _recurso;

        [ObservableProperty]
        private UniMedRe _uniMedRe;
    }
}
