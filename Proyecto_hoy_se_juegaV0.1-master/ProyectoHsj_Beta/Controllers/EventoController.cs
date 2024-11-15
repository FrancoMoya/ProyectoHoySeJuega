using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ProyectoHsj_Beta.Models;
using ProyectoHsj_Beta.Services;
using ProyectoHsj_Beta.ViewsModels;
using System.Data;

namespace ProyectoHsj_Beta.Controllers
{
    public class EventoController : Controller
    {
        private readonly HoySeJuegaContext _context;
        public EventoController(HoySeJuegaContext context)
        {
            _context = context;
        }

        // GET: EventoController
        public async Task<IActionResult> Index()
        {
            var eventos = await _context.Set<EventosAdminGetViewModel>()
                .FromSqlRaw("EXEC SP_GET_EVENTOS_ADMIN")
                .ToListAsync();
            return View(eventos);
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
                if (horario != null)
                {
                    horario.DisponibleHorario = true;
                    // Guardar los cambios en la tabla HorariosDisponibles
                    await _context.SaveChangesAsync();
                }
                var estado = evento.IdEstadoReserva;
                if (estado != null)
                {
                    evento.IdEstadoReserva = 3;
                    // Cambiar a estado cancelado
                    await _context.SaveChangesAsync();
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
                _context.Eventos.Remove(evento);
            }

            await _context.SaveChangesAsync();
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

                _context.Eventos.Add(nuevoEvento);
                await _context.SaveChangesAsync(); // Guarda el rol y genera su ID

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
