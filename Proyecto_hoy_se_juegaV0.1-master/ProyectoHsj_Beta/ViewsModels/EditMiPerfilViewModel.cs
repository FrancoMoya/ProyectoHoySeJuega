using System.ComponentModel.DataAnnotations;

namespace ProyectoHsj_Beta.ViewsModels
{
    public class EditMiPerfilViewModel
    {
        public int IdUsuario { get; set; }
        [Required(ErrorMessage = "El número de celular es obligatorio.")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "El número de celular debe contener exactamente 10 dígitos.")]
        public string TelefonoUsuario { get; set; }
    }
}
