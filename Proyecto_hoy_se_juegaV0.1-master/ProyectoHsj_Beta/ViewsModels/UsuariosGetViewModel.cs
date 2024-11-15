namespace ProyectoHsj_Beta.ViewsModels
{
    public class UsuariosGetViewModel
    {
        public int IdUsuario { get; set; }
        public string NombreUsuario { get; set; }
        public string ApellidoUsuario { get; set; }
        public string CorreoUsuario { get; set; }
        public int TelefonoUsuario { get; set; }
        public string RolUsuario { get; set; }
        public bool Activo { get; set; }
        public DateTime UltimaSesion { get; set; }
    }
}
