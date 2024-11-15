namespace ProyectoHsj_Beta.Models
{
    public class HistorialReserva
    {
        public int IdHistorialReserva { get; set; }
        public int IdReserva { get; set; }
        public int ID_usuario { get; set; }
        public DateTime FechaReserva { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
        public string EstadoReserva { get; set; }
        public DateTime FechaEliminacion { get; set; }
        public string NombreUsuario { get; set; }
        public string TelefonoUsuario { get; set; }


    }
}
