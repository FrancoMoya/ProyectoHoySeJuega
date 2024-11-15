using System;
using System.Collections.Generic;

namespace ProyectoHsj_Beta.Models;

public partial class Reserva
{
    public int IdReserva { get; set; }

    public int? IdUsuario { get; set; }

    public int? IdHorarioDisponible { get; set; }

    public DateTime? FechaReserva { get; set; }

    public int? IdEstadoReserva { get; set; }

    public virtual EstadoReserva? IdEstadoReservaNavigation { get; set; }

    public virtual HorarioDisponible? IdHorarioDisponibleNavigation { get; set; }

    public virtual Usuario? IdUsuarioNavigation { get; set; }

    public virtual ICollection<Notificacion> Notificacions { get; set; } = new List<Notificacion>();

    public virtual ICollection<Pago> Pagos { get; set; } = new List<Pago>();
}
