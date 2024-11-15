using System;
using System.Collections.Generic;

namespace ProyectoHsj_Beta.Models;

public partial class Pago
{
    public int IdPago { get; set; }

    public int? IdReserva { get; set; }

    public decimal MontoPago { get; set; }

    public DateTime? FechaPago { get; set; }

    public virtual Reserva? IdReservaNavigation { get; set; }
}
