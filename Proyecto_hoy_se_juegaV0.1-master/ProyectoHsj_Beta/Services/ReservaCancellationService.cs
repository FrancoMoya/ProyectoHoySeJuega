using Microsoft.EntityFrameworkCore;
using ProyectoHsj_Beta.Models;


public class ReservaCancellationService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public ReservaCancellationService(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<HoySeJuegaContext>();

                // Consulta usando FromSqlRaw con el DTO registrado en el contexto
                var reservasPendientes = await context.ReservasPendientes
                    .FromSqlRaw("EXEC SP_GET_RESERVAS_PENDIENTES")
                    .ToListAsync();
                var reservasEnPago = await context.ReservasPendientes
                    .FromSqlRaw("EXEC SP_GET_RESERVAS_PAGOENCURSO")
                    .ToListAsync();

                foreach (var reserva in reservasPendientes)
                {
                    // Cancelar cada reserva pendiente después del tiempo límite
                    await context.Database.ExecuteSqlRawAsync("EXEC SP_CANCELAR_RESERVAS @p0", reserva.ID_reserva);
                }
                foreach (var reserva in reservasEnPago)
                {
                    // Cancelar cada reserva pendiente después del tiempo límite
                    await context.Database.ExecuteSqlRawAsync("EXEC SP_CANCELAR_RESERVAS @p0", reserva.ID_reserva);
                }

                // Guardar los cambios en la base de datos
                await context.SaveChangesAsync();
            }

            // Esperar un intervalo de tiempo antes de volver a comprobar
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}