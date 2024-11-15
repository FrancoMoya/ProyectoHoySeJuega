using Microsoft.AspNetCore.Mvc;
using ProyectoHsj_Beta.Models;
using System.Threading.Tasks;
using ProyectoHsj_Beta.DTO;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ProyectoHsj_Beta.ViewsModels;
namespace ProyectoHsj_Beta.Controllers
{
    public class AdminController : Controller
    {

        private readonly HoySeJuegaContext _context;

        public AdminController(HoySeJuegaContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        
        [HttpGet]
        public IActionResult GetReservas()
        {
            var reservas = _context.Reservas
                .Include(r => r.IdUsuarioNavigation) // Cargar datos del usuario relacionado
                .Select(r => new
                {
                    idReserva = r.IdReserva,
                    title = $"Reserva #{r.IdReserva}",
                    start = r.FechaReserva,
                    estado = r.IdEstadoReservaNavigation.NombreEstadoReserva,
                    usuarioNombre = r.IdUsuarioNavigation.NombreUsuario,
                    usuarioTelefono = r.IdUsuarioNavigation.TelefonoUsuario
                }).ToList();

            return Json(reservas);
        }
    }
}

