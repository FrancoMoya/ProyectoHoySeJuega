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
      options.LoginPath = "/Acces/Login"; //Ruta de la página de inicio de sesión
      options.AccessDeniedPath = "/Acces/AccessDenied"; // Ruta de la página de acceso denegado
      options.ExpireTimeSpan = TimeSpan.FromHours(24);// Tiempo de expiración de la cookie

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
    options.AddPolicy("UserOnly", policy => policy.RequireRole("1"));
    options.AddPolicy("EmployedOnly", policy => policy.RequireRole("3"));
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
