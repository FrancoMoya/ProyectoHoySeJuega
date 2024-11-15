using System;
using System.Collections.Generic;

namespace ProyectoHsj_Beta.Models;

public partial class Usuario
{
    public int IdUsuario { get; set; }

    public string NombreUsuario { get; set; } = null!;

    public string ApellidoUsuario { get; set; } = null!;

    public string CorreoUsuario { get; set; } = null!;

    public string ContraseniaUsuario { get; set; } = null!;

    public int TelefonoUsuario { get; set; }

    public bool? Activo { get; set; }

    public string? PasswordResetToken { get; set; }

    public DateTime? PasswordResetTokenExpiry { get; set; }

    public string? EmailConfirmationToken { get; set; }

    public bool? EmailConfirmed { get; set; }

    public DateOnly? UltimaSesion { get; set; }

    public int? IdRol { get; set; }

    public virtual Rol? IdRolNavigation { get; set; }
    public DateOnly? FechaRegistro { get; set; }

    public virtual ICollection<Notificacion> Notificacions { get; set; } = new List<Notificacion>();

    public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
}
