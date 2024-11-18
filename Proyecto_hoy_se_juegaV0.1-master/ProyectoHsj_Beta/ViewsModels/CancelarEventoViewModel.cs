using ProyectoHsj_Beta.Models;

namespace ProyectoHsj_Beta.ViewsModels
{
    public class CancelarEventoViewModel
    {
        public List<EventoRecurrente> EventosRecurrentes { get; set; }
        public int IdEventoRecurrente { get; set; }
        public DateTime FechaCancelacion { get; set; }
    }
}
