using System;
using System.Collections.Generic;

namespace ProyectoHsj_Beta.Models;

public partial class AccionRealizadum
{
    public int IdAccionRealizada { get; set; }

    public string TituloAccionRealizada { get; set; } = null!;

    public virtual ICollection<Auditorium> Auditoria { get; set; } = new List<Auditorium>();
}
