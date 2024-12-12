using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using ProyectoHsj_Beta.Models;
using ProyectoHsj_Beta.Services;
using ProyectoHsj_Beta.ViewsModels;
using System.Data;

namespace ProyectoHsj_Beta.Controllers
{
    public class EventoController : Controller
    {
        private readonly HoySeJuegaContext _context;
        private readonly AuditoriaService _auditoriaService;

        public EventoController(HoySeJuegaContext context, AuditoriaService auditoriaService)
        {
            _context = context;
            _auditoriaService = auditoriaService;
        }

        [Authorize(Policy = "AdminOrEmployed")]
        public async Task<IActionResult> Index()
        {
            var eventos = await _context.Set<EventosAdminGetViewModel>()
                .FromSqlRaw("EXEC SP_GET_EVENTOS_ADMIN")
                .ToListAsync();
            return View(eventos);
        }
        [Authorize(Policy = "AdminOrEmployed")]
        public async Task<IActionResult> ReservasAdminHistorial()
        {
            var eventos = await _context.Set<ReservasAdminHistorialGetViewModel>()
                .FromSqlRaw("EXEC SP_GET_RESERVAS_ADMIN_HISTORIAL")
                .ToListAsync();
            return View(eventos);
        }

        //Obtener y renderizar los horarios PARA EVENTOS ADMIN
        [Authorize(Policy = "AdminOrEmployed")]
        [HttpGet]
        public async Task<IActionResult> GetAvailableTimeSlotsAdmin(DateTime fecha)
        {
            TimeOnly horaLimite = TimeOnly.FromDateTime(DateTime.Now.AddHours(3));
            var horarios = await _context.HorarioDisponibles
                .Where(h => (h.DisponibleHorario ?? false) && h.FechaHorario == DateOnly.FromDateTime(fecha) && (
                    // Si es hoy, la hora de inicio debe ser mayor o igual a la hora actual
                    (h.FechaHorario == DateOnly.FromDateTime(DateTime.Now.AddHours(3)) && h.HoraInicio >= horaLimite) ||
                    // Si no es hoy, cualquier horario de inicio futuro es válido
                    h.FechaHorario > DateOnly.FromDateTime(DateTime.Now.AddHours(3))
                    ))
                .Select(h => new
                {
                    IdHorarioDisponible = h.IdHorarioDisponible, // Agrega el ID
                    HoraInicio = h.HoraInicio.ToString("HH:mm"),
                    HoraFin = h.HoraFin.ToString("HH:mm")
                })
                .ToListAsync();

            // Cambiar el return para incluir ID y las horas
            return Json(horarios);
        }

        [Authorize(Policy = "AdminOrEmployed")]
        [HttpPost, ActionName("Cancel")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelConfirmed(int id)
        {
            var evento = await _context.Eventos.FindAsync(id);
            if (evento != null)
            {
                var horario = await _context.HorarioDisponibles
                                             .FirstOrDefaultAsync(h => h.IdHorarioDisponible == evento.IdHorarioDisponible);
                var estado = evento.IdEstadoReserva;
                if ((horario != null) && (estado != null))
                {
                    var descripcionAuditoria = $"El usuario ha cancelado un evento. Detalles, ID del evento: {evento.IdEvento}.";
                    horario.DisponibleHorario = true;
                    // Guardar los cambios en la tabla HorariosDisponibles
                    evento.IdEstadoReserva = 3;
                    // Cambiar a estado cancelado
                    await _context.SaveChangesAsync();
                    await _auditoriaService.RegistrarAuditoriaAsync(
                    seccion: "Administración",
                    descripcion: descripcionAuditoria,
                    idAccion: 2);
                }
            }
            return RedirectToAction(nameof(IndexCalendar));
        }

        [Authorize(Policy = "AdminOrEmployed")]
        public ActionResult Create()
        {
            return View();
        }

        [Authorize(Policy = "AdminOrEmployed")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(EventoCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var fechaActual = DateTime.Now.AddHours(3); // Aumenta 3 horas
                // Crear el nuevo evento
                var nuevoEvento = new Evento 
                { 
                    NombreEvento = model.NombreEvento.ToUpper(),
                    DescripcionEvento = model.DescripcionEvento,
                    CorreoClienteEvento = model.CorreoClienteEvento,
                    TelefonoClienteEvento = model.TelefonoClienteEvento,
                    FechaEvento = fechaActual,
                    IdHorarioDisponible = model.IdHorarioDisponible,
                    IdEstadoReserva = 2 // CONFIRMADA
                };
                
                _context.Eventos.Add(nuevoEvento);
                await _context.SaveChangesAsync();
                var descripcionAuditoria = $"El usuario ha creado un nuevo evento. Detalles, ID del evento: {nuevoEvento.IdEvento}.";
                await _auditoriaService.RegistrarAuditoriaAsync(
                seccion: "Administración",
                descripcion: descripcionAuditoria,
                idAccion: 1);

                // Obtener el ID del horario seleccionado y actualizar el estado en HorariosDisponibles
                var horario = await _context.HorarioDisponibles
                                             .FirstOrDefaultAsync(h => h.IdHorarioDisponible == model.IdHorarioDisponible);

                if (horario != null)
                {
                    // Cambiar el campo booleano (por ejemplo, "Disponible" a false)
                    horario.DisponibleHorario = false;

                    // Guardar los cambios en la tabla HorariosDisponibles
                    await _context.SaveChangesAsync();
                }


                return RedirectToAction(nameof(IndexCalendar));
            }
            // Si el modelo no es válido, regresa a la vista de creación con errores
            return View(model);

        }


        [Authorize(Policy = "AdminOrEmployed")]
        public IActionResult IndexCalendar()
        {
            return View();
        }
        [Authorize(Policy = "AdminOrEmployed")]
        public async Task<IActionResult> GetReservas()
        {
            var reservas = await _context.Set<ReservasAdminGetViewModel>()
                .FromSqlRaw("EXEC SP_GET_RESERVAS_EVENTOS_CALENDAR_ADMIN")
                .ToListAsync();
            return Json(reservas);
        }
        [Authorize(Policy = "AdminOrEmployed")]
        [HttpPost]
        public async Task<IActionResult> CancelConfirmedCalendar(int id)
        {
            var evento = await _context.Eventos.FindAsync(id);
            if (evento != null)
            {
                var horario = await _context.HorarioDisponibles
                                             .FirstOrDefaultAsync(h => h.IdHorarioDisponible == evento.IdHorarioDisponible);
                var estado = evento.IdEstadoReserva;
                if ((horario != null) && (estado != null))
                {
                    var descripcionAuditoria = $"El usuario ha cancelado un evento. Detalles, ID del evento: {evento.IdEvento}.";
                    horario.DisponibleHorario = true;
                    // Guardar los cambios en la tabla HorariosDisponibles
                    evento.IdEstadoReserva = 3;
                    // Cambiar a estado cancelado
                    await _context.SaveChangesAsync();
                    await _auditoriaService.RegistrarAuditoriaAsync(
                    seccion: "Administración",
                    descripcion: descripcionAuditoria,
                    idAccion: 2);
                }
            }
            return RedirectToAction(nameof(IndexCalendar));
        }
    }
}
