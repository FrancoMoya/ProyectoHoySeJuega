namespace ProyectoHsj_Beta.ViewsModels
{
    public class FijosAdminGetViewModel
    {
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string CorreoCliente { get; set; }
        public string TelefonoCliente { get; set; }
        public TimeOnly HoraInicio { get; set; }
        public string DiaSemana {  get; set; }
        public DateOnly FechaInicio { get; set; }
        public DateOnly FechaFin {  get; set; }
    }
}
