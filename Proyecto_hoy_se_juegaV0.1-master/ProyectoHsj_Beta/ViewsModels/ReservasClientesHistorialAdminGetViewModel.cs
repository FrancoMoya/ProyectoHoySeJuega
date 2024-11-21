namespace ProyectoHsj_Beta.ViewsModels
{
    public class ReservasClientesHistorialAdminGetViewModel
    {
        public int Id { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string NombreUsuario { get; set; }
        public string ApellidoUsuario { get; set; }
        public int Celular { get; set; }
        public DateOnly FechaHorario { get; set; }
        public TimeOnly HoraInicio {  get; set; }
        public TimeOnly HoraFin {  get; set; }
        public string Estado { get; set; }

    }
}
