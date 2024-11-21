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


        //para el calendario
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
            var descripcionAuditoria = $"El usuario ha cancelado una reserva. Detalles de la reserva, ID: {reserva.IdReserva}.";
            reserva.IdEstadoReserva = 3;  // Estado: Cancelada
            _context.Reservas.Update(reserva); // Cambiar el estado
            _context.SaveChanges();
            await _auditoriaService.RegistrarAuditoriaAsync(
            seccion: "Administración",
            descripcion: descripcionAuditoria,
            idAccion: 2);
            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> EliminarReserva(int id)
        {
            var reserva = _context.Reservas.Find(id);
            if (reserva != null)
            {
                var descripcionAuditoria = $"El usuario ha eliminado la reserva con el ID: {reserva.IdReserva}.";
                _context.Reservas.Remove(reserva);
                _context.SaveChanges();
                await _auditoriaService.RegistrarAuditoriaAsync(
                seccion: "Administración",
                descripcion: descripcionAuditoria,
                idAccion: 3);
                return Ok();
            }
            return NotFound();
        }

        //ELIMINAR REGISTROS

        public IActionResult DeleteData()
        {
            return View();
        }
        public async Task<IActionResult> EliminarTodosLosRegistros()
        {
            try
            {
                // Calcular la fecha límite (90 días atrás)
                DateTime fechaLimite = DateTime.Now.AddDays(-90);
                DateOnly fechaLimiteEvtRec = DateOnly.FromDateTime(DateTime.Now.AddDays(-90));
                DateTime fechaLimiteReservas = DateTime.Now.AddDays(-104);
                DateTime fechaLimiteEventos = DateTime.Now.AddDays(-114);

                // Obtener los registros antiguos de la base de datos
                var reservasAntiguas = await _context.Reservas
                    .Where(r => r.FechaReserva < fechaLimiteReservas)
                    .ToListAsync();

                var horariosAntiguos = await _context.HorarioDisponibles
                    .Where(h => h.FechaHorario < DateOnly.FromDateTime(fechaLimite))
                    .ToListAsync();

                var auditoriasAntiguas = await _context.Auditoria
                    .Where(r => r.FechaAuditoria < fechaLimite)
                    .ToListAsync();

                var pagosAntiguos = await _context.Pagos
                    .Where(r => r.FechaPago < fechaLimiteReservas)
                    .ToListAsync();

                var eventosAntiguos = await _context.Eventos
                    .Where(r => r.FechaEvento < fechaLimite)
                    .ToListAsync();

                var eventosRecurrentesAntiguos = await _context.EventoRecurrentes
                    .Where(r => r.FechaFin < fechaLimiteEvtRec)
                    .ToListAsync();


                // Validar si hay registros para eliminar
                if (!reservasAntiguas.Any() && !horariosAntiguos.Any() && !auditoriasAntiguas.Any() && !pagosAntiguos.Any() && !eventosAntiguos.Any() && !eventosRecurrentesAntiguos.Any())
                {
                    TempData["ErrorMessage"] = "No se encontraron registros para eliminar.";
                    return RedirectToAction("DeleteData", "Admin");
                }

                // Eliminar registros si existen
                if (reservasAntiguas.Any())
                {
                    _context.Reservas.RemoveRange(reservasAntiguas);
                }

                if (auditoriasAntiguas.Any())
                {
                    _context.Auditoria.RemoveRange(auditoriasAntiguas);
                }
                if (pagosAntiguos.Any())
                {
                    _context.Pagos.RemoveRange(pagosAntiguos);
                }
                if (eventosAntiguos.Any())
                {
                    _context.Eventos.RemoveRange(eventosAntiguos);
                }
                if (eventosRecurrentesAntiguos.Any())
                {
                    _context.EventoRecurrentes.RemoveRange(eventosRecurrentesAntiguos);
                }
                if (horariosAntiguos.Any())
                {
                    _context.HorarioDisponibles.RemoveRange(horariosAntiguos);
                }

                // Guardar cambios en la base de datos
                int registrosEliminados = reservasAntiguas.Count + horariosAntiguos.Count + auditoriasAntiguas.Count + eventosAntiguos.Count + pagosAntiguos.Count + eventosRecurrentesAntiguos.Count;
                await _context.SaveChangesAsync();

                // Registrar auditoría
                await _auditoriaService.RegistrarAuditoriaAsync(
                    seccion: "Mantenimiento",
                    descripcion: $"El ADMINISTRADOR eliminó {registrosEliminados} registros antiguos. Incluyendo registros de las siguientes tablas de BD: RESERVA, EVENTO, HORARIO_DISPONIBLE, AUDITORIA, PAGO, EVENTO_RECURRENTE.",
                    idAccion: 3);

                // Mensaje de éxito
                TempData["Message"] = $"Se eliminaron {registrosEliminados} registros correctamente.";
                TempData["MessageDetails"] = $"Reservas eliminadas: {reservasAntiguas.Count}, Eventos eliminados: {eventosAntiguos.Count}, Horarios eliminados: {horariosAntiguos.Count}, Auditorías eliminadas: {auditoriasAntiguas.Count}, Pagos eliminados: {pagosAntiguos.Count}, Reservas FIJAS eliminadas: {eventosRecurrentesAntiguos.Count}.";
            }
            catch (Exception ex)
            {
                // Manejo de errores y auditoría
                TempData["ErrorMessage"] = "Ocurrió un error inesperado al intentar eliminar los registros.";
                TempData["ErrorDetails"] = ex.Message;
            }

            return RedirectToAction("DeleteData", "Admin");
        }

    }
}

