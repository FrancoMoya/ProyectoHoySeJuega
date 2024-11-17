using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProyectoHsj_Beta.Models;
using ProyectoHsj_Beta.Services;
using ProyectoHsj_Beta.ViewsModels;

namespace ProyectoHsj_Beta.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly HoySeJuegaContext _context;
        private readonly AuditoriaService _auditoriaService;

        public UsuariosController(HoySeJuegaContext context, AuditoriaService auditoriaService)
        {
            _context = context;
            _auditoriaService = auditoriaService;
        }

        // Metodo Get
        public async Task<IActionResult> Index()
        {
            var usuarios = await _context.Set<UsuariosGetViewModel>()
                .FromSqlRaw("EXEC SP_GET_USERS")
                .ToListAsync();
            return View(usuarios);
        }
      

        // GET: Usuarios/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            var viewModel = new UsuarioEditViewModel
            {
                IdUsuario = usuario.IdUsuario,
                Activo = usuario.Activo,
                IdRol = usuario.IdRol,
                Roles = await _context.Rols.Select(r => new SelectListItem
                {
                    Value = r.IdRol.ToString(),
                    Text = r.NombreRol
                }).ToListAsync(),
                // Aquí agregamos las opciones de Activo o No Activo
                ActivoOptions = new List<SelectListItem>
                {
                    new SelectListItem { Value = "true", Text = "Activo" },
                    new SelectListItem { Value = "false", Text = "No Activo" }
                }
            };

            return View(viewModel);
        }

        // POST: Usuario/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UsuarioEditViewModel viewModel)
        {
            if (id != viewModel.IdUsuario)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var usuario = await _context.Usuarios.FindAsync(id);
                    if (usuario == null)
                    {
                        return NotFound();
                    }

                    usuario.Activo = viewModel.Activo;
                    usuario.IdRol = viewModel.IdRol ?? (int?)null;  // Esta es la forma correcta de manejarlo

                    _context.Update(usuario);
                    await _context.SaveChangesAsync();
                    await _auditoriaService.RegistrarAuditoriaAsync(
                    seccion: "Administración",
                    descripcion: "Se ha editado el rol y/o el tipo activo de un usuario.",
                    idAccion: 2);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UsuarioExists(viewModel.IdUsuario))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            // If model state is not valid, reload the roles and return the view
            viewModel.Roles = await _context.Rols.Select(r => new SelectListItem
            {
                Value = r.IdRol.ToString(),
                Text = r.NombreRol
            }).ToListAsync();
            return View(viewModel);
        }

        // GET: Usuarios/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios
                .Include(u => u.IdRolNavigation)
                .FirstOrDefaultAsync(m => m.IdUsuario == id);
            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        // POST: Usuarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario != null)
            {
                _context.Usuarios.Remove(usuario);
            }

            await _context.SaveChangesAsync();
            await _auditoriaService.RegistrarAuditoriaAsync(
                    seccion: "Administración",
                    descripcion: "Se ha eliminado a un usuario.",
                    idAccion: 3);
            return RedirectToAction(nameof(Index));
        }

        private bool UsuarioExists(int id)
        {
            return _context.Usuarios.Any(e => e.IdUsuario == id);
        }
    }
}
