using Microsoft.AspNetCore.Mvc.Rendering;

namespace ProyectoHsj_Beta.ViewsModels
{
    public class OtorgarPViewModel
    {
        public int IdRolSeleccionado { get; set; }
        public List<SelectListItem> Roles { get; set; }
        public string NombreRol { get; set; }

        // Lista de permisos disponibles
        public List<PermisoCheckBoxViewModel> Permisos { get; set; } = new List<PermisoCheckBoxViewModel>();
    }


    public class PermisoCheckBoxViewModel
    {
        public int IdPermiso { get; set; }
        public string NombrePermiso { get; set; }
        public bool Asignado { get; set; } // Indica si el permiso está asignado o no
    }
}
