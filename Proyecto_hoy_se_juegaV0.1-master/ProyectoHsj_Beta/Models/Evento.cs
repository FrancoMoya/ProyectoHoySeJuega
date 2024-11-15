using System;
using System.Collections.Generic;

namespace ProyectoHsj_Beta.Models;

public partial class Evento
{
    public int IdEvento { get; set; }

    public string NombreEvento { get; set; } = null!;

    public string? DescripcionEvento { get; set; }

    public string? CorreoClienteEvento { get; set; }

    public int? TelefonoClienteEvento { get; set; }

    public DateTime? FechaEvento { get; set; }

    public int? IdHorarioDisponible { get; set; }

    public int? IdEstadoReserva { get; set; }

    public virtual EstadoReserva? IdEstadoReservaNavigation { get; set; }

    public virtual HorarioDisponible? IdHorarioDisponibleNavigation { get; set; }
}
