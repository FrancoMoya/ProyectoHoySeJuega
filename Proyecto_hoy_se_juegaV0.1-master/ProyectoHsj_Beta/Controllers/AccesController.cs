using Microsoft.AspNetCore.Mvc;
using ProyectoHsj_Beta.Models;
using ProyectoHsj_Beta.ViewsModels;
using ProyectoHsj_Beta.Utilities;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using FluentEmail.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using ProyectoHsj_Beta.Repositories;


namespace ProyectoHsj_Beta.Controllers
{
    public class AccesController : Controller
    {
        private readonly HoySeJuegaContext _hoysejuegacontext;
        private readonly IFluentEmail _fluentEmail;
        private readonly IPermisoRepository _permisoRepository;
        private readonly HoySeJuegaContext _context;

        public AccesController(HoySeJuegaContext seJuegaContext, IFluentEmail fluentEmail, IPermisoRepository permisoRepository, HoySeJuegaContext context)
        {
            _hoysejuegacontext = seJuegaContext;
            _fluentEmail = fluentEmail;
            _permisoRepository = permisoRepository;
            _context = context;
        }
        public IActionResult AccessDenied()
        {
            return View();
        }

        //Logueo via Google
        [HttpGet]
        public async Task GoogleLogin()
        {
            await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme,
                new AuthenticationProperties
                {
                    RedirectUri = Url.Action("GoogleResponse")
                });
            Console.WriteLine("Entro al properties");
            
        }

        //Autenticacion de via google method :  GET
        [HttpGet]
        public async Task<IActionResult> GoogleResponse( string remoteError = null)
        {
            if (remoteError != null)
            {
                // Manejar el error
                return RedirectToAction(nameof(Login));
            }
            Console.WriteLine("Entro a la funcion Callback");

            // Obtener información del usuario autenticado por Google iji
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (result?.Principal == null)
            {
                return RedirectToAction("Login");
            }

            // Extraer el email y la información básica
            var claims = result.Principal.Identities.FirstOrDefault()?.Claims;
            var email = claims?.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
            var nombre = claims?.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(email))
            {
                ViewData["Message"] = "Error obteniendo los datos del usuario.";
                return RedirectToAction("Login");
            }

            // Comprobar si el usuario ya existe en la base de datos
            var usuarioExistente = await _hoysejuegacontext.Usuarios
                .FirstOrDefaultAsync(u => u.CorreoUsuario == email);


              Console.WriteLine($"El usuario es:  {usuarioExistente}");
            

            if (usuarioExistente == null)
            {
                // Registrar nuevo usuario si  es que no existe
                Usuario nuevoUsuario = new Usuario
                {
                    CorreoUsuario = email,
                    NombreUsuario = nombre,
                    TelefonoUsuario = 116162,
                    ApellidoUsuario = "GoogleUser",  // Valor por defecto indicar al usuario que debe de modificar
                    ContraseniaUsuario = "123", // .
                    EmailConfirmed = true, // Marcamos como confirmado porque Google valida los correos
                    IdRol = 1 // Asignar el rol por defecto(Rol de usuario)
                };

                _hoysejuegacontext.Usuarios.Add(nuevoUsuario);
                await _hoysejuegacontext.SaveChangesAsync();

                // Autenticar al usuario manualmente después de registrarlo
                await SignInUser(nuevoUsuario);
            }
            else
            {
                // Si el usuario ya existe, lo auntenticamos directamente
                await SignInUser(usuarioExistente);
            }

            return RedirectToAction("Index", "Home");
        }

        //Creacion de claims por medio del acceso de google
        private async Task SignInUser(Usuario usuario)
        {
            // Crear los claims para el usuario
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, usuario.NombreUsuario),
            new Claim(ClaimTypes.Email, usuario.CorreoUsuario),
            new Claim(ClaimTypes.Role, usuario.IdRol.ToString())
        };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            // Iniciar sesión con la autenticación por cookies
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        }

        //Registro de usuario method : GET
        [HttpGet]
        public IActionResult Signup()
        {
            return View();
        }

        //Registro de usuario method : POST
        [HttpPost]
        public async Task <IActionResult> Signup(UsuarioVM modelo)
        {
            
              var UserExist = await _hoysejuegacontext.Usuarios
                    .Where(u => u.CorreoUsuario != null)
                    .FirstOrDefaultAsync(u => u.CorreoUsuario == modelo.CorreoUsuario);
            
              if(UserExist != null)
              {
                    ViewData["Message"] = "El correo ya existe";

                    return View(modelo);

              }

            if(modelo.ContraseniaUsuario != modelo.ConfirmarContraseña)
            {
                ViewData["Message"] = "Las contraseñas no coinciden";
                return View();
            }
            // Funcion traida de utilities para encryptar la contraseña.
            string hashedPassword = PasswordHasher.HashPassword(modelo.ContraseniaUsuario);

            // Funcion para obtener el id manualmente.(Eliminar una vez tengan la base de datos actualizada)
           

            Usuario usuario = new Usuario()
            {
                NombreUsuario = modelo.NombreUsuario,
                ApellidoUsuario = modelo.ApellidoUsuario,
                CorreoUsuario = modelo.CorreoUsuario,
                TelefonoUsuario = modelo.TelefonoUsuario,
                ContraseniaUsuario = hashedPassword,
                EmailConfirmed = false, // por defecto lo dejamos en falso, para poder gestionar la validacion
                EmailConfirmationToken = Guid.NewGuid().ToString(), // Obtencion del token jiji
                // Modificar el valor a (2/3) si es la primera vez ->
                //  para poder tener acceso al panel de administracion
                //  2 = admin / 3 = Empleado
                IdRol = (1),
            };
            await _hoysejuegacontext.Usuarios.AddAsync(usuario);
            await _hoysejuegacontext.SaveChangesAsync();

            var confirmacionUrl = Url.Action("ConfirmarCorreo", "Acces",
            new { token = usuario.EmailConfirmationToken, email = usuario.CorreoUsuario },
           Request.Scheme);
            // Contenido del correo
            string mensaje = $"Por favor confirma tu correo electrónico haciendo clic en el siguiente enlace: <a href='{confirmacionUrl}'>Confirmar correo</a>";
            await _fluentEmail
           .To(modelo.CorreoUsuario)
           .Subject("Confirmación de correo electrónico")
           .Body(mensaje, isHtml: true)
           .SendAsync();

            if (usuario.IdUsuario != 0)
            {
                ViewData["Message"] = "Registro exitoso. Revisa tu correo para confirmar tu dirección.";
                return View("RegistroExitoso"); // Vista que indica el éxito del registro
            }
            ViewData["Message"] = "No se pudo registrar al usuario";
            return View();
        }

        // View para indicar al usuario que su registro se completo y debe verificar su correo(Ayuda tengo depresion)
        public IActionResult RegistroExitoso()
        {
            return View();
        }

        //Validar correo method post
        public async Task<IActionResult> ConfirmarCorreo(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                ViewData["Message"] = "Token o correo inválido.";
                return View();
            }

            var usuario = await _hoysejuegacontext.Usuarios
                .FirstOrDefaultAsync(u => u.CorreoUsuario == email && u.EmailConfirmationToken == token);

            if (usuario == null)
            {
                ViewData["Message"] = "Token inválido o usuario no encontrado.";
                return View();
            }

            usuario.EmailConfirmed = true; // Marcar el correo como confirmado
            usuario.EmailConfirmationToken = null; // Borrar el token, ya no es necesario

            _hoysejuegacontext.Usuarios.Update(usuario);
            await _hoysejuegacontext.SaveChangesAsync();

            ViewData["Message"] = "Correo electrónico confirmado con éxito.";
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        //Controlador de Login method POST
        [HttpPost]
        public async Task<IActionResult> Login(LoginVM modelo)
        {
            Usuario? usuario_found = await _hoysejuegacontext.Usuarios
                                    .Where(u =>
                                     u.CorreoUsuario == modelo.CorreoUsuario
                                     ).FirstOrDefaultAsync();
            if (usuario_found == null || !PasswordHasher.VerifyPassword(modelo.ContraseniaUsuario, usuario_found.ContraseniaUsuario))
            {
                ViewData["Message"] = "No se pudo iniciar sesión, por favor revise los datos ingresados.";
                return View();
            }

            if(usuario_found.EmailConfirmed != true)
            {
                ViewData["Message"] = "Debes confirmar tu correo antes de iniciar sesión.";
                ViewData["ShowResendLink"] = true;
                return View(modelo);

            }

           
            // Obtener permisos desde el rol del usuario(Se usara para dar las restricciones en base a permisos otorgados)
            var permisos = await _permisoRepository.GetPermisosByRolIdAsync((int)usuario_found.IdRol);



            //Auntenticacion via claims y cookies. jijij

            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, usuario_found.IdUsuario.ToString()),
                new Claim(ClaimTypes.Name, usuario_found.NombreUsuario),
                new Claim(ClaimTypes.Role, usuario_found.IdRol.ToString())
            };

            foreach (var permiso in permisos)
            {
                claims.Add(new Claim("Permiso", permiso.NombrePermiso)); // Tambien se puede colocar idPermiso (A preferencia)
            }
            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            AuthenticationProperties properties = new AuthenticationProperties()
            {
                AllowRefresh = true,
            };
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                properties

                );

            foreach (var claim in claims)
            {
                Console.WriteLine($"Claim: {claim.Type}, Value: {claim.Value}");
            }

            //Guardar ultima sesion (NO FUNCIONAAAA)
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                // Buscar al usuario en la base de datos
                var usuario = await _context.Usuarios.FindAsync(userId);

                if (usuario != null)
                {
                    // Actualizar el campo UltimaSesion con la fecha actual
                    usuario.UltimaSesion = DateOnly.FromDateTime(DateTime.Now);
                    // Guardar los cambios en la base de datos
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> resendEmail(string email)
        {
            var usuario_foound = await _hoysejuegacontext.Usuarios.FirstOrDefaultAsync(u => u.CorreoUsuario == email);
            if(usuario_foound == null)
            {
                ViewData["Message"] = "El usuario no existe.";
                return RedirectToAction("Login");
            }

            if (usuario_foound.EmailConfirmed == true)
            {
                ViewData["Message"] = "El correo ya fue confirmado.";
                return RedirectToAction("Login");

            }

            // Generar nuevo token de confirmación si es necesario
            usuario_foound.EmailConfirmationToken = Guid.NewGuid().ToString();
            await _hoysejuegacontext.SaveChangesAsync();

            // Generar la URL de confirmación
            var confirmacionUrl = Url.Action("ConfirmarCorreo", "Acces", new { token = usuario_foound.EmailConfirmationToken, email = usuario_foound.CorreoUsuario }, Request.Scheme);

            // Contenido del correo de confirmación
            string mensaje = $"Por favor, confirma tu correo haciendo clic en el siguiente enlace: <a href='{confirmacionUrl}'>Confirmar correo</a>";

            // Enviar correo
            await _fluentEmail.To(usuario_foound.CorreoUsuario)
                              .Subject("Reenvío de confirmación de correo electrónico")
                              .Body(mensaje, isHtml: true)
                              .SendAsync();

            ViewData["Message"] = "Se ha reenviado el correo de confirmación.";
            return RedirectToAction("Login");
        }

        //Logout
        public async Task<IActionResult> Cerrar()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Home");
        }


    }
}
