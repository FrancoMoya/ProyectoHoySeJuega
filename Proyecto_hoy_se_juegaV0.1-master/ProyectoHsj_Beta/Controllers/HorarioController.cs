using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoHsj_Beta.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProyectoHsj_Beta.ViewsModels;
using ProyectoHsj_Beta.Services;
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

        // GET: HorarioController

        public ActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> GenerarFechasYHorariosSemana()
        {
            var horarios = new List<(TimeOnly Inicio, TimeOnly Fin)>();
            for (int hora = 12; hora < 23; hora++)
            {
                horarios.Add((new TimeOnly(hora, 0), new TimeOnly(hora + 1, 0)));
            }

            DateTime fechaActual = DateTime.Now.Date;
            DateTime finDeSemana = fechaActual.AddDays(13);

            var horariosAGuardar = new List<HorarioDisponible>();
            for (DateTime fecha = fechaActual; fecha <= finDeSemana; fecha = fecha.AddDays(1))
            {
                foreach (var horario in horarios)
                {
                    bool existe = await _context.HorarioDisponibles.AnyAsync(h =>
                        h.FechaHorario == DateOnly.FromDateTime(fecha) &&
                        h.HoraInicio == horario.Inicio &&
                        h.HoraFin == horario.Fin);

                    if (!existe)
                    {
                        bool disponible = !(horario.Inicio == new TimeOnly(19, 0) && horario.Fin == new TimeOnly(20, 0));

                        horariosAGuardar.Add(new HorarioDisponible
                        {
                            IdCancha = 1,
                            FechaHorario = DateOnly.FromDateTime(fecha),
                            HoraInicio = horario.Inicio,
                            HoraFin = horario.Fin,
                            DisponibleHorario = disponible
                        });
                    }
                }
            }

            if (horariosAGuardar.Any())
            {
                _context.HorarioDisponibles.AddRange(horariosAGuardar);
                await _context.SaveChangesAsync();
                await _auditoriaService.RegistrarAuditoriaAsync(
                seccion: "Administración",
                descripcion: "El usuario ha generado nuevos horarios semanales.",
                idAccion: 1);
                TempData["Message"] = "Horarios generados exitosamente para la semana.";
            }
            else
            {
                TempData["Message"] = "No se generaron horarios porque ya existen.";
            }

            return RedirectToAction("ManageView", "Horario");
        }

        public async Task<IActionResult> GenerarFechasYHorariosDelMes()
        {
            var horarios = new List<(TimeOnly Inicio, TimeOnly Fin)>
            {
                (new TimeOnly(9, 0), new TimeOnly(11, 0)),
                (new TimeOnly(11, 30), new TimeOnly(13, 30)),
                (new TimeOnly(15, 0), new TimeOnly(17, 0)),
                (new TimeOnly(17, 30), new TimeOnly(19, 30))
            };

            DateTime fechaActual = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime ultimoDiaDelMes = fechaActual.AddMonths(1).AddDays(-1);

            if (fechaActual.Month != DateTime.Now.Month)
            {
                TempData["Message"] = "No se puede generar horarios para un mes diferente al actual.";
                return RedirectToAction("ManageView", "Horario");
            }

            var horariosAGuardar = new List<HorarioDisponible>();
            //recorre cada dia del mes
            for (DateTime fecha = fechaActual; fecha <= ultimoDiaDelMes; fecha = fecha.AddDays(1))
            {
                foreach (var horario in horarios)
                {
                    // Verificar si ya existe el horario para esa fecha
                    bool existe = await _context.HorarioDisponibles.AnyAsync(h =>
                        h.FechaHorario == DateOnly.FromDateTime(fecha) &&
                        h.HoraInicio == horario.Inicio &&
                        h.HoraFin == horario.Fin);

                    if (!existe)
                    {
                        horariosAGuardar.Add(new HorarioDisponible
                        {
                            IdCancha = 1,
                            FechaHorario = DateOnly.FromDateTime(fecha),
                            HoraInicio = horario.Inicio,
                            HoraFin = horario.Fin,
                            DisponibleHorario = true
                        });
                    }
                }
            }

            if (horariosAGuardar.Any())
            {
                _context.HorarioDisponibles.AddRange(horariosAGuardar);
                await _context.SaveChangesAsync();
                await _auditoriaService.RegistrarAuditoriaAsync(
                seccion: "Administración",
                descripcion: "El usuario ha generado nuevos horarios mensuales.",
                idAccion: 1);
                TempData["Message"] = "Horarios generados exitosamente.";
            }
            else
            {
                TempData["Message"] = "No se generaron horarios porque ya existen.";
            }

            return RedirectToAction("ManageView", "Horario");
        }

        public async Task<IActionResult> ManageView()
        {
            // Ordena los horarios por FechaHorario DESC y luego por HoraInicio DESC
            var horarios = await _context.HorarioDisponibles
                .OrderByDescending(h => h.FechaHorario)
                .ThenByDescending(h => h.HoraInicio)
                .ToListAsync();

            return View(horarios);
        }


        [HttpGet]
        public IActionResult CreateHorario()
        {
            // Muestra el formulario para crear un nuevo horario.
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateHorario(HoraViewModel model)
        {
            if (ModelState.IsValid)
            {
                var horario = new HorarioDisponible
                {
                    IdCancha = model.IdCancha,
                    FechaHorario = DateOnly.FromDateTime(model.FechaHorario),
                    HoraInicio = model.HoraInicio,
                    HoraFin = model.HoraFin,
                    DisponibleHorario = model.DisponibleHorario
                };
                _context.HorarioDisponibles.Add(horario);
                await _context.SaveChangesAsync();
                await _auditoriaService.RegistrarAuditoriaAsync(
                seccion: "Administración",
                descripcion: "El usuario ha generado un nuevo horario.",
                idAccion: 1);
                TempData["Message"] = "Horario creado exitosamente.";
                return RedirectToAction("ManageView");
            }

            return View(model);
        }

        public async Task<IActionResult> Search(DateTime? fecha, TimeOnly? horaInicio)
        {
            var horarios = _context.HorarioDisponibles.AsQueryable();

            if (fecha.HasValue)
            {
                horarios = horarios.Where(h => h.FechaHorario == DateOnly.FromDateTime(fecha.Value));
            }

            if (horaInicio.HasValue)
            {
                horarios = horarios.Where(h => h.HoraInicio == horaInicio);
            }

            var resultado = await horarios.ToListAsync();
            return View("ManageView", resultado); // Reutilizamos la vista 'ManageView' para mostrar los resultados de búsqueda
        }

        public async Task<IActionResult> EliminarHorariosDeLaSemana()
        {
            // Obtener la fecha actual y el rango de dos semanas (fecha actual hasta 13 días después)
            DateTime fechaActual = DateTime.Now.Date;
            DateTime finDeSemana = fechaActual.AddDays(13); // Rango de dos semanas

            // Eliminar los horarios dentro de este rango de fechas
            var horariosAEliminar = await _context.HorarioDisponibles
                .Where(h => h.FechaHorario >= DateOnly.FromDateTime(fechaActual) && h.FechaHorario <= DateOnly.FromDateTime(finDeSemana))
                .ToListAsync();

            if (horariosAEliminar.Any())
            {
                _context.HorarioDisponibles.RemoveRange(horariosAEliminar);
                await _context.SaveChangesAsync();
                await _auditoriaService.RegistrarAuditoriaAsync(
                seccion: "Administración",
                descripcion: "El usuario ha eliminado los horarios de la semana.",
                idAccion: 3);
                TempData["Message"] = "Horarios de la semana eliminados exitosamente.";
            }
            else
            {
                TempData["Message"] = "No se encontraron horarios para eliminar en el rango de las dos semanas.";
            }

            return RedirectToAction("ManageView", "Horario");
        }

        public async Task<IActionResult> EliminarHorario(int id)
        {
            var horario = await _context.HorarioDisponibles.FindAsync(id);
            var reservasAsociadas = await _context.Reservas
            .Where(r => r.IdHorarioDisponible == id)
            .ToListAsync();

            if (reservasAsociadas.Any())
            {
                // Aquí puedes decidir qué hacer: mostrar un mensaje, eliminar las reservas, etc.
                TempData["ErrorMessage"] = "No se puede eliminar el horario porque hay reservas asociadas.";
                return RedirectToAction("ManageView");
            }
            if (horario != null)
            {
                var descripcionAuditoria = $"El usuario ha eliminado un horario. Detalles, ID del horario: {horario.IdHorarioDisponible}.";
                _context.HorarioDisponibles.Remove(horario);
                await _context.SaveChangesAsync();
                await _auditoriaService.RegistrarAuditoriaAsync(
                seccion: "Administración",
                descripcion: descripcionAuditoria,
                idAccion: 3);
                TempData["Message"] = "El horario fue eliminado correctamente.";
            }
            else
            {
                TempData["Message"] = "El horario no se encontró.";
            }
            return RedirectToAction("ManageView");
        }
    }
}