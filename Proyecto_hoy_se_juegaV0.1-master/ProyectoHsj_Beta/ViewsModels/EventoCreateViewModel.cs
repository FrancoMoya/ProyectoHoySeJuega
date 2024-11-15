namespace ProyectoHsj_Beta.ViewsModels
{
    public class EventoCreateViewModel
    {
        public string NombreEvento { get; set; }
        public string DescripcionEvento { get; set; }
        public string? CorreoClienteEvento { get; set; }
        public int? TelefonoClienteEvento { get; set; }
        public int IdHorarioDisponible {  get; set; }
    }
}
