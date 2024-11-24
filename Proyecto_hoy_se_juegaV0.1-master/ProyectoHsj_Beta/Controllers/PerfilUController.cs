using Microsoft.AspNetCore.Mvc;
using ProyectoHsj_Beta.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using ProyectoHsj_Beta.ViewsModels;
using ProyectoHsj_Beta.Services;
using Microsoft.AspNetCore.Authorization;

namespace ProyectoHsj_Beta.Controllers
{
    public class PerfilUController : Controller
    {
        private readonly HoySeJuegaContext _context;
        private readonly AuditoriaService _auditoriaService;

        public PerfilUController(HoySeJuegaContext Context, AuditoriaService auditoriaService)
        {
            _context = Context;
            _auditoriaService = auditoriaService;
        }
        [Authorize(Policy = "UserOnly")]
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



        [Authorize(Policy = "UserOnly")]
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
                    await _auditoriaService.RegistrarAuditoriaAsync(
                    seccion: "Perfil",
                    descripcion: "El usuario ha actualizado su número de celular.",
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
