﻿using Microsoft.AspNetCore.Mvc;
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
using Microsoft.AspNetCore.Authorization;
namespace ProyectoHsj_Beta.Controllers
{
    public class Reservas : Controller
    {
        private readonly HoySeJuegaContext _context;
        private readonly MercadoPagoService _MercadoPagoService;
        private readonly AuditoriaService _auditoriaService;
        public Reservas(HoySeJuegaContext context, MercadoPagoService mercadoPagoService, AuditoriaService auditoriaService)
        {
            _context = context;
            _MercadoPagoService = mercadoPagoService;
            _auditoriaService = auditoriaService;
        }

        [Authorize(Policy = "AdminOrEmployed")]
        public async Task<IActionResult> ReservasClientesHistorial()
        {
            var eventos = await _context.Set<ReservasClientesHistorialAdminGetViewModel>()
                .FromSqlRaw("EXEC SP_GET_RESERVAS_CLIENTES_HISTORIAL_ADMIN")
                .ToListAsync();
            return View(eventos);
        }
        [Authorize(Policy = "AdminOrEmployed")]
        public async Task<IActionResult> HistorialReservas()
        {
            var eventos = await _context.Set<AdminHistorialReservasGetViewModel>()
                .FromSqlRaw("EXEC SP_GET_ADMIN_HISTORIAL_RESERVAS")
                .ToListAsync();
            return View(eventos);
        }


        [Authorize(Policy = "UserOnly")]
        public async Task<IActionResult> Reservar()
        {
            var configPago = await _context.ConfiguracionPagos
                        .Select(c => c.MontoSena)
                        .FirstOrDefaultAsync();  // Toma el primer (y único) resultado que cumple la condición

            if (configPago == null)
            {
                configPago = 0; // Valor predeterminado o lanzar un error si prefieres
            }

            string monto = configPago.ToString("N0");
            ViewData["monto"] = monto;

            return View();
        }

        // CALENDARIO
        [Authorize(Policy = "UserOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReservaCreate([FromBody] ReservaRequestID request)
        {
            // Verifica si el usuario está autenticado
            if (User.Identity.IsAuthenticated)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    RedirectToAction("Login", "Acces");
                }
                int user = int.Parse(userId);
                var tieneReservasPendientes = await _context.Reservas
                    .Where(r => r.IdUsuario == user && r.IdEstadoReserva == 1)
                    .ToListAsync();
                if (tieneReservasPendientes.Count > 0)
                {
                    return Json(new { success = false, message = "Usted cuenta con una reserva pendiente de pago. Por favor complete el pago de la misma o vuelva a intentarlo dentro de unos minutos." });
                }
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
                            await _auditoriaService.RegistrarAuditoriaAsync(
                            seccion: "Reserva",
                            descripcion: $"El usuario ha creado una nueva reserva. Detalles, ID: {idReserva}.",
                            idAccion: 1);

                            // Devuelve una respuesta JSON de éxito con el ID de la reserva
                            return Json(new { success = true, message = "Horario reservado correctamente.", idReserva });
                        }
                        catch (Exception) //ex
                        {
                            // Maneja cualquier error de ejecución de la base de datos
                            return Json(new { success = false, message = "Ocurrió un error al crear la reserva." }); //+ex.Message
                        }
                    }
                    else
                    {
                        return Json(new { success = false, message = "El horario seleccionado ya no está disponible, por favor seleccione otro." });
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
                return RedirectToAction("Login", "Acces");
            }
        }

        //GET: Reservas/MisReservas
        [Authorize(Policy = "UserOnly")]
        [HttpGet]
        public async Task<IActionResult> MisReservas()
        {
            if (User.Identity.IsAuthenticated)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Obtiene el ID del usuario autenticado
                if (userId == null)
                {
                    RedirectToAction("Login", "Acces");
                }
                try
                {
                    var misReservas = await _context.Set<MisReservasGetViewModel>()
                        .FromSqlRaw("EXEC SP_GET_MIS_RESERVAS @ID_usuario = {0}", userId)
                        .ToListAsync();

                    if (misReservas == null || misReservas.Count == 0)
                    {
                        return View(new List<MisReservasGetViewModel>());
                    }

                    var celularCancelaciones = await _context.ConfiguracionPagos
                        .Select(c => c.CelularCancelaciones)
                        .FirstOrDefaultAsync(); 

                    if (celularCancelaciones == null)
                    {
                        celularCancelaciones = 1234567890; // Valor predeterminado
                    }

                    string number = celularCancelaciones.ToString();
                    // Pasar las reservas y el número de WhatsApp a la vista
                    ViewData["number"] = number;

                    return View(misReservas);
                }
                catch (Exception) //ex
                {
                    return Json(new { success = false, message = "Ocurrió un error al obtener las reservas."}); // +ex.Message
                }
            }
            else
            {
                return RedirectToAction("Login", "Acces");
            }
        }


        //Obtener y renderizar los horarios
        [Authorize(Policy = "UserOnly")]
        [HttpGet]
        public async Task<IActionResult> GetAvailableTimeSlots(DateTime fecha) //Modificado para incluir solo los horarios dentro de 2 semanas
        {
            DateOnly fechaLimite = DateOnly.FromDateTime(DateTime.Now.AddDays(14).AddHours(3));
            TimeOnly horaLimite = TimeOnly.FromDateTime(DateTime.Now.AddHours(3));
            var horarios = await _context.HorarioDisponibles
                .Where(h => (h.DisponibleHorario ?? false) && h.FechaHorario == DateOnly.FromDateTime(fecha) && h.FechaHorario <= fechaLimite && (
                    // Si es hoy, la hora de inicio debe ser mayor o igual a la hora actual
                    (h.FechaHorario == DateOnly.FromDateTime(DateTime.Now.AddHours(3)) && h.HoraInicio >= horaLimite) ||
                    // Si no es hoy, cualquier horario de inicio futuro es válido
                    h.FechaHorario > DateOnly.FromDateTime(DateTime.Now.AddHours(3))
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

        [Authorize(Policy = "UserOnly")]
        [HttpPost]
        public async Task<IActionResult> PagarReserva(int reservaId, decimal monto)
        {
            var reserva = await _context.Reservas
                                 .FirstOrDefaultAsync(r => r.IdReserva == reservaId);
            if (reserva == null)
            {
                // Si no se encuentra la reserva, mostrar un mensaje de error
                TempData["ErrorMessage"] = "La reserva no existe.";
                return RedirectToAction("MisReservas", "Reservas");
            }
            // Verificamos el estado de la reserva
            if (reserva.IdEstadoReserva != 1)
            {
                // Si el estado de la reserva no es 1, mostrar un mensaje de error
                TempData["ErrorMessage"] = "La reserva ya no está disponible para proceder con el pago.";
                return RedirectToAction("MisReservas", "Reservas");
            }
            var pago = new Pago
            {
                IdReserva = reservaId,
                MontoPago = monto
            };
            reserva.IdEstadoReserva = 4;
            await _context.SaveChangesAsync();
            var preferencia = await _MercadoPagoService.CrearPreferenciaDePago(pago);

            // Redirige al usuario al URL de Mercado Pago
            return Redirect(preferencia.InitPoint);
        }

        //para el calendario
        [Authorize(Policy = "AdminOrEmployed")]
        public IActionResult AllReservationsCalendar()
        {
            return View();
        }
        [Authorize(Policy = "AdminOrEmployed")]
        public async Task<IActionResult> GetReservas()
        {
            var reservas = await _context.Set<ReservasAdminGetViewModel>()
                .FromSqlRaw("EXEC SP_GET_ALL_RESERVAS_CALENDAR_ADMIN")
                .ToListAsync();
            return Json(reservas);
        }

        // TODAS LAS RESERVAS ADMIN
        [Authorize(Policy = "AdminOrEmployed")]
        public async Task<IActionResult> Index()
        {
            var reservas = await _context.Set<ReservasAdminGetAllViewModel>()
                .FromSqlRaw("EXEC SP_GET_ALL_RESERVAS_ADMIN")
                .ToListAsync();
            return View(reservas);
        }

        [Authorize(Policy = "AdminOrEmployed")]
        public async Task<IActionResult> ReservasClientes()
        {
            var eventos = await _context.Set<ReservasClientesAdminGetViewModel>()
                .FromSqlRaw("EXEC SP_GET_RESERVAS_CLIENTES_ADMIN")
                .ToListAsync();
            return View(eventos);
        }


        // CANCELAR RESERVAS DESDE LA VISTA DE ADMIN RESERVAS CLIENTES(Tarjetas)
        [Authorize(Policy = "AdminOrEmployed")]
        [HttpPost, ActionName("CancelReserva")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelConfirmed(int id)
        {
            var reserva = await _context.Reservas.FindAsync(id);
            if (reserva != null)
            {
                var horario = await _context.HorarioDisponibles
                                             .FirstOrDefaultAsync(h => h.IdHorarioDisponible == reserva.IdHorarioDisponible);
                var estado = reserva.IdEstadoReserva;
                if ((horario != null) && (estado != null))
                {
                    var descripcionAuditoria = $"El usuario ha cancelado la reserva de un cliente. Detalles, ID de la reserva: {reserva.IdReserva}.";
                    horario.DisponibleHorario = true;
                    // Guardar los cambios en la tabla HorariosDisponibles
                    reserva.IdEstadoReserva = 3;
                    // Cambiar a estado cancelado
                    await _context.SaveChangesAsync();
                    await _auditoriaService.RegistrarAuditoriaAsync(
                    seccion: "Administración",
                    descripcion: descripcionAuditoria,
                    idAccion: 2);
                }
            }
            return RedirectToAction(nameof(ReservasClientes));
        }
    }
}
