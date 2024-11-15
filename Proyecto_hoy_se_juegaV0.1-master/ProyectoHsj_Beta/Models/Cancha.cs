using System;
using System.Collections.Generic;

namespace ProyectoHsj_Beta.Models;

public partial class Cancha
{
    public int IdCancha { get; set; }

    public string NombreCancha { get; set; } = null!;

    public string UbicacionCancha { get; set; } = null!;

    public virtual ICollection<HorarioDisponible> HorarioDisponibles { get; set; } = new List<HorarioDisponible>();
}
