using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoHsj_Beta.Models;
using ProyectoHsj_Beta.Services;
using System.Security.Claims;

namespace ProyectoHsj_Beta.Controllers
{
    public class ConfiguracionPagoController : Controller
    {
        private readonly HoySeJuegaContext _context;
        private readonly AuditoriaService _auditoriaService;

        public ConfiguracionPagoController(HoySeJuegaContext context, AuditoriaService auditoriaService)
        {
            _context = context;
            _auditoriaService = auditoriaService;
        }
        public async Task<IActionResult> Index()
        {
            return View(await _context.ConfiguracionPagos.ToListAsync());
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var configuracionPago = await _context.ConfiguracionPagos.FindAsync(id);
            if (configuracionPago == null)
                return NotFound();

            return View(configuracionPago);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ConfiguracionPago configuracionPago)
        {
            if (id != configuracionPago.IdConfiguracion)
                return NotFound();

            if (ModelState.IsValid)
            {
                var descripcionAuditoria = $"El usuario ha actualizado los valores en Monto pago. Detalles, valor de la seña: ${configuracionPago.MontoSena.ToString("N0")}, WhatsApp de cancelaciones: {configuracionPago.CelularCancelaciones}.";
                configuracionPago.FechaModificacion = DateTime.Now;
                _context.Update(configuracionPago);
                await _context.SaveChangesAsync();
                await _auditoriaService.RegistrarAuditoriaAsync(
                seccion: "Administración",
                descripcion: descripcionAuditoria,
                idAccion: 2);
                return RedirectToAction(nameof(Index));
            }
            return View(configuracionPago);
        }

    }
}
