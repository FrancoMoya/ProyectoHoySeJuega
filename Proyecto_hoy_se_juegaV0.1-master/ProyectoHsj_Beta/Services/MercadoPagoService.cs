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
                    //PictureUrl = "",
                    Quantity = 1,
                    CurrencyId = "ARS",
                    UnitPrice = pago.MontoPago
                }
            },
                PaymentMethods = new PreferencePaymentMethodsRequest
                {
                    ExcludedPaymentTypes = new List<PreferencePaymentTypeRequest>
                    {
                        new PreferencePaymentTypeRequest
                        {
                            Id = "ticket",
                        },
                        new PreferencePaymentTypeRequest
                        {
                            Id = "credit_card",
                        },
                    },
                },

                BackUrls = new PreferenceBackUrlsRequest
                {
                    Success = "https://hsejuega.somee.com/Pagoes/Exito",
                    Failure = "https://hsejuega.somee.com/Home/Index",
                    Pending = "https://hsejuega.somee.com/Home/Index"
                },
                AutoReturn = "approved",
                ExternalReference = pago.IdReserva.ToString(),
                Expires = true,
                ExpirationDateFrom = DateTime.Now.AddHours(3), // Aumenta 3 horas por el servidor
                ExpirationDateTo = DateTime.Now.AddHours(3).AddMinutes(5).AddSeconds(30)
            };
            
            return await client.CreateAsync(request);
        }
    }
}
