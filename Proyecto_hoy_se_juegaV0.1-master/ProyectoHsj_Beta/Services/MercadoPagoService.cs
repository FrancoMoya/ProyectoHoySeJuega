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
                    Success = "https://hsejuega.somee.com/Pagoes/Exito",
                    Failure = "https://hsejuega.somee.com/Reservas/Reservar",
                    Pending = "https://hsejuega.somee.com/Home/Index"
                },
                AutoReturn = "approved",
                ExternalReference = pago.IdReserva.ToString()
            };
            
            return await client.CreateAsync(request);
        }
    }
}
