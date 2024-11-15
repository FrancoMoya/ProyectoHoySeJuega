using MercadoPago.Client.Preference;
using MercadoPago.Config;
using MercadoPago.Resource.Preference;
using ProyectoHsj_Beta.Models;
namespace ProyectoHsj_Beta.Services
{
    public class MercadoPagoService
    {
        private readonly IConfiguration _configuration;

        public MercadoPagoService(IConfiguration configuration)
        {
            _configuration = configuration;
            MercadoPagoConfig.AccessToken = _configuration["MercadoPago:AccessToken"];
        }

        public async Task<Preference> CrearPreferenciaDePago(Pago pago)
        {
            var client = new PreferenceClient();
            var request = new PreferenceRequest
            {
                Items = new List<PreferenceItemRequest>
            {
                new PreferenceItemRequest
                {
                    Title = "Reserva #" + pago.IdReserva,
                    Quantity = 1,
                    CurrencyId = "ARS",
                    UnitPrice = pago.MontoPago
                }
            },
                BackUrls = new PreferenceBackUrlsRequest
                {
                    Success = "https://localhost:7081/Pagoes/Exito",
                    Failure = "https://localhost:7081/Home/Index",
                    Pending = "https://localhost:7081/Reservas/Index"
                },
                AutoReturn = "approved",
                ExternalReference = pago.IdReserva.ToString()
            };

            return await client.CreateAsync(request);
        }
    }
}
