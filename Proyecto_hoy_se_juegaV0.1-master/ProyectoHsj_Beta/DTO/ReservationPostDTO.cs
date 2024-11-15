namespace ProyectoHsj_Beta.DTO
{
    public class ReservationPostDTO
    {
        public class ReservaRequest
        {
            public int ID_usuario { get; set; }
            public int ID_horario_disponible { get; set; }
        }

        public class ReservaResponse
        {
            public int ID_reserva { get; set; }
            public int ID_usuario { get; set; }
            public int ID_horario_disponible { get; set; }
            public DateTime Fecha_Reserva { get; set; }
            public int ID_estado_reserva { get; set; }
        }

    }
}
