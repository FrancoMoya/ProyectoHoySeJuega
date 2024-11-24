using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MercadoPago.Resource.Preference;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProyectoHsj_Beta.Models;
using ProyectoHsj_Beta.Services;
using ProyectoHsj_Beta.ViewsModels;

namespace ProyectoHsj_Beta.Controllers
{
    public class PagoesController : Controller
    {
        private readonly HoySeJuegaContext _context;
        private readonly MercadoPagoService _mercadoPagoService;
        private readonly AuditoriaService _auditoriaService;

        public PagoesController(HoySeJuegaContext context, MercadoPagoService mercadoPagoService, AuditoriaService auditoriaService)
        {
            _context = context;
            _mercadoPagoService = mercadoPagoService;
            _auditoriaService = auditoriaService;
        }

        [Authorize(Policy = "UserOnly")]
        public IActionResult Index(int reservaId)
        {
            // Obtenemos la configuración de pago (puedes ajustar esto según tu estructura)
            var pagoConfiguracion = _context.ConfiguracionPagos.FirstOrDefault();

            // Verificamos que exista la configuración antes de pasarla a la vista
            if (pagoConfiguracion == null)
            {
                // Manejar el caso en que no haya configuración de pago (opcional)
                pagoConfiguracion = new ConfiguracionPago { MontoSena = 0 };
            }
            var reserva = _context.Reservas
                       .Include(r => r.IdHorarioDisponibleNavigation)
                       .FirstOrDefault(r => r.IdReserva == reservaId);


            if (reserva == null)
            {
                TempData["ErrorMessage"] = "La reserva no existe.";
                return RedirectToAction("MisReservas", "Reservas");
            }

            var viewModel = new PagoViewModel
            {
                Monto = pagoConfiguracion.MontoSena,
                ReservaId = reservaId,
                FechaReserva = reserva.IdHorarioDisponibleNavigation.FechaHorario,
                HoraInicio = reserva.IdHorarioDisponibleNavigation.HoraInicio,
                HoraFin = reserva.IdHorarioDisponibleNavigation.HoraFin
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> IniciarPago(decimal monto, int idReserva)
        {
            // Crear un modelo de pago con los detalles necesarios
            var pago = new Pago
            {
                IdReserva = idReserva,
                MontoPago = monto
            };

            // Crear la preferencia de pago usando el servicio de Mercado Pago
            var preferencia = await _mercadoPagoService.CrearPreferenciaDePago(pago);

            if (preferencia == null)
            {
                return View("Error", "No se pudo crear la preferencia de pago.");
            }

            // Redirige al usuario a la URL de Mercado Pago para completar el pago
            return Redirect(preferencia.InitPoint);
        }


        [HttpGet]
        public async Task<IActionResult> Exito(string external_reference, decimal montoPago)
        {
            Console.WriteLine("Probemos que trae la referencia :" + external_reference);
            // Convertimos external_reference a entero para obtener el ID de la reserva
            if (!int.TryParse(external_reference, out int reservaId))
            {
                return BadRequest("Referencia externa no válida.");
            }

            // Obtén la reserva correspondiente
            var reserva = await _context.Reservas.FindAsync(reservaId);
            if (reserva == null)
            {
                return NotFound("Reserva no encontrada.");
            }
            var configuracionPago = await _context.ConfiguracionPagos.FirstOrDefaultAsync();
            if (configuracionPago == null)
            {
                throw new InvalidOperationException("No se encontró la configuración de pago.");
            }
            montoPago = configuracionPago.MontoSena;
            // Crea un nuevo registro de pago
            var pago = new Pago
            {
                IdReserva = reservaId,
                MontoPago = montoPago,
                FechaPago = DateTime.Now.AddHours(3)
            };

            // Agrega el pago a la base de datos
            _context.Pagos.Add(pago);

            reserva.IdEstadoReserva = 2; // Asegúrate de que 2 signifique CONFIRMADA en tu lógica
            var descripcionAuditoria = $"El usuario ha concretado con éxito el pago de su reserva con los siguientes detalles, ID: {reserva.IdReserva}, Monto: ${montoPago.ToString("N0")}.";
            // Guarda los cambios en la base de datos
            await _context.SaveChangesAsync();
            await _auditoriaService.RegistrarAuditoriaAsync(
                seccion: "Reserva",
                descripcion: descripcionAuditoria,
                idAccion: 1);
            TempData["Message"] = "La reserva ha sido confirmada con éxito.";
            return RedirectToAction("MisReservas", "Reservas");
        }

        private bool PagoExists(int id)
        {
            return _context.Pagos.Any(e => e.IdPago == id);
        }

        [Authorize(Policy = "AdminOrEmployed")]
        public async Task<IActionResult> ListPagos()
        {
            var pagos = await _context.Set<ListPagosAdminViewModel>()
                .FromSqlRaw("EXEC SP_GET_PAGOS_ADMIN")
                .ToListAsync();
            return View(pagos);
        }
        [Authorize(Policy = "AdminOrEmployed")]
        public async Task<IActionResult> HistorialPagos()
        {
            var pagos = await _context.Set<ListPagosAdminViewModel>()
                .FromSqlRaw("EXEC SP_GET_ALL_PAGOS_ADMIN")
                .ToListAsync();
            return View(pagos);
        }
    }
}
