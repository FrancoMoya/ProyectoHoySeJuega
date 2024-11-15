using Microsoft.EntityFrameworkCore;
using ProyectoHsj_Beta.Models;

namespace ProyectoHsj_Beta.Repositories
{
    public class PermisoRepository : IPermisoRepository
    {
        private readonly HoySeJuegaContext _juegaContext;
        
        public PermisoRepository(HoySeJuegaContext juegaContext)
        {
            _juegaContext = juegaContext;

        }

        public async Task<List<Permiso>> GetAllPermisosAsync() // Nuevo método para obtener todos los permisos
        {
            return await _juegaContext.Permisos.ToListAsync();
        }

        public async Task<Permiso> GetPermisoByIdAsync(int id)
        {
            return await _juegaContext.Permisos.FindAsync(id);
        }

        //obtener todos los roles
        public async Task<List<Rol>> GetAllRolesAsync()
        {
            return await _juegaContext.Rols.ToListAsync();
        }
        //obtener roles por id
        public async Task<Rol> GetRolByIdAsync(int idRol)
        {
            return await _juegaContext.Rols
                        .Include(r => r.IdPermisos) // Incluir los permisos relacionados
                        .FirstOrDefaultAsync(r => r.IdRol == idRol);
        }
        //Otorgar manualmente id autoincremental (Borrar una vez tengan la base de datos actualizada o comentar en caso de errores)
        //public async Task<int> GetNextPermisoIdAsync()
        //{
            //var lastPermiso = await _juegaContext.Permisos.OrderByDescending(p => p.IdPermiso).FirstOrDefaultAsync();
            //return (lastPermiso != null) ? lastPermiso.IdPermiso + 1 : 1; // Si no hay permisos, comienza desde 1
        //}


        public async Task AddPermisoAsync(Permiso permiso)
        {
            _juegaContext.Permisos.Add(permiso);
             await _juegaContext.SaveChangesAsync();
        }

        public async Task UpdatePermisoAsync(Permiso permiso)
        {
            _juegaContext.Permisos.Update(permiso);
            await _juegaContext.SaveChangesAsync();
        }

        public async Task DeletePermisoAsync(int id)
        {
            var permiso = await _juegaContext.Permisos.FindAsync(id);
            if (permiso != null)
            {
                _juegaContext.Permisos.Remove(permiso);
                await _juegaContext.SaveChangesAsync();
            }
        }


        public async Task<List<Permiso>> GetPermisosByRolIdAsync(int rolId)
        {
            var rol = await _juegaContext.Rols.Include(r => r.IdPermisos)
                                          .FirstOrDefaultAsync(r => r.IdRol == rolId);
            return rol?.IdPermisos.ToList() ?? new List<Permiso>();
        }

        public async Task AsignarPermisosARolAsync(int rolId, List<int> permisosIds)
        {
            Console.WriteLine("Iniciando asignación de permisos fase repositorio...");
            var rol = await _juegaContext.Rols.Include(r => r.IdPermisos)
                                          .FirstOrDefaultAsync(r => r.IdRol == rolId);

            if (rol == null) throw new Exception("Rol no encontrado");
            Console.WriteLine("Ids de permisos seleccionados: " + string.Join(", ", permisosIds)); // Agregar aquí para inspeccionar permisosIds

            Console.WriteLine("Rol encontrado: " + rol.NombreRol);
            // Limpiar permisos que ya existen
            var permisos = await _juegaContext.Permisos
                                    .Where(p => permisosIds.Contains(p.IdPermiso))
                                    .ToListAsync();

            Console.WriteLine("Permisos obtenidos: " + permisos.Count);
            rol.IdPermisos.Clear();
            Console.WriteLine("Permisos actuales limpiados.");
            // Asignar nuevos permisos
            foreach (var permiso in permisos)
            {
                if (permiso != null)
                {
                    rol.IdPermisos.Add(permiso);
                    Console.WriteLine("Permiso añadido: " + permiso.NombrePermiso);
                }
            }

            try
            {
                await _juegaContext.SaveChangesAsync();
                Console.WriteLine("Cambios guardados en la base de datos.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al guardar los cambios: " + ex.Message);
                throw; // Para propagar el error al controlador
            }
        }

    }
}
