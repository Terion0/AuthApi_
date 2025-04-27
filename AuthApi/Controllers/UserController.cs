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
        public async Task<IActionResult> Postuser(RegisterDTO credentials)
        {
            if (credentials.Password == credentials.PasswordRepeat)
            {
                var confirmacion = await _context.Users.FirstOrDefaultAsync(u => u.Email == credentials.Email);
                if (confirmacion == null)
                {
                    User nuevo = new();
                    nuevo.Name = credentials.Name;
                    nuevo.Email = credentials.Email;
                    nuevo.PasswordHash = _encryptService.HashPassword(credentials.Password);
                    nuevo.UserType = credentials.UserType;
                    string token = _jwtService.GenerateJwtToken(nuevo, "RegisteredUser", TimeSpan.FromHours(1));
                    bool enviado = _emailService.CreateAndSendConfirmarionEmail(nuevo.Email, token);
                    if (enviado) 
                    {
                        _context.Users.Add(nuevo);
                        await _context.SaveChangesAsync();
                        return Ok("Usuario creado. Por favor, confirme su email.");
                    }
                    else return BadRequest("Hubo un problema al enviar el correo de confirmación.");
                   
                }
                else return BadRequest("Este correo ya está en uso");
            }
            else  return BadRequest("Contraseñas no coinciden"); 
        }

        [HttpPost("LogUser", Name = "LogUser")]
        public async Task<IActionResult> Getuser(LoginDTO credentials)
        {
            var confirmacion = await _context.Users.FirstOrDefaultAsync(u => u.Email == credentials.Email);
            if (confirmacion != null)
            {
                if (confirmacion.Confirmed == true)
                {
                    if (_encryptService.VerifyPassword(credentials.Password, confirmacion.PasswordHash))
                    {
                        string loginToken = _jwtService.GenerateJwtToken(confirmacion, "LoginUser", TimeSpan.FromDays(30));
                        return Ok(loginToken);
                    }
                    else  return Unauthorized("Email o contraseña no válidos"); 
                }
                else  return BadRequest("Por favor, confirma tu email"); 
            }
            else  return NotFound("Usuario no encontrado"); 
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
                            return Ok("Email confirmado");
                        }
                        else return Forbid("Ya validado");
                    }
                    else return BadRequest("Token inválido: no contiene un email");
                }
                else  return Unauthorized("Validación incorrecta"); 
            }
            catch (Exception e) { return BadRequest(e.Message); }
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
                                return Ok("Email enviado");
                            }
                            else return BadRequest("No ha sido posible enviar el email");
                        }
                        else return NotFound("Usuario no encontrado");
                    }
                    else return BadRequest("Email no válido");
            }
            catch (Exception e) { return BadRequest(e.Message); }
        }

        [HttpPatch("ChangePassword", Name = "ChangePassword")]
        public async Task<IActionResult> PatchNew(ChangePasswordDTO change)
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
                                return Ok("Contraseña cambiada");
                            }
                            else return NotFound("Usuario no encontrado");
                        }
                        else return BadRequest("Token inválido: no contiene un email");
                    }
                    else return Unauthorized("Validación incorrecta");
                }
                else return BadRequest("Contraseñas no coinciden");
            }
            catch (Exception e) { return BadRequest(e.Message); }
        }
    }

}

