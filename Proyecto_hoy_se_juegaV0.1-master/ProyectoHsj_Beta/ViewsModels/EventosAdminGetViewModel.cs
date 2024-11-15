namespace ProyectoHsj_Beta.ViewsModels
{
    public class EventosAdminGetViewModel
    {
        public int IdEvento { get; set; }
        public DateTime FechaEvento { get; set; }
        public string NombreEvento { get; set; }
        public string DescripcionEvento { get; set; }
        public string CorreoClienteEvento { get; set; }
        public int TelefonoClienteEvento { get; set; }
        public DateOnly FechaHorario {  get; set; }
        public TimeOnly HoraInicio { get; set; }
        public TimeOnly HoraFin {  get; set; }
        public string NombreEstadoReserva {  get; set; }
    }
}
