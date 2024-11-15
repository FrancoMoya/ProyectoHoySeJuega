using System;
using System.Collections.Generic;

namespace ProyectoHsj_Beta.Models;

public partial class Notificacion
{
    public int IdNotificacion { get; set; }

    public int IdUsuario { get; set; }

    public int? IdReserva { get; set; }

    public string MensajeNotificacion { get; set; } = null!;

    public DateTime FechaEnvioNotificacion { get; set; }

    public int? IdTituloNotificacion { get; set; }

    public virtual Reserva? IdReservaNavigation { get; set; }

    public virtual TituloNotificacion? IdTituloNotificacionNavigation { get; set; }

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
