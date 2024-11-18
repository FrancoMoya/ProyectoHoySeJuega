using System;
using System.Collections.Generic;

namespace ProyectoHsj_Beta.Models;

public partial class EventoRecurrente
{
    public int IdEventoRecurrente { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public string? CorreoCliente { get; set; }

    public string? TelefonoCliente { get; set; }

    public TimeOnly HoraInicio { get; set; }

    public int DiaSemana { get; set; }

    public DateOnly FechaInicio { get; set; }

    public DateOnly FechaFin { get; set; }
}
