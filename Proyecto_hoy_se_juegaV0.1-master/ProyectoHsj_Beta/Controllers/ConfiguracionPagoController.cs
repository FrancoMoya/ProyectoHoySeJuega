﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoHsj_Beta.Models;
using System.Security.Claims;

namespace ProyectoHsj_Beta.Controllers
{
    public class ConfiguracionPagoController : Controller
    {
        private readonly HoySeJuegaContext _context;
        public ConfiguracionPagoController(HoySeJuegaContext context)
        {
            _context = context;
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
                configuracionPago.FechaModificacion = DateTime.Now;
                _context.Update(configuracionPago);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(configuracionPago);
        }

    }
}
