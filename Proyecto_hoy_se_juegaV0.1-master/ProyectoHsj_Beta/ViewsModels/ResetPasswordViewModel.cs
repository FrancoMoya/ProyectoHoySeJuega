using System.ComponentModel.DataAnnotations;

namespace ProyectoHsj_Beta.ViewsModels
{
    public class ResetPasswordViewModel
    {
        [Required]
        public string? Token { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string? NewPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Las contraseñas ingresadas no coinciden.")]
        public string? ConfirmPassword { get; set; }
    }
}
