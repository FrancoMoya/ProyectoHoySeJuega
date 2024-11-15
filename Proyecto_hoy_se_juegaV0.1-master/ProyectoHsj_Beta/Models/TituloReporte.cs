using System;
using System.Collections.Generic;

namespace ProyectoHsj_Beta.Models;

public partial class TituloReporte
{
    public int IdTituloReporte { get; set; }

    public string TituloReporte1 { get; set; } = null!;

    public virtual ICollection<Reporte> Reportes { get; set; } = new List<Reporte>();
}
