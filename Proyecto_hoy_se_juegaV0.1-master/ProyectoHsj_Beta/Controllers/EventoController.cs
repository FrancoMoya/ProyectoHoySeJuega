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

        // GET: EventoController
        public async Task<IActionResult> Index()
        {
            var eventos = await _context.Set<EventosAdminGetViewModel>()
                .FromSqlRaw("EXEC SP_GET_EVENTOS_ADMIN")
                .ToListAsync();
            return View(eventos);
        }

        //Obtener y renderizar los horarios PARA EVENTOS ADMIN
        [HttpGet]
        public async Task<IActionResult> GetAvailableTimeSlotsAdmin(DateTime fecha) //Modificado para incluir solo los horarios dentro de 2 semanas
        {
            TimeOnly horaLimite = TimeOnly.FromDateTime(DateTime.Now);
            var horarios = await _context.HorarioDisponibles
                .Where(h => (h.DisponibleHorario ?? false) && h.FechaHorario == DateOnly.FromDateTime(fecha) && (
                    // Si es hoy, la hora de inicio debe ser mayor o igual a la hora actual
                    (h.FechaHorario == DateOnly.FromDateTime(DateTime.Now) && h.HoraInicio >= horaLimite) ||
                    // Si no es hoy, cualquier horario de inicio futuro es válido
                    h.FechaHorario > DateOnly.FromDateTime(DateTime.Now)
                    )) // Hasta 2 semanas después)
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


        // POST: EventoController/Cancel/5
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
            return RedirectToAction(nameof(Index));
        }

        // POST: EventoController/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var evento = await _context.Eventos.FindAsync(id);
            if (evento != null)
            {
                var descripcionAuditoria = $"El usuario ha eliminado un evento. Detalles, ID del evento: {evento.IdEvento}.";
                _context.Eventos.Remove(evento);
                await _context.SaveChangesAsync();
                await _auditoriaService.RegistrarAuditoriaAsync(
                seccion: "Administración",
                descripcion: descripcionAuditoria,
                idAccion: 3);
            }
            
            return RedirectToAction(nameof(Index));
        }



        // GET: EventoController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: EventoController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(EventoCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Crear el nuevo evento
                var nuevoEvento = new Evento 
                { 
                    NombreEvento = model.NombreEvento,
                    DescripcionEvento = model.DescripcionEvento,
                    CorreoClienteEvento = model.CorreoClienteEvento,
                    TelefonoClienteEvento = model.TelefonoClienteEvento,
                    IdHorarioDisponible = model.IdHorarioDisponible,
                    IdEstadoReserva = 2 // CONFIRMADA
                };
                var descripcionAuditoria = $"El usuario ha creado nuevo evento. Detalles, ID del evento: {nuevoEvento.IdEvento}.";
                _context.Eventos.Add(nuevoEvento);
                await _context.SaveChangesAsync();
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

                // Si todo va bien, redirigir a otra vista (por ejemplo, la lista de roles)
                return RedirectToAction("Index", "Evento");
            }
            // Si el modelo no es válido, regresa a la vista de creación con errores
            return View(model);

        }



    }
}
