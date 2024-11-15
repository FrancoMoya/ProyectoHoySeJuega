
using System.ComponentModel.DataAnnotations;

namespace ProyectoHsj_Beta.ViewsModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
