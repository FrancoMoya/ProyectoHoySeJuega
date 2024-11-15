namespace ProyectoHsj_Beta.DTO
{
    public class ReservaRequest
    {
        public int IdUsuario { get; set; } // ID del usuario que hace la reserva
        public int IdHorarioDisponible { get; set; } // ID del horario a reservar
    }
    public class ReservaRequestID
    {
        public int IdHorarioDisponible { get; set; } // ID del horario a reservar
    }
}
