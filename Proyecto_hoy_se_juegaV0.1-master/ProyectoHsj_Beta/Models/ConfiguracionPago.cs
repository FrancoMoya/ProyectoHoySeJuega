using System;
using System.Collections.Generic;

namespace ProyectoHsj_Beta.Models;

public partial class ConfiguracionPago
{
    public int IdConfiguracion { get; set; }

    public decimal MontoSena { get; set; }

    public DateTime FechaModificacion { get; set; }

    public int? CelularCancelaciones { get; set; }
}
