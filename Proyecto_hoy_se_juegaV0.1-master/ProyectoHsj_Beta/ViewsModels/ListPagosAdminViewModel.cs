namespace ProyectoHsj_Beta.ViewsModels
{
    public class ListPagosAdminViewModel
    {
		public int Id { get; set; }
		public DateTime FechaPago { get; set; }
		public decimal Monto { get; set; }
		public string NombreUsuario { get; set; }
        public string ApellidoUsuario { get; set; }
        public DateTime FechaCreacionReserva {  get; set; }
        public int IdReserva { get; set; }
        public DateOnly FechaHorario { get; set; }
        public TimeOnly HoraInicio { get; set; }
        public TimeOnly HoraFin {  get; set; }
        public string Estado {  get; set; }
        
    }
}
