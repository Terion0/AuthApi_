using AutenticationApi.Models;
using AuthApi.DbsContext;
using AuthApi.Models;
using AuthApi.Models.DTO;
using AuthApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AuthApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AuDbContext _context;
        private readonly JWTService _jwtService;
        private readonly EmailService _emailService;
        private readonly EncryptService _encryptService;

        public UserController(AuDbContext context, JWTService jwtGenerator, EmailService emailGenerator, EncryptService encryptGenerator)
        {
            _encryptService = encryptGenerator;
            _jwtService = jwtGenerator;
            _emailService = emailGenerator;
            _context = context;
        }

        [HttpGet("GetUsers", Name = "GetUsers")]
        public async Task<List<User>> Get()
        {
            return await _context.Users.ToListAsync();
        }

        [HttpPost("RegisterUsers", Name = "RegUser")]
        public async Task<IActionResult> Postuser([FromBody] RegisterDTO credentials)
        {
            if (credentials.Password != credentials.PasswordRepeat)
            {
                return BadRequest(new { message = "Las contraseñas no coinciden." });
            }

            var usuarioExistente = await _context.Users.FirstOrDefaultAsync(u => u.Email == credentials.Email);

            if (usuarioExistente != null)
            {
                if (usuarioExistente.Confirmed)
                {
                    return BadRequest(new { message = "Este correo ya está en uso." });
                }
                else
                {         
                    string newToken = _jwtService.GenerateJwtToken(usuarioExistente, "RegisteredUser", TimeSpan.FromHours(1));
                    bool reenviado = _emailService.CreateAndSendConfirmarionEmail(usuarioExistente.Email, newToken);

                    if (reenviado)
                    {
                        return Ok(new { message = "Usuario creado. Por favor, confirme su email." });
                    }
                    else
                    {
                        return BadRequest(new { message = "Error al reenviar el correo de confirmación." });
                    }
                }
            }
            User nuevo = new();
            nuevo.Name = credentials.Name;
            nuevo.Email = credentials.Email;
            nuevo.PasswordHash = _encryptService.HashPassword(credentials.Password);
            nuevo.UserType = credentials.UserType;

            string token = _jwtService.GenerateJwtToken(nuevo, "RegisteredUser", TimeSpan.FromHours(1));
            bool enviado = _emailService.CreateAndSendConfirmarionEmail(nuevo.Email, token);

            if (!enviado)
            {
                return BadRequest(new { message = "Hubo un problema al enviar el correo de confirmación." });
            }

            _context.Users.Add(nuevo);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Usuario creado. Por favor, confirme su email." });
        }

        [HttpPost("LogUser", Name = "LogUser")]
        public async Task<IActionResult> Getuser([FromBody] LoginDTO credentials)
        {
            var confirmacion = await _context.Users.FirstOrDefaultAsync(u => u.Email == credentials.Email);
            if (confirmacion != null)
            {
                if (confirmacion.Confirmed == true)
                {
                    if (_encryptService.VerifyPassword(credentials.Password, confirmacion.PasswordHash))
                    {
                        string loginToken = _jwtService.GenerateJwtToken(confirmacion, "LoginUser", TimeSpan.FromDays(30));
                        return Ok(new { token = loginToken });
                    }
                    else return Unauthorized(new { message = "Email o contraseña no válidos" });
                }
                else return BadRequest(new { message = "Por favor, confirma tu email" });
            }
            else return NotFound(new { message = "Usuario no encontrado" });
        }

        [HttpGet("ConfirmEmail/{token}", Name = "ConfirmUser")]
        public async Task<IActionResult> PostEmail(string token)
        {
            try
            {
                ClaimsPrincipal? confirm = _jwtService.ValidateToken(token, "RegisteredUser");
                if (confirm != null)
                {
                    string? email = confirm.FindFirst(ClaimTypes.Email)?.Value;
                    if (email != null)
                    {
                        User? confirmado = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                        if (confirmado != null && confirmado.Confirmed == false)
                        {
                            confirmado.Confirmed = true;
                            _context.Users.Update(confirmado);
                            await _context.SaveChangesAsync();
                            return Ok(new { message = "Email confirmado" });
                        }
                        else return StatusCode(403, new { message = "Ya validado" });
                    }
                    else return BadRequest(new { message = "Token inválido: no contiene un email" });
                }
                else return Unauthorized(new { message = "Validación incorrecta" });
            }
            catch (Exception e)
            {
                return BadRequest(new { message = e.Message });
            }
        }

        [HttpGet("RequestChangePassword/{email}", Name = "Request")]
        public async Task<IActionResult> PostPassword(string email)
        {
            try
            {
                if (email != null)
                {
                    User? confirmado = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                    if (confirmado != null)
                    {
                        string tokenReset = _jwtService.GenerateJwtToken(confirmado, "UserChangePassword", TimeSpan.FromMinutes(30));
                        bool enviado = _emailService.CreateAndSendPasswordRessetEmail(email, tokenReset);
                        if (enviado)
                        {
                            return Ok(new { message = "Email enviado" });
                        }
                        else return BadRequest(new { message = "No ha sido posible enviar el email" });
                    }
                    else return NotFound(new { message = "Usuario no encontrado" });
                }
                else return BadRequest(new { message = "Email no válido" });
            }
            catch (Exception e)
            {
                return BadRequest(new { message = e.Message });
            }
        }

        [HttpPatch("ChangePassword", Name = "ChangePassword")]
        public async Task<IActionResult> PatchNew([FromBody]ChangePasswordDTO change)
        {
            try
            {
                if (change.Password == change.PasswordRepeat)
                {
                    ClaimsPrincipal? confirm = _jwtService.ValidateToken(change.Token, "UserChangePassword");
                    if (confirm != null)
                    {
                        string? email = confirm.FindFirst(ClaimTypes.Email)?.Value;
                        if (email != null)
                        {
                            User? confirmado = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                            if (confirmado != null)
                            {
                                confirmado.PasswordHash = _encryptService.HashPassword(change.Password);
                                _context.Users.Update(confirmado);
                                await _context.SaveChangesAsync();
                                return Ok(new { message = "Contraseña cambiada" });
                            }
                            else return NotFound(new { message = "Usuario no encontrado" });
                        }
                        else return BadRequest(new { message = "Token inválido: no contiene un email" });
                    }
                    else return Unauthorized(new { message = "Validación incorrecta" });
                }
                else return BadRequest(new { message = "Contraseñas no coinciden" });
            }
            catch (Exception e)
            {
                return BadRequest(new { message = e.Message });
            }
        }

    }
}

