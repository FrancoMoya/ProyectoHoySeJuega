using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProyectoHsj_Beta.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProyectoHsj_Beta.Attributes;
using Microsoft.AspNetCore.Authorization;
using ProyectoHsj_Beta.Repositories;
using ProyectoHsj_Beta.ViewsModels;
using System.Data;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ProyectoHsj_Beta.Controllers
{
    public class PermisosController : Controller
    {
        private readonly IPermisoRepository _permisoRepository;

        public PermisosController(IPermisoRepository permisoRepository)
        {
            _permisoRepository = permisoRepository;
        }
        [Authorize(Policy = "AdminOnly")]

        [HttpGet]
        public async Task<IActionResult>ManageView()
        {
            var permisos = await _permisoRepository.GetAllPermisosAsync();
            Console.WriteLine($"Permisos obtenidos: {permisos?.Count()}");
            // Usar el repositorio para obtener permisos
            if (!permisos.Any())
            {
                ViewData["Error"] = "No se encontraron permisos.";
            }
            return View(permisos);
        }

        // GET: PermisosController/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var permiso = await _permisoRepository.GetPermisoByIdAsync(id.Value); // Usar el repositorio para obtener el permiso
            if (permiso == null)
            {
                return NotFound();
            }

            return View(permiso);
        }


        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        // POST: PermisosController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("NombrePermiso")] Permiso permiso)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _permisoRepository.AddPermisoAsync(permiso);
                    return RedirectToAction("ManageView");
                }
                catch (Exception ex)
                {
                    ViewData["Error"] = "Ocurrió un problema al crear el permiso: " + ex.Message;
                }
            }
            return View(permiso);
        }

        // Funcion edit de metodo GET
        public async Task<IActionResult> Edit(int id)
        {
            var permiso = await _permisoRepository.GetPermisoByIdAsync(id);
            if (permiso == null)
            {
                return NotFound();
            }
            return View(permiso);
        }

        // Funcion edit de metodo POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Permiso permiso)
        {
            if (id != permiso.IdPermiso)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _permisoRepository.UpdatePermisoAsync(permiso);
                    return RedirectToAction("ManageView");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al actualizar el permiso: " + ex.Message);
                }
            }
            return View(permiso);
        }

        // Funcion Eliminar de metodo GET
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var permiso = await _permisoRepository.GetPermisoByIdAsync(id);
            if (permiso == null)
            {
                return NotFound();
            }
            return View(permiso);
        }


        // Funcion eliminar metodo post
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _permisoRepository.DeletePermisoAsync(id);
                return RedirectToAction("ManageView");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al eliminar el permiso: " + ex.Message);
                return RedirectToAction("ManageView");
            }
        }

        // Funcion para asignar permisos a un rol
        [HttpGet]
        public async Task<IActionResult> AsignarPermisosARol(int idRol)
        {
            var viewModel = new OtorgarPViewModel
            {
                IdRolSeleccionado = idRol,
                Roles = (await _permisoRepository.GetAllRolesAsync()).Select(r => new SelectListItem
                {
                    Value = r.IdRol.ToString(),   // El valor debe ser un string
                    Text = r.NombreRol            // El texto visible en el dropdown
                }).ToList(),  // Convierte la lista de roles a SelectListItem

                Permisos = (await _permisoRepository.GetAllPermisosAsync()).Select(p => new PermisoCheckBoxViewModel
                {
                    IdPermiso = p.IdPermiso,
                    NombrePermiso = p.NombrePermiso,
                    Asignado = false // Inicialmente, ningún permiso está asignado
                }).ToList() // Obtener los permisos como antes

             
            };



            // Obtener los permisos ya asignados al rol
            var permisosAsignados = await _permisoRepository.GetPermisosByRolIdAsync(idRol);

            // Actualizar el estado de los permisos asignados en el ViewModel
            viewModel.Permisos = viewModel.Permisos.Select(p => new PermisoCheckBoxViewModel
            {
                IdPermiso = p.IdPermiso,
                NombrePermiso = p.NombrePermiso,
                Asignado = permisosAsignados.Any(pa => pa.IdPermiso == p.IdPermiso) // Aquí se marcan los permisos asignados
            }).ToList();

            return View(viewModel);

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AsignarPermisosARol(OtorgarPViewModel model)
        {
            Console.WriteLine("Ids de permisos seleccionados antes del try: " + string.Join(", ", model.Permisos.Where(p => p.Asignado).Select(p => p.IdPermiso)));
            try
            {
                // Obtener los IDs de los permisos seleccionados (los checkboxes que están marcados)
                var permisosSeleccionados = model.Permisos
                                                   .Where(p => p.Asignado) // Solo los permisos que están asignados
                                                   .Select(p => p.IdPermiso)
                                                   .ToList();
                Console.WriteLine("Ids de permisos seleccionados controlador : " + string.Join(", ", permisosSeleccionados));
                // Asignar los permisos al rol
                Console.WriteLine("Iniciando asignación de permisos fase controlador...");
                await _permisoRepository.AsignarPermisosARolAsync(model.IdRolSeleccionado, permisosSeleccionados);
                

                Console.WriteLine("Permisos asignados correctamente.");
                // Redireccionar a la vista de roles
                Console.WriteLine("Paso por aca");
                return RedirectToAction("Index", "Rols");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Paso por aca 2");
                // Si hay un error, muestra un mensaje
                ModelState.AddModelError("", "Error al asignar permisos: " + ex.Message);
                return View(model);
            }
        }

    }
}
