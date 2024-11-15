using Microsoft.AspNetCore.Mvc.Rendering;

namespace ProyectoHsj_Beta.ViewsModels
{
    public class RolCreateViewModel
    {
        public string NombreRol { get; set; }

        // Lista de permisos seleccionados por el usuario
        public List<int> PermisosSeleccionados { get; set; } = new List<int>();

        // Lista de permisos disponibles para seleccionar
        public List<SelectListItem> PermisosDisponibles { get; set; } = new List<SelectListItem>();
    }


}
