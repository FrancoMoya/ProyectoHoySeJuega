using Microsoft.AspNetCore.Mvc;
using ProyectoHsj_Beta.Models;
using System.Threading.Tasks;
using ProyectoHsj_Beta.DTO;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ProyectoHsj_Beta.ViewsModels;
using ProyectoHsj_Beta.Services;
namespace ProyectoHsj_Beta.Controllers
{
    public class AdminController : Controller
    {

        private readonly HoySeJuegaContext _context;
        private readonly AuditoriaService _auditoriaService;


        public AdminController(HoySeJuegaContext context, AuditoriaService auditoriaService)
        {
            _context = context;
            _auditoriaService = auditoriaService;
        }

        public IActionResult Index()
        {
            return View();
        }


        public async Task<IActionResult> GetReservas()
        {
            var reservas = await _context.Set<ReservasAdminGetViewModel>()
                .FromSqlRaw("EXEC SP_GET_RESERVAS_ADMIN")
                .ToListAsync();
            return Json(reservas);
        }

        [HttpPost]
        public async Task<IActionResult> CancelarReserva(int id)
        {
            var reserva = _context.Reservas.Find(id);
            Console.WriteLine("tenes la id reserva :" + id);
            if(reserva == null)
            {
                return NotFound();
            }
            Console.WriteLine("Entro a la condicional.");
            reserva.IdEstadoReserva = 3;  // Estado: Cancelada
            _context.Reservas.Update(reserva); // Cambiar el estado
            _context.SaveChanges();
            await _auditoriaService.RegistrarAuditoriaAsync(
            seccion: "Administración",
            descripcion: "El usuario ha cancelado una reserva.",
            idAccion: 2);
            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> EliminarReserva(int id)
        {
            var reserva = _context.Reservas.Find(id);
            if (reserva != null)
            {
                _context.Reservas.Remove(reserva);
                _context.SaveChanges();
                await _auditoriaService.RegistrarAuditoriaAsync(
                seccion: "Administración",
                descripcion: "El usuario ha eliminado una reserva.",
                idAccion: 3);
                return Ok();
            }
            return NotFound();
        }
    }
}

