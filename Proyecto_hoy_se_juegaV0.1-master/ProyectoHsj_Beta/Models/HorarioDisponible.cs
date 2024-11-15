using System;
using System.Collections.Generic;

namespace ProyectoHsj_Beta.Models;

public partial class HorarioDisponible
{
    public int IdHorarioDisponible { get; set; }

    public int IdCancha { get; set; }

    public DateOnly FechaHorario { get; set; }

    public TimeOnly HoraInicio { get; set; }

    public TimeOnly HoraFin { get; set; }

    public bool? DisponibleHorario { get; set; }

    public virtual ICollection<Evento> Eventos { get; set; } = new List<Evento>();

    public virtual Cancha IdCanchaNavigation { get; set; } = null!;

    public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
}
