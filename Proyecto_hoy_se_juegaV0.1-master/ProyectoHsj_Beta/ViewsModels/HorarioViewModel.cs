namespace ProyectoHsj_Beta.ViewsModels
{
    public class HorarioViewModel
    {
        public int IdHorarioDisponible { get; set; }

        public DateOnly FechaHorario { get; set; }

        public TimeOnly HoraInicio { get; set; }

        public TimeOnly HoraFin { get; set; }

        public bool DisponibleHorario { get; set; } //No nullable
    }
}
