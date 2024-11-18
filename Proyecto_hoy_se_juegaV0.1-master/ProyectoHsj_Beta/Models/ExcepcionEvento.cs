using System;
using System.Collections.Generic;

namespace ProyectoHsj_Beta.Models;

public partial class ExcepcionEvento
{
    public int IdExcepcionEvento { get; set; }

    public int IdEventoRecurrente { get; set; }

    public DateOnly FechaCancelacion { get; set; }

    public int IdEstadoReserva { get; set; }

    public virtual EstadoReserva IdEstadoReservaNavigation { get; set; } = null!;

    public virtual EventoRecurrente IdEventoRecurrenteNavigation { get; set; } = null!;
}
