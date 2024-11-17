using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MercadoPago.Resource.Preference;
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

        // GET: Pagoes
        public async Task<IActionResult> Index(int reservaId)
        {
            // Obtenemos la configuración de pago (puedes ajustar esto según tu estructura)
            var pagoConfiguracion = _context.ConfiguracionPagos.FirstOrDefault();

            // Verificamos que exista la configuración antes de pasarla a la vista
            if (pagoConfiguracion == null)
            {
                // Manejar el caso en que no haya configuración de pago (opcional)
                pagoConfiguracion = new ConfiguracionPago { MontoSena = 0 };
            }
            var reserva = _context.Reservas.FirstOrDefault(r => r.IdReserva == reservaId);

            if (reserva == null)
            {
                // Manejar el caso en que no se encuentre la reserva
                return RedirectToAction("Acces", "AccesDenied");
            }

            var viewModel = new PagoViewModel
            {
                Monto = pagoConfiguracion.MontoSena,
                ReservaId = reservaId
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

            // Crea un nuevo registro de pago
            var pago = new Pago
            {
                IdReserva = reservaId,
                MontoPago = montoPago,
                FechaPago = DateTime.Now
            };

            // Agrega el pago a la base de datos
            _context.Pagos.Add(pago);

            // Actualiza el estado de la reserva a CONFIRMADA (por ejemplo, usando un código de estado 2)
            reserva.IdEstadoReservaNavigation.IdEstadoReserva = 2; // Asegúrate de que 2 signifique CONFIRMADA en tu lógica

            // Guarda los cambios en la base de datos
            await _context.SaveChangesAsync();
            await _auditoriaService.RegistrarAuditoriaAsync(
                seccion: "Pagos",
                descripcion: "El usuario concretado con éxito el pago de una reserva.",
                idAccion: 1);

            return View("PagoExitoso", pago); // Redirige a una vista de confirmación
            //RETORNAR A PAGINA DE INICIO
        }
        // GET: Pagoes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pago = await _context.Pagos
                .Include(p => p.IdReservaNavigation)
                .FirstOrDefaultAsync(m => m.IdPago == id);
            if (pago == null)
            {
                return NotFound();
            }

            return View(pago);
        }

        // POST: Pagoes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pago = await _context.Pagos.FindAsync(id);
            if (pago != null)
            {
                _context.Pagos.Remove(pago);
            }

            await _context.SaveChangesAsync();
            await _auditoriaService.RegistrarAuditoriaAsync(
                seccion: "Pagos",
                descripcion: "El usuario ha eliminado un registro de pago", //REVISAR!!!!!!
                idAccion: 3);
            return RedirectToAction(nameof(Index));
        }

        private bool PagoExists(int id)
        {
            return _context.Pagos.Any(e => e.IdPago == id);
        }
    }
}
