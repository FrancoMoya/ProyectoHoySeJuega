using Microsoft.AspNetCore.Mvc.Rendering;

namespace ProyectoHsj_Beta.ViewsModels
{
    public class UsuarioEditViewModel
    {
        public int IdUsuario {  get; set; }
        public bool? Activo { get; set; } 
        public int? IdRol { get; set; }
        public List<SelectListItem>? Roles { get; set; }  // Para el dropdown de roles
        public List<SelectListItem>? ActivoOptions { get; set; }  // Opciones de Activo / No Activo
    }
}
