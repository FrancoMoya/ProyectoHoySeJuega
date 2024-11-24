using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoHsj_Beta.Models;
using ProyectoHsj_Beta.Services;
using ProyectoHsj_Beta.ViewsModels;

namespace ProyectoHsj_Beta.Controllers
{
    public class EventosFijosController : Controller
    {
        private readonly HoySeJuegaContext _context;
        private readonly AuditoriaService _auditoriaService;
        public EventosFijosController(HoySeJuegaContext context, AuditoriaService auditoriaService)
        {
            _context = context;
            _auditoriaService = auditoriaService;
        }

        [Authorize(Policy = "AdminOrEmployed")]
        public IActionResult CrearFijos()
        {
            // Aquí puedes cargar cualquier lista que necesites para mostrar en los formularios
            ViewData["DiasSemana"] = Enum.GetValues(typeof(DayOfWeek)); // Muestra los días de la semana
            return View();
        }

        [Authorize(Policy = "AdminOrEmployed")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearFijos(EventoRecurrente modeloEvento)
        {
            if (ModelState.IsValid)
            {
                // Verificar que los horarios estén disponibles para las fechas solicitadas
                var fechaInicio = modeloEvento.FechaInicio;
                var fechaFin = modeloEvento.FechaFin;

                // Verifica que la fecha de fin no esté antes que la fecha de inicio
                if (fechaFin < fechaInicio)
                {
                    TempData["Message"] = "La fecha de fin no puede ser anterior a la fecha de inicio.";
                    return RedirectToAction("CrearFijos");
                }

                // Verificar que el día seleccionado está dentro del rango de fechas
                if (fechaInicio > fechaFin)
                {
                    TempData["Message"] = "La fecha de inicio no puede ser posterior a la fecha de fin.";
                    return RedirectToAction("CrearFijos");
                }

                List<DateOnly> fechasEvento = new List<DateOnly>();
                bool diaValido = false;
                for (var fecha = fechaInicio; fecha <= fechaFin; fecha = fecha.AddDays(1))
                {
                    if (fecha.DayOfWeek == (DayOfWeek)modeloEvento.DiaSemana)
                    {
                        fechasEvento.Add(fecha);
                        diaValido = true;  // Si encontramos un día válido dentro del rango
                    }
                }

                // Si no encontramos ningún día válido dentro del rango, mostrar error
                if (!diaValido)
                {
                    TempData["Message"] = "El día seleccionado no se encuentra dentro del rango de fechas especificado.";
                    return RedirectToAction("CrearFijos");
                }

                // Crear el evento recurrente
                var eventoRecurrente = new EventoRecurrente
                {
                    Nombre = modeloEvento.Nombre.ToUpper(),
                    Descripcion = modeloEvento.Descripcion,
                    CorreoCliente = modeloEvento.CorreoCliente,
                    TelefonoCliente = modeloEvento.TelefonoCliente,
                    HoraInicio = modeloEvento.HoraInicio,
                    DiaSemana = modeloEvento.DiaSemana,
                    FechaInicio = modeloEvento.FechaInicio,
                    FechaFin = modeloEvento.FechaFin
                };

                // Comenzar la transacción
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        _context.EventoRecurrentes.Add(eventoRecurrente);
                        await _context.SaveChangesAsync();

                        // Asignar horarios disponibles para esas fechas
                        foreach (var fecha in fechasEvento)
                        {
                            var horario = await _context.HorarioDisponibles
                                .FirstOrDefaultAsync(h => h.FechaHorario == fecha && h.HoraInicio == modeloEvento.HoraInicio && h.DisponibleHorario == true);

                            if (horario == null)
                            {
                                TempData["Message"] = "No hay horarios disponibles suficientes para el rango de fechas seleccionado, intente seleccionando un rango menor.";
                                await transaction.RollbackAsync();  // Hacer rollback de la transacción
                                return RedirectToAction("CrearFijos");
                            }

                            var fechaActual = DateTime.Now.AddHours(3); // Aumenta 3 horas

                            // Crear el evento asociado con ese horario
                            var evento = new Evento
                            {
                                NombreEvento = eventoRecurrente.Nombre,
                                DescripcionEvento = eventoRecurrente.Descripcion,
                                CorreoClienteEvento = eventoRecurrente.CorreoCliente,
                                TelefonoClienteEvento = Convert.ToInt32(eventoRecurrente.TelefonoCliente),
                                FechaEvento = fechaActual,
                                IdHorarioDisponible = horario.IdHorarioDisponible,
                                IdEstadoReserva = 2
                            };

                            _context.Eventos.Add(evento);
                            horario.DisponibleHorario = false; // Marcar el horario como no disponible
                        }
                        var descripcionAuditoria = $"El usuario ha creado una nueva RESERVA FIJA. Detalles de las nuevas reservas creadas, Título: {eventoRecurrente.Nombre}, Descripción: {eventoRecurrente.Descripcion}.";
                        await _context.SaveChangesAsync();
                        await _auditoriaService.RegistrarAuditoriaAsync(
                        seccion: "Administración",
                        descripcion: descripcionAuditoria,
                        idAccion: 1);

                        // Confirmar la transacción si todo sale bien
                        await transaction.CommitAsync();

                        return RedirectToAction("Index"); // Redirigir al listado de Reservas fijas
                    }
                    catch (Exception ex)
                    {
                        // Si algo falla, hacer rollback de la transacción
                        await transaction.RollbackAsync();
                        TempData["Message"] = "Ocurrió un error al procesar la solicitud: " + ex.Message;
                        return RedirectToAction("CrearFijos");
                    }
                }
            }

            // Si algo salió mal, retornar la vista con los errores
            ViewData["DiasSemana"] = Enum.GetValues(typeof(DayOfWeek)); // Asegúrate de pasar de nuevo los valores
            return View(modeloEvento);
        }

        [Authorize(Policy = "AdminOrEmployed")]
        public async Task<IActionResult> Index()
        {
            var fijosCreados = await _context.Set<FijosAdminGetViewModel>()
                .FromSqlRaw("EXEC SP_GET_FIJOS_CREADOS_ADMIN")
                .ToListAsync();
            return View(fijosCreados);
        }
    }
}
