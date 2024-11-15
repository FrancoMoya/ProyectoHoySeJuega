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
namespace ProyectoHsj_Beta.Controllers
{
    public class HorarioController : Controller
    {
        private readonly HoySeJuegaContext _context;
        public HorarioController(HoySeJuegaContext context)
        {
            _context = context;
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
            DateTime finDeSemana = fechaActual.AddDays(6);

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
            var horarios = await _context.HorarioDisponibles.ToListAsync();
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

                TempData["Message"] = "Horario creado exitosamente.";
                return RedirectToAction("ManageView");
            }

            return View(model);
        }

        public async Task<IActionResult> EditarHorario(int id)
        {
            var horarioDb = await _context.HorarioDisponibles.FindAsync(id);
            if (horarioDb == null) return NotFound();

            // Mapear el modelo de la base de datos al ViewModel
            var viewModel = new HorarioViewModel
            {
                IdHorarioDisponible = horarioDb.IdHorarioDisponible,
                FechaHorario = horarioDb.FechaHorario,
                HoraInicio = horarioDb.HoraInicio,
                HoraFin = horarioDb.HoraFin,
                DisponibleHorario = horarioDb.DisponibleHorario ?? false // Valor por defecto si es null
            };

            return View(viewModel);
        }
        [HttpPost]
        public async Task<IActionResult> EditarHorario(HorarioViewModel model)
        {
            if (ModelState.IsValid)
            {
                var horarioDb = await _context.HorarioDisponibles.FindAsync(model.IdHorarioDisponible);
                if (horarioDb == null) return NotFound();

                // Mapear del ViewModel al modelo de la base de datos
                horarioDb.FechaHorario = model.FechaHorario;
                horarioDb.HoraInicio = model.HoraInicio;
                horarioDb.HoraFin = model.HoraFin;
                horarioDb.DisponibleHorario = model.DisponibleHorario;

                await _context.SaveChangesAsync();
                TempData["Message"] = "Horario actualizado exitosamente.";
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

        public async Task<IActionResult> EliminarTodosLosHorarios()
        {
            var horarios = _context.HorarioDisponibles.ToList();
            var reservasAsociadas = await _context.Reservas
            .ToListAsync();
            if (reservasAsociadas.Any())
            {
                // Aquí puedes decidir qué hacer: mostrar un mensaje, eliminar las reservas, etc.
                TempData["ErrorMessage"] = "No se puede eliminar los horarios porque hay reservas asociadas.";
                return RedirectToAction("ManageView");
            }
            if (horarios.Any())
            {
                _context.HorarioDisponibles.RemoveRange(horarios);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Todos los horarios han sido eliminados.";
            }
            else
            {
                TempData["Message"] = "No hay horarios para eliminar.";
            }
            return RedirectToAction("ManageView");
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
                _context.HorarioDisponibles.Remove(horario);
                await _context.SaveChangesAsync();
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
