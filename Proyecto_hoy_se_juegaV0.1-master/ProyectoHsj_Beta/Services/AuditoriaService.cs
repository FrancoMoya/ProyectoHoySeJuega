using Microsoft.EntityFrameworkCore;
using ProyectoHsj_Beta.Models;
using System.Security.Claims;

namespace ProyectoHsj_Beta.Services
{
    public class AuditoriaService
    {
        private readonly HoySeJuegaContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditoriaService(HoySeJuegaContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task RegistrarAuditoriaAsync(string seccion, string descripcion, int idAccion)
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!string.IsNullOrEmpty(userId))
            {
                await _context.Database.ExecuteSqlInterpolatedAsync(
                    $"EXEC SP_AUDITORIA {userId}, {seccion}, {descripcion}, {idAccion}");
            }
        }
    }
}
