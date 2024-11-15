using System;
using System.Collections.Generic;

namespace ProyectoHsj_Beta.Models;

public partial class Reporte
{
    public int IdReporte { get; set; }

    public byte MesReporte { get; set; }

    public short AnioReporte { get; set; }

    public int? UsuariosRegistradosReporte { get; set; }

    public int? ReservasRealizadasReporte { get; set; }

    public DateTime? FechaDeReporte { get; set; }

    public string DescripcionReporte { get; set; } = null!;

    public int? IdTituloReporte { get; set; }

    public virtual TituloReporte? IdTituloReporteNavigation { get; set; }
}
