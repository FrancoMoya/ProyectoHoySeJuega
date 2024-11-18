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
        private readonly AuditoriaService _auditoriaService;
        public Reservas(HoySeJuegaContext context, MercadoPagoService mercadoPagoService, AuditoriaService auditoriaService)
        {
            _context = context;
            _MercadoPagoService = mercadoPagoService;
            _auditoriaService = auditoriaService;
        }

        

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
                            await _auditoriaService.RegistrarAuditoriaAsync(
                            seccion: "Reserva",
                            descripcion: $"El usuario ha creado una nueva reserva. Detalles, ID: {idReserva}.",
                            idAccion: 1);

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

                    // Obtener el número de WhatsApp de alguna tabla de configuración
                    var celularCancelaciones = await _context.ConfiguracionPagos
                        .Select(c => c.CelularCancelaciones)
                        .FirstOrDefaultAsync();  // Toma el primer (y único) resultado que cumple la condición

                    // Si no existe el número de WhatsApp en la configuración, asignar un valor predeterminado
                    if (celularCancelaciones == null)
                    {
                        celularCancelaciones = 1234567890; // Valor predeterminado o lanzar un error si prefieres
                    }

                    string number = celularCancelaciones.ToString();
                    // Pasar las reservas y el número de WhatsApp a la vista
                    ViewData["number"] = number;

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

            // REVISAR SI PAGO SE DEBE AGREGAR UN AUDITORIAAAAAA!

            // Redirige al usuario al URL de Mercado Pago
            return Redirect(preferencia.InitPoint);
        }

        // TODAS LAS RESERVAS ADMIN
        public async Task<IActionResult> Index()
        {
            var reservas = await _context.Set<ReservasAdminGetAllViewModel>()
                .FromSqlRaw("EXEC SP_GET_ALL_RESERVAS_ADMIN")
                .ToListAsync();
            return View(reservas);
        }

    }
}
