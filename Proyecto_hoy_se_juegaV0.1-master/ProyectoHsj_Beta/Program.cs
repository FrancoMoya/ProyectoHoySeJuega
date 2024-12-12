using Microsoft.EntityFrameworkCore;
using ProyectoHsj_Beta.Models;
using System.Net.Mail;
using System.Net;
using FluentEmail.Core;
using FluentEmail.Smtp;
using Microsoft.AspNetCore.Authentication.Cookies;
using ProyectoHsj_Beta.Repositories;
using ProyectoHsj_Beta.Services;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpContextAccessor(); // Para acceder al contexto HTTP en el servicio
builder.Services.AddScoped<AuditoriaService>(); // Registrar el servicio de auditoria
builder.Services.AddHostedService<ReservaCancellationService>(); // // Registrar el Background Service
builder.Services.AddScoped<IPermisoRepository, PermisoRepository>();
builder.Services.AddControllersWithViews();
//Servicio de conexion con la database
builder.Services.AddDbContext<HoySeJuegaContext>(Options =>
{
    Options.UseSqlServer(builder.Configuration.GetConnectionString("Conexion"));
});


builder.Services.AddFluentEmail(builder.Configuration["Smtp:Username"], "HoySeJuega")
    .AddSmtpSender(new SmtpClient(builder.Configuration["Smtp:Host"])
    {
        Port = int.Parse(builder.Configuration["Smtp:Port"]),
        Credentials = new NetworkCredential(
            builder.Configuration["Smtp:Username"],
            builder.Configuration["Smtp:Password"] // seteada en dotnet user-secrets
        ),
        EnableSsl = true
    });

// Configuracion de autenticacion
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
  .AddCookie(options =>
  {
      options.LoginPath = "/Acces/Login"; //Ruta de la p�gina de inicio de sesi�n
      options.AccessDeniedPath = "/Acces/AccessDenied"; // Ruta de la p�gina de acceso denegado
      options.ExpireTimeSpan = TimeSpan.FromDays(2);// Tiempo de expiraci�n de la cookie
      options.SlidingExpiration = true; // La cookie se renovar� autom�ticamente si el usuario sigue activo
      options.Cookie.Domain = null; // Define el dominio de la cookie
      options.Cookie.SameSite = SameSiteMode.None; // Permite que la cookie sea enviada en solicitudes entre sitios (necesario si la autenticaci�n es cross-site, como en un subdominio diferente)
      options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // La cookie solo se enviar� a trav�s de HTTPS
  }
    )
  .AddGoogle(options =>
  {


      options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
      options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
      options.CallbackPath = new PathString("/Acces/ExternalLoginCallback");
  });

// Configguracion de politicas de autorizacion
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("2"));
    options.AddPolicy("UserOnly", policy => policy.RequireRole("1","2","3"));
    options.AddPolicy("AdminOrEmployed", policy =>
       policy.RequireRole("2", "3")); // Admin (2) y Employed (3)
});

builder.Services.AddScoped<MercadoPagoService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication(); // Ensure authentication middleware is added

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
