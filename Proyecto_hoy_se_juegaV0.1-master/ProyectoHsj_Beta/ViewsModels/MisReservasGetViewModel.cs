﻿namespace ProyectoHsj_Beta.ViewsModels
{
    public class MisReservasGetViewModel
    {
        public int IdReserva { get; set; }
        public DateTime FechaReserva { get; set; }
        public string NombreEstadoReserva { get; set; }
        public DateTime FechaHorario { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
        public int IdPago { get; set; }
        public DateTime FechaPago { get; set; }
        public Decimal Monto { get; set; }
    }
}
