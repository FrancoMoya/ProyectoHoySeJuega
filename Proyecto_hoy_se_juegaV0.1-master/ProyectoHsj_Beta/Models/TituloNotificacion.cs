using System;
using System.Collections.Generic;

namespace ProyectoHsj_Beta.Models;

public partial class TituloNotificacion
{
    public int IdTituloNotificacion { get; set; }

    public string TituloNotificacion1 { get; set; } = null!;

    public virtual ICollection<Notificacion> Notificacions { get; set; } = new List<Notificacion>();
}
