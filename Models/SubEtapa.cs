using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DelCorp.Models;

public partial class SubEtapa : ObservableObject // Modificado para heredar de ObservableObject
{
    [ObservableProperty]
    private long _id;

    [ObservableProperty]
    private DateTime _createdAt;

    [ObservableProperty]
    private long _numeroSubEtapa; // Esta propiedad notificara cambios a la UI

    [ObservableProperty]
    private long? _actividadSubEtapaId;

    [ObservableProperty]
    private decimal? _cantidadSubEtapa;

    [ObservableProperty]
    private decimal? _precioUniSubEtapa;

    [ObservableProperty]
    private decimal? _precioUniEjeSubEtapa;

    [ObservableProperty]
    private decimal? _totalSubEstapa;

    [ObservableProperty]
    private decimal? _montoEjeSubEtapa;

    [ObservableProperty]
    private long? _diasCalSubEtapa;

    [ObservableProperty]
    private long? _diasEjeSubEtapa;

    [ObservableProperty]
    private long _idEtapa;

    [ObservableProperty]
    private Actividad _actividad;
}