using Microsoft.AspNetCore.Mvc;
using ProyectoHsj_Beta.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;

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
            Console.WriteLine(" usuario :" + usuario.NombreUsuario);

            if(usuario == null)
            {
                return RedirectToAction("Signup","Acces");
            }

            return View(usuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Perfil(Usuario usuario)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _context.Usuarios.FindAsync(usuario.IdUsuario);
                if (existingUser == null)
                {
                    return NotFound();
                }

                // Actualiza solo los campos permitidos
                existingUser.NombreUsuario = usuario.NombreUsuario;
                existingUser.ApellidoUsuario = usuario.ApellidoUsuario;
                existingUser.CorreoUsuario = usuario.CorreoUsuario;
                existingUser.TelefonoUsuario = usuario.TelefonoUsuario;
                // Puedes agregar aquí la lógica para actualizar la contraseña si es necesario

                await _context.SaveChangesAsync();
                return RedirectToAction("Perfil");
            }

            return View(usuario);
        }

        
    }
}
