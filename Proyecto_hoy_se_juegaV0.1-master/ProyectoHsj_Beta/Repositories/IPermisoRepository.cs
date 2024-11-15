using ProyectoHsj_Beta.Models;

namespace ProyectoHsj_Beta.Repositories
{
    public interface IPermisoRepository
    {
        Task<List<Permiso>> GetAllPermisosAsync();
        Task AddPermisoAsync(Permiso permiso);
        //Otorgar manualmente id autoincremental (Borrar una vez tengan la base de datos
        //actualizada o comentar en caso de errores) -->
        Task UpdatePermisoAsync(Permiso permiso);
        Task DeletePermisoAsync(int id);
        Task<List<Permiso>> GetPermisosByRolIdAsync(int rolId);
        Task<Permiso> GetPermisoByIdAsync(int id);
        Task<List<Rol>> GetAllRolesAsync();
        Task<Rol> GetRolByIdAsync(int idRol);
        Task AsignarPermisosARolAsync(int rolId, List<int> permisosIds);
    }
}
