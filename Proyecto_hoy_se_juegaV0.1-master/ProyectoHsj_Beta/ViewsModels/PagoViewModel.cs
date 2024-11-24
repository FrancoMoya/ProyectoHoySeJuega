namespace ProyectoHsj_Beta.ViewsModels
{
    public class PagoViewModel
    {
        public int ReservaId { get; set; }
        public decimal Monto { get; set; }
        public DateOnly FechaReserva { get; set; }
        public TimeOnly HoraInicio { get; set; }
        public TimeOnly HoraFin {  get; set; }
    }
}
