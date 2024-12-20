﻿using Microsoft.AspNetCore.Mvc;
using ProyectoHsj_Beta.Models;
using System.Threading.Tasks;
using ProyectoHsj_Beta.DTO;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ProyectoHsj_Beta.ViewsModels;
using ProyectoHsj_Beta.Services;
using Microsoft.AspNetCore.Authorization;
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
        //RESERVAS CLIENTES CALENDARIO
        [Authorize(Policy = "AdminOrEmployed")]
        public IActionResult Index()
        {
            return View();
        }


        //para el calendario
        [Authorize(Policy = "AdminOrEmployed")]
        public async Task<IActionResult> GetReservas()
        {
            var reservas = await _context.Set<ReservasAdminGetViewModel>()
                .FromSqlRaw("EXEC SP_GET_RESERVAS_ADMIN")
                .ToListAsync();
            return Json(reservas);
        }

        [Authorize(Policy = "AdminOrEmployed")]
        [HttpPost]
        public async Task<IActionResult> CancelarReserva(int id)
        {
            var reserva = await _context.Reservas.FindAsync(id);
            if(reserva != null)
            {
                var horario = await _context.HorarioDisponibles
                                             .FirstOrDefaultAsync(h => h.IdHorarioDisponible == reserva.IdHorarioDisponible);
                var estado = reserva.IdEstadoReserva;
                if((estado != null) && (horario != null))
                {
                    var descripcionAuditoria = $"El usuario ha cancelado una reserva. Detalles de la reserva, ID: {reserva.IdReserva}.";
                    horario.DisponibleHorario = true;
                    reserva.IdEstadoReserva = 3;  // Estado: Cancelada
                    await _context.SaveChangesAsync();
                    await _auditoriaService.RegistrarAuditoriaAsync(
                    seccion: "Administración",
                    descripcion: descripcionAuditoria,
                    idAccion: 2);
                }
                
            }
            
            return RedirectToAction(nameof(Index));
        }

        //ELIMINAR REGISTROS
        [Authorize(Policy = "AdminOnly")]
        public IActionResult DeleteData()
        {
            return View();
        }
        [Authorize(Policy = "AdminOnly")]
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

