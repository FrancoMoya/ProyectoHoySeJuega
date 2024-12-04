using Microsoft.AspNetCore.Mvc;
using ProyectoHsj_Beta.Models;
using Microsoft.EntityFrameworkCore;
using ProyectoHsj_Beta.ViewsModels;
using ProyectoHsj_Beta.Services;
using Microsoft.AspNetCore.Authorization;
namespace ProyectoHsj_Beta.Controllers
{
    public class HorarioController : Controller
    {
        private readonly HoySeJuegaContext _context;
        private readonly AuditoriaService _auditoriaService;
        public HorarioController(HoySeJuegaContext context, AuditoriaService auditoriaService)
        {
            _context = context;
            _auditoriaService = auditoriaService;
        }

        [Authorize(Policy = "AdminOrEmployed")]
        public async Task<IActionResult> GenerarFechasYHorariosMes(int mes, int año)
        {
            // Obtener el mes y año actuales
            DateTime fechaActual = DateTime.Now;
            int mesActual = fechaActual.Month;
            int añoActual = fechaActual.Year;

            // Validamos si el mes y año proporcionado es menor que el mes y año actuales
            if (año < añoActual || (año == añoActual && mes < mesActual))
            {
                TempData["ErrorMessage"] = "No se pueden crear horarios para meses anteriores al actual.";
                return RedirectToAction("ManageView", "Horario");
            }

            var horarios = new List<(TimeOnly Inicio, TimeOnly Fin)>();

            // Generamos los horarios de 12:00 a 12:59, 13:00 a 13:59, ..., 23:00 a 23:59
            for (int hora = 12; hora < 24; hora++)
            {
                horarios.Add((new TimeOnly(hora, 0), new TimeOnly(hora, 59)));
            }

            // Calculamos el primer y último día del mes
            DateTime fechaInicioMes = new DateTime(año, mes, 1);
            DateTime fechaFinMes = fechaInicioMes.AddMonths(1).AddDays(-1);  // Último día del mes

            // Verificamos si ya existen horarios en el mes seleccionado
            bool existenHorarios = await _context.HorarioDisponibles
                .AnyAsync(h => h.FechaHorario >= DateOnly.FromDateTime(fechaInicioMes) && h.FechaHorario <= DateOnly.FromDateTime(fechaFinMes));

            if (existenHorarios)
            {
                // Si ya existen horarios para cualquier día en el mes, no generamos nuevos
                TempData["ErrorMessage"] = "Se han encontrado horarios dentro del mes seleccionado, por lo que no fue posible crear otros, por favor intente seleccionando otro mes.";
                return RedirectToAction("ManageView", "Horario");
            }

            // Lista para guardar los nuevos horarios
            var horariosAGuardar = new List<HorarioDisponible>();

            // Usamos una transacción para asegurarnos de que todos los horarios se agreguen o ninguno
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Iteramos sobre cada día del mes
                    for (DateTime fecha = fechaInicioMes; fecha <= fechaFinMes; fecha = fecha.AddDays(1))
                    {
                        foreach (var horario in horarios)
                        {
                            // Agregamos el horario a la lista de horarios para guardar
                            horariosAGuardar.Add(new HorarioDisponible
                            {
                                IdCancha = 1,  // Cambia según tu lógica
                                FechaHorario = DateOnly.FromDateTime(fecha),
                                HoraInicio = horario.Inicio,
                                HoraFin = horario.Fin,
                                DisponibleHorario = true  // Horarios disponibles
                            });
                        }
                    }

                    // Si tenemos horarios para guardar, los insertamos
                    if (horariosAGuardar.Any())
                    {
                        await _context.HorarioDisponibles.AddRangeAsync(horariosAGuardar);
                        await _context.SaveChangesAsync();

                        // Registrar auditoría
                        await _auditoriaService.RegistrarAuditoriaAsync(
                            seccion: "Administración",
                            descripcion: "El usuario ha generado nuevos horarios mensuales.",
                            idAccion: 1);

                        // Confirmar la transacción
                        await transaction.CommitAsync();

                        TempData["Message"] = "Horarios generados exitosamente para el mes seleccionado.";
                        TempData["MessageDetails"] = $"Se generaron {horariosAGuardar.Count} horarios.";
                    }
                    else
                    {
                        // Si no hay horarios para guardar
                        TempData["ErrorMessage"] = "No se generaron horarios, porque ya existen.";
                    }
                }
                catch (Exception ex)
                {
                    // Si ocurre algún error, hacemos un rollback de la transacción
                    await transaction.RollbackAsync();
                    TempData["ErrorMessage"] = "Hubo un error al generar los horarios: " + ex.Message;
                }
            }

            return RedirectToAction("ManageView", "Horario");
        }

        [Authorize(Policy = "AdminOrEmployed")]
        public async Task<IActionResult> ManageView()
        {
            // Obtener todos los horarios disponibles
            var horarios = await _context.HorarioDisponibles.ToListAsync();

            // Fecha actual para comparar
            var fechaActual = DateTime.Now;

            // Filtrar los horarios para solo incluir aquellos que son del mes y año actual o posterior
            var filteredHorarios = horarios
                .Where(h => h.FechaHorario.Year > fechaActual.Year ||
                            (h.FechaHorario.Year == fechaActual.Year && h.FechaHorario.Month >= fechaActual.Month))
                .ToList();

            // Agrupar los horarios por mes y año, y determinar si hay disponibilidad
            var monthAvailabilityList = filteredHorarios
                .GroupBy(h => new { h.FechaHorario.Year, h.FechaHorario.Month })
                .Select(g => new MonthAvailability
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    HasAvailableHours = g.Any(h => (bool)h.DisponibleHorario)  // Si hay horarios disponibles en el mes
                })
                .OrderBy(m => new DateTime(m.Year, m.Month, 1))  // Ordenar por año y mes
                .ToList();

            // Pasar la lista de MonthAvailability a la vista
            return View(monthAvailabilityList);
        }
        [Authorize(Policy = "AdminOrEmployed")]
        public IActionResult HorariosPorFecha()
        {
            return View();
        }
        [Authorize(Policy = "AdminOrEmployed")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ListarHorariosPorFecha(DateOnly Fecha)
        {
            var horarios = await _context.HorarioDisponibles
                .Where(h => h.FechaHorario == Fecha)
                .Select(h => new
                {
                    h.IdHorarioDisponible,
                    h.FechaHorario,
                    Horario = $"{h.HoraInicio} - {h.HoraFin}",
                    h.DisponibleHorario,
                    TieneReservas = h.Reservas.Count != 0
                })
                .ToListAsync();

            return Json(horarios);
        }

        [Authorize(Policy = "AdminOrEmployed")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarDisponibilidadHorario(int idHorarioDisponible, bool activar)
        {
            var horario = await _context.HorarioDisponibles
                .Include(h => h.Reservas)
                .Include(h => h.Eventos)
                .FirstOrDefaultAsync(h => h.IdHorarioDisponible == idHorarioDisponible);

            if (horario == null)
            {
                return Json(new { success = false, message = "Horario no encontrado." });
            }
            // Verificar si hay reservas asociadas en estado CONFIRMADA
            bool TieneReservaConfirmada = horario.Reservas.Any(r => r.IdEstadoReserva != 3);
            bool TieneEventoConfirmado = horario.Eventos.Any(e => e.IdEstadoReserva != 3);

            if (activar && TieneReservaConfirmada)
            {
                return Json(new { success = false, message = "No ha sido posible activar el horario ya que cuenta con una RESERVA CLIENTE asociada." });
            }
            if (activar && TieneEventoConfirmado)
            {
                return Json(new { success = false, message = "No ha sido posible activar el horario ya que cuenta con una RESERVA ADMIN asociada." });
            }

            horario.DisponibleHorario = activar;
            _context.HorarioDisponibles.Update(horario);
            await _context.SaveChangesAsync();

            string accion = activar ? "ACTIVADO" : "DESACTIVADO";
            await _auditoriaService.RegistrarAuditoriaAsync(
                seccion: "Administración",
                descripcion: $"El usuario ha {accion} el horario con ID: {idHorarioDisponible}.",
                idAccion: 2
            );

            string mensaje = activar
                ? "Horario activado exitosamente."
                : "Horario desactivado exitosamente.";

            return Json(new { success = true, message = mensaje });
        }

    }
}