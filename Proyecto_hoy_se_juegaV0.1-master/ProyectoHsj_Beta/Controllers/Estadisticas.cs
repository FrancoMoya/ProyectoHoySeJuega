using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoHsj_Beta.Models;

namespace ProyectoHsj_Beta.Controllers
{
    public class Estadisticas : Controller
    {
        private readonly HoySeJuegaContext _context;

        public Estadisticas(HoySeJuegaContext context)
        {
            _context = context;
        }
        // se obtiene los datos de usuarios registrados y autenticados
        [Authorize(Policy = "AdminOnly")]
        public async Task<JsonResult> GetUserStatistics()
        {
            // Obtener el primer día del mes actual
            var primerDiaDelMes = new DateOnly(DateTime.Now.Year, DateTime.Now.Month, 1);

            // Obtener el último día del mes actual
            var ultimoDiaDelMes = primerDiaDelMes.AddMonths(1).AddDays(-1);

            // USUARIOS
            var totalUsuariosRegistrados = await _context.Usuarios.CountAsync();
            var usuariosAutenticados = await _context.Usuarios
                                                    .Where(u => u.EmailConfirmed == true)
                                                    .CountAsync();
            //Mensual
            int usuariosRegistradosMensual = await _context.Usuarios
                .Where(u => u.FechaRegistro >= primerDiaDelMes && u.FechaRegistro <= ultimoDiaDelMes)
                .CountAsync();

            int usuariosAutenticadosMensual = await _context.Usuarios
                .Where(u => u.FechaRegistro >= primerDiaDelMes && u.EmailConfirmed == true && u.FechaRegistro <= ultimoDiaDelMes)
                .CountAsync();

            // RESERVAS
            var reservasClientesConfirmadas = await _context.Reservas
                                                    .Where(r => r.IdEstadoReserva == 2)
                                                    .CountAsync();
            var reservasAdminConfirmadas = await _context.Eventos
                                                    .Where(e => e.IdEstadoReserva == 2)
                                                    .CountAsync();
            var canchasConfirmadas = reservasClientesConfirmadas + reservasAdminConfirmadas;

            var reservasAdmin = await _context.Eventos
                                                    .CountAsync();
            var reservasClientes = await _context.Reservas
                                                    .CountAsync();
            var canchasReservadas = reservasAdmin + reservasClientes;
            //Mensual
            int canchasReservadasClientesMensual = await _context.Reservas
                .Where(u => u.FechaReserva.HasValue &&
                u.FechaReserva.Value.Month == DateTime.Now.Month &&
                u.FechaReserva.Value.Year == DateTime.Now.Year)
                .CountAsync();
            int canchasReservadasAdminMensual = await _context.Eventos
                .Where(u => u.FechaEvento.HasValue &&
                u.FechaEvento.Value.Month == DateTime.Now.Month &&
                u.FechaEvento.Value.Year == DateTime.Now.Year)
                .CountAsync();
            int canchasReservadasMensual = canchasReservadasClientesMensual + canchasReservadasAdminMensual;

            int canchasConfirmadasClientesMensual = await _context.Reservas
                .Where(u => u.FechaReserva.HasValue &&
                u.FechaReserva.Value.Month == DateTime.Now.Month &&
                u.FechaReserva.Value.Year == DateTime.Now.Year && u.IdEstadoReserva == 2)
                .CountAsync();
            int canchasConfirmadasAdminMensual = await _context.Eventos
                .Where(u => u.FechaEvento.HasValue &&
                u.FechaEvento.Value.Month == DateTime.Now.Month &&
                u.FechaEvento.Value.Year == DateTime.Now.Year && u.IdEstadoReserva == 2)
                .CountAsync();
            int canchasConfirmadasMensual = canchasConfirmadasClientesMensual + canchasConfirmadasAdminMensual;

            var data = new
            {
                Registrados = totalUsuariosRegistrados,
                Autenticados = usuariosAutenticados,
                Reservas = canchasReservadas,
                Confirmadas = canchasConfirmadas,
                UsuariosRegistradosMensual = usuariosRegistradosMensual,
                UsuariosAutenticadosMensual = usuariosAutenticadosMensual,
                CanchasReservadasMensual = canchasReservadasMensual,
                CanchasConfirmadasMensual = canchasConfirmadasMensual
            };

            return Json(data);
        }


    }
}
