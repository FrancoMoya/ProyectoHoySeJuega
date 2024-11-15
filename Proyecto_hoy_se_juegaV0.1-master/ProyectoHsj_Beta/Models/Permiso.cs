using System;
using System.Collections.Generic;

namespace ProyectoHsj_Beta.Models;

public partial class Permiso
{
    public int IdPermiso { get; set; }

    public string NombrePermiso { get; set; } = null!;

    public virtual ICollection<Rol> IdRols { get; set; } = new List<Rol>();
}
