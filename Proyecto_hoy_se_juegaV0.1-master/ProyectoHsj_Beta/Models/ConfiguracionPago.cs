using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProyectoHsj_Beta.Models;

public partial class ConfiguracionPago
{
    public int IdConfiguracion { get; set; }

    [RegularExpression(@"^\d+$", ErrorMessage = "El campo {0} solo debe contener números enteros.")]
    public decimal MontoSena { get; set; }

    public DateTime FechaModificacion { get; set; }

    [Range(100000000, 999999999, ErrorMessage = "El campo {0} debe ser un número de teléfono válido.")]
    public int? CelularCancelaciones { get; set; }
}
