namespace ProyectoHsj_Beta.ViewsModels
{
    public class UsuarioVM
    {
        public string NombreUsuario { get; set; } = null!;

        public string ApellidoUsuario { get; set; } = null!;

        public string CorreoUsuario { get; set; } = null!;

        public string ContraseniaUsuario { get; set; } = null!;
        public string ConfirmarContraseña { get; set; } = null!;

        public int TelefonoUsuario { get; set; } 

        public int IdRol { get; set; }
    }
}
