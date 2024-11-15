using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoHsj_Beta.Models;

namespace ProyectoHsj_Beta.Controllers
{
    public class Estadisticas : Controller
    {
        private readonly HoySeJuegaContext _context;

        public Estadisticas(HoySeJuegaContext context)
        {
            _context = context;
        }
        // se obtiene los datos de usuarios registrados y autenticados
        public async Task<JsonResult> GetUserStatistics()
        {
            var totalUsuariosRegistrados = await _context.Usuarios.CountAsync();
            var usuariosAutenticados = await _context.Usuarios
                                                    .Where(u => u.EmailConfirmed == true)
                                                    .CountAsync();
            Console.WriteLine("Total de usuarios registrados : " + totalUsuariosRegistrados);
            Console.WriteLine("Total de usuarios autenticados : " + usuariosAutenticados);

            var data = new
            {
                Registrados = totalUsuariosRegistrados,
                Autenticados = usuariosAutenticados
            };

            return Json(data);
        }


        public IActionResult Index()
        {
            return View();
        }
    }
}
