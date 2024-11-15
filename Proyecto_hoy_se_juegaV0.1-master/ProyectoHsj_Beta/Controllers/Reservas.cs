using Microsoft.AspNetCore.Mvc;
using ProyectoHsj_Beta.Models;
using System.Threading.Tasks;
using ProyectoHsj_Beta.DTO;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ProyectoHsj_Beta.ViewsModels;
using ProyectoHsj_Beta.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using static ProyectoHsj_Beta.DTO.ReservationPostDTO;
using Microsoft.Data.SqlClient;
using System.Data;
using Newtonsoft.Json.Linq;
namespace ProyectoHsj_Beta.Controllers
{
    public class Reservas : Controller
    {
        private readonly HoySeJuegaContext _context;
        private readonly MercadoPagoService _MercadoPagoService;
        public Reservas(HoySeJuegaContext context, MercadoPagoService mercadoPagoService)
        {
            _context = context;
            _MercadoPagoService = mercadoPagoService;
        }

        

        public IActionResult Reservar()
        {
            return View();
        }

        // CALENDARIO
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReservaCreate([FromBody] ReservaRequestID request)
        {
            // Verifica si el usuario está autenticado
            if (User.Identity.IsAuthenticated)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Verifica que el objeto request no sea nulo y que el IdHorarioDisponible sea válido
                if (request != null && request.IdHorarioDisponible > 0)
                {
                    //Verifica que el horario siga disponible antes de reservar
                    var horario = await _context.HorarioDisponibles
                        .Where(h => h.IdHorarioDisponible == request.IdHorarioDisponible)
                        .FirstOrDefaultAsync();
                    if (horario != null & horario.DisponibleHorario == true)
                    {
                        try
                        {
                            // Variable para recibir el ID de la reserva
                            var parametros = new SqlParameter[]
                            {
                                new SqlParameter("@ID_usuario", userId),
                                new SqlParameter("@ID_horario_disponible", request.IdHorarioDisponible),
                                new SqlParameter("@ID_reserva", SqlDbType.Int) { Direction = ParameterDirection.Output }
                            };

                            // Ejecuta el procedimiento almacenado
                            await _context.Database.ExecuteSqlRawAsync("EXEC SP_RESERVA_CREATE @ID_usuario, @ID_horario_disponible, @ID_reserva OUT", parametros);

                            // Captura el ID de la reserva creada
                            var idReserva = (int)parametros[2].Value;

                            // Devuelve una respuesta JSON de éxito con el ID de la reserva
                            return Json(new { success = true, message = "Reserva confirmada correctamente", idReserva });
                        }
                        catch (Exception ex)
                        {
                            // Maneja cualquier error de ejecución de la base de datos
                            return Json(new { success = false, message = "Ocurrió un error al crear la reserva: " + ex.Message });
                        }
                    }
                    else
                    {
                        return Json(new { success = false, message = "El horario seleccionado ya no está disponible" });
                    }

                }
                else
                {
                    return Json(new { success = false, message = "Los datos proporcionados no son válidos." });
                }
            }
            else
            {
                // Si el usuario no está autenticado, redirige a la página de login
                return Json(new { success = false, message = "Usuario no autenticado." });
            }
        }

        //GET: Reservas/MisReservas
        [HttpGet]
        public async Task<IActionResult> MisReservas()
        {
            if (User.Identity.IsAuthenticated)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Obtiene el ID del usuario autenticado
                try
                {
                    var misReservas = await _context.Set<MisReservasGetViewModel>()
                        .FromSqlRaw("EXEC SP_GET_MIS_RESERVAS @ID_usuario = {0}", userId)
                        .ToListAsync();

                    if (misReservas == null || misReservas.Count == 0)
                    {
                        return View(new List<MisReservasGetViewModel>());
                    }

                    return View(misReservas);
                }
                catch (Exception ex)
                {
                    // Log del error para ver el mensaje detallado
                    Console.WriteLine("Error al obtener reservas: " + ex.Message);
                    return Json(new { success = false, message = "Ocurrió un error al obtener las reservas: " + ex.Message });
                }
            }
            else
            {
                return Json(new { success = false, message = "Usuario no autenticado." });
            }
        }

        // GET: Reservas/Create

        // POST: Reservas/Create
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ReservaViewModel reserva)
        {
            if (reserva.Fecha == null || reserva.Fecha == DateTime.MinValue)
            {
                return BadRequest("Fecha no válida.");
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized("Usuario no autenticado.");
            }
            int idUsuario = int.Parse(userIdClaim);

            // Buscar el horario específico seleccionado por el usuario
            var horario = await _context.HorarioDisponibles
                .FirstOrDefaultAsync(h => h.IdHorarioDisponible == reserva.IdHorarioDisponible && h.DisponibleHorario == true);

            if (horario == null)
            {
                Console.WriteLine("Paso por el null de horario");
                return BadRequest("No hay horario disponible para esa fecha.");
            }

            var nuevaReserva = new Reserva
            {
                FechaReserva = reserva.Fecha,
                IdHorarioDisponible = horario.IdHorarioDisponible,
                IdEstadoReserva = 1, // Activa
                IdUsuario = idUsuario
            };

            // Marcar el horario como no disponible
            horario.DisponibleHorario = false;
            Console.WriteLine($"este es el id de fecha: {horario.IdHorarioDisponible} ");
            _context.HorarioDisponibles.Update(horario);

            _context.Reservas.Add(nuevaReserva);
            await _context.SaveChangesAsync();
            return Json(new { success = true, idReserva = nuevaReserva.IdReserva });
        }
        public async Task<IActionResult> Index()
        {
            ViewData["ID_horario_disponible"] = new SelectList(await _context.HorarioDisponibles.ToListAsync(), "Id", "HoraInicio");
            return View();
        }

        /*public async Task<IActionResult> GetReservas()
        {
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)); // Asegúrate de obtener el ID correctamente
            var isAdmin = User.IsInRole("admin");

            // Cargamos las reservas desde la base de datos
            var reservasDb = await _context.Reservas
                .Where(r => r.FechaReserva.HasValue)
                .ToListAsync();
            var reservas = reservasDb.Select(r => new Evento
            {
                id = r.IdReserva,
                title = "Reserva " + r.IdReserva,
                start = r.FechaReserva.Value.ToString("yyyy-MM-ddTHH:mm"),
                end = r.FechaReserva.Value.AddHours(1).ToString("yyyy-MM-ddTHH:mm"),
                creadorId = r.IdUsuario,
                color = ObtenerColorEstado((int)r.IdEstadoReserva),
                estado = ObtenerEstado((int)r.IdEstadoReserva)
            })
            .Where(e => e.estado != "CANCELADA" || e.creadorId == currentUserId || isAdmin) // Filtra según visibilidad
            .ToList();

            var horariosDisponibles = await _context.HorarioDisponibles
                .Where(h => h.DisponibleHorario == true)
                .Select(h => new Evento
                {
                    id = h.IdHorarioDisponible,
                    title = "Disponible",
                    start = new DateTime(
                        h.FechaHorario.Year,
                        h.FechaHorario.Month,
                        h.FechaHorario.Day,
                        h.HoraInicio.Hour,
                        h.HoraInicio.Minute,
                        0).ToString("yyyy-MM-ddTHH:mm"),
                    end = new DateTime(
                        h.FechaHorario.Year,
                        h.FechaHorario.Month,
                        h.FechaHorario.Day,
                        h.HoraFin.Hour,
                        h.HoraFin.Minute,
                        0).ToString("yyyy-MM-ddTHH:mm"),
                    color = "green",
                    estado = "Disponible"
                })
                .ToListAsync();

            // Ahora puedes concatenar ambas listas sin problemas
            var eventos = reservas.Concat(horariosDisponibles).ToList();

            return Json(eventos);
        }*/

        private string ObtenerEstado(int estadoId) =>
            estadoId switch
            {
                1 => "PENDIENTE",
                2 => "CONFIRMADA",
                3 => "CANCELADA",
                _ => "DESCONOCIDO"
            };

        private string ObtenerColorEstado(int estadoId) =>
            estadoId switch
            {
                1 => "yellow",
                2 => "green",
                3 => "gray",
                _ => "black"
            };
        //Obtener y renderizar los horarios
        [HttpGet]
        public async Task<IActionResult> GetAvailableTimeSlots(DateTime fecha) //Modificado para incluir solo los horarios dentro de 2 semanas
        {
            DateOnly fechaLimite = DateOnly.FromDateTime(DateTime.Now.AddDays(14));
            TimeOnly horaLimite = TimeOnly.FromDateTime(DateTime.Now);
            var horarios = await _context.HorarioDisponibles
                .Where(h => (h.DisponibleHorario ?? false) && h.FechaHorario == DateOnly.FromDateTime(fecha) && h.FechaHorario <= fechaLimite && (
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
        [HttpPost]
        public async Task<IActionResult> PagarReserva(int reservaId, decimal monto)
        {
            Console.WriteLine("La id reserva es :" + reservaId);
            Console.WriteLine("El monto de pago es:" + monto);
            var pago = new Pago
            {
                IdReserva = reservaId,
                MontoPago = monto
            };

            var preferencia = await _MercadoPagoService.CrearPreferenciaDePago(pago);

            // Redirige al usuario al URL de Mercado Pago
            return Redirect(preferencia.InitPoint);
        }

        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            var reserva = await _context.Reservas.FindAsync(id);

            if (reserva == null)
            {
                return NotFound("Reserva no encontrada.");
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || int.Parse(userIdClaim) != reserva.IdUsuario)
            {
                return Forbid("No tienes permiso para cancelar esta reserva.");
            }

            // Marcar la reserva como cancelada
            reserva.IdEstadoReserva = 3;  // Estado: Cancelada
            _context.Reservas.Update(reserva);

            // Liberar el horario asociado
            var horario = await _context.HorarioDisponibles
                .FirstOrDefaultAsync(h => h.IdHorarioDisponible == reserva.IdHorarioDisponible);
            if (horario != null)
            {
                horario.DisponibleHorario = true;
                _context.HorarioDisponibles.Update(horario);
            }

            await _context.SaveChangesAsync();
            return Ok("Reserva cancelada exitosamente.");
        }

       
       


        [HttpDelete]
        public async Task<IActionResult> DeleteReserva(int id)
        {
            var reserva = await _context.Reservas.FindAsync(id);

            if (reserva == null)
            {
                return NotFound("No se encontró la reserva.");
            }

            if (reserva.IdEstadoReserva != 3) // Estado 2 = CANCELADA
            {
                return BadRequest("Solo se pueden eliminar reservas canceladas.");
            }
            _context.Reservas.Remove(reserva);
            await _context.SaveChangesAsync();

            return Ok("Reserva eliminada correctamente.");
        }

    }
}
