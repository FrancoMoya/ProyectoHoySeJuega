using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoHsj_Beta.Models;
using ProyectoHsj_Beta.ViewsModels;

namespace ProyectoHsj_Beta.Controllers
{
    public class AuditoriaController : Controller
    {
        private readonly HoySeJuegaContext _context;

        public AuditoriaController(HoySeJuegaContext context)
        {
            _context = context;
        }

        // GET: Auditoria
        public async Task<IActionResult> Index()
        {
            var registros = await _context.Set<AuditoriaGetViewModel>()
                .FromSqlRaw("EXEC SP_GET_AUDITORIA")
                .ToListAsync();
            return View(registros);
        }
        public async Task<IActionResult> HistorialAuditoria()
        {
            var registros = await _context.Set<AuditoriaGetViewModel>()
                .FromSqlRaw("EXEC SP_GET_ALL_AUDITORIA")
                .ToListAsync();
            return View(registros);
        }

    }
}
