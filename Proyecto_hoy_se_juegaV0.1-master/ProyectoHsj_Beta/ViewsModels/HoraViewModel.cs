namespace ProyectoHsj_Beta.ViewsModels
{
    public class HoraViewModel
    {
        public int IdCancha { get; set; }
        public DateTime FechaHorario { get; set; }
        public TimeOnly HoraInicio { get; set; }
        public TimeOnly HoraFin { get; set; }
        public bool DisponibleHorario { get; set; }
    }
}
