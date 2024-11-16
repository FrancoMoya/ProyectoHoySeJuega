using Microsoft.AspNetCore.Mvc;
using ProyectoHsj_Beta.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using ProyectoHsj_Beta.ViewsModels;

namespace ProyectoHsj_Beta.Controllers
{
    public class PerfilUController : Controller
    {
        private readonly HoySeJuegaContext _context;

        public PerfilUController(HoySeJuegaContext Context)
        {
            _context = Context;
        }
        
        public async Task <IActionResult> Perfil()
        {
            var usuarioclaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (usuarioclaim == null)
            {
                return RedirectToAction("Login", "Acces");

            }
            var userId = int.Parse(usuarioclaim.Value);
            var usuario = await _context.Usuarios
                .Include(u => u.IdRolNavigation)
                .FirstOrDefaultAsync(u => u.IdUsuario == userId);

            if(usuario == null)
            {
                return RedirectToAction("Signup","Acces");
            }

            return View(usuario);
        }



        // GET: Usuarios/Edit/5
        public async Task<IActionResult> EditMiPerfil()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            var userClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int userId = int.Parse(userClaim);
            var usuario = await _context.Usuarios.FindAsync(userId);
            if (usuario == null)
            {
                return NotFound();
            }
            var viewModel = new EditMiPerfilViewModel
            {
                IdUsuario = usuario.IdUsuario,
                TelefonoUsuario = Convert.ToString(usuario.TelefonoUsuario),
            };

            return View(viewModel);
        }

        // POST: Usuario/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMiPerfil(int id, EditMiPerfilViewModel viewModel)
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

                    usuario.TelefonoUsuario = Convert.ToInt32(viewModel.TelefonoUsuario);
                    _context.Update(usuario);
                    await _context.SaveChangesAsync();
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
                return RedirectToAction("Index", "Home");
            }
            return View(viewModel);
        }

        private bool UsuarioExists(int id)
        {
            return _context.Usuarios.Any(e => e.IdUsuario == id);
        }
    }
}
