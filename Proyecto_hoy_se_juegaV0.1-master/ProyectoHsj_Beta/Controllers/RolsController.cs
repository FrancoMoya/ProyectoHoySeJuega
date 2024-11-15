using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoHsj_Beta.Attributes;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProyectoHsj_Beta.Models;
using ProyectoHsj_Beta.ViewsModels;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ProyectoHsj_Beta.Controllers
{
    public class RolsController : Controller
    {
        private readonly HoySeJuegaContext _context;

        public RolsController(HoySeJuegaContext context)
        {
            _context = context;
        }

        // View de rol

        //[Permiso("bloc")]
        public async Task<IActionResult> Index()
        {
            var roles = await _context.Set<RolesPermisosGetViewModel>()
                .FromSqlRaw("EXEC SP_GET_ROLES_PERMISOS")
                .ToListAsync();
            return View(roles);
        }

        // Acción GET para crear un nuevo rol
        [HttpGet]
        public async Task<ActionResult> Create()
        {
            // Obtener todos los permisos disponibles para asignar
            var permisos = await _context.Permisos
                .Select(p => new SelectListItem
                {
                    Value = p.IdPermiso.ToString(),
                    Text = p.NombrePermiso
                })
                .ToListAsync();

            // Crear el ViewModel y pasarlo a la vista
            var viewModel = new RolCreateViewModel
            {
                PermisosDisponibles = permisos,
                PermisosSeleccionados = new List<int>() // Lista vacía inicialmente
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(RolCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Crear el nuevo rol
                var nuevoRol = new Rol { NombreRol = model.NombreRol };
                _context.Rols.Add(nuevoRol);
                await _context.SaveChangesAsync(); // Guarda el rol y genera su ID

                // Crear una lista de parámetros para insertar en la tabla intermedia
                if (model.PermisosSeleccionados != null && model.PermisosSeleccionados.Count > 0)
                {
                    var parametros = new List<SqlParameter>();
                    foreach (var permisoId in model.PermisosSeleccionados)
                    {
                        // Insertar directamente en la tabla intermedia
                        var idRolParam = new SqlParameter("@ID_rol", SqlDbType.Int) { Value = nuevoRol.IdRol };
                        var idPermisoParam = new SqlParameter("@ID_permiso", SqlDbType.Int) { Value = permisoId };
                        parametros.Add(idRolParam);
                        parametros.Add(idPermisoParam);

                        // Aquí deberías usar un query que inserte en la tabla intermedia
                        await _context.Database.ExecuteSqlRawAsync(
                            "INSERT INTO PERMISO_ROL (ID_rol, ID_permiso) VALUES (@ID_rol, @ID_permiso)",
                            idRolParam, idPermisoParam);
                    }
                }

                // Si todo va bien, redirigir a otra vista (por ejemplo, la lista de roles)
                return RedirectToAction("Index", "Rols");
            }

            // Si el modelo no es válido, regresa a la vista de creación con errores
            return View(model);
        }



        // GET: Rols/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rol = await _context.Rols
                .FirstOrDefaultAsync(m => m.IdRol == id);
            if (rol == null)
            {
                return NotFound();
            }

            return View(rol);
        }

        // POST: Rols/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var rol = await _context.Rols.FindAsync(id);
            if (rol != null)
            {
                _context.Rols.Remove(rol);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RolExists(int id)
        {
            return _context.Rols.Any(e => e.IdRol == id);
        }
    }
}
