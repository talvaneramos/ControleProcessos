using ControleProcessos.DTOs;
using ControleProcessos.Interfaces;
using ControleProcessos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ControleProcessos.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly IUsuarioService _service;
        private readonly IConfiguration _config;

        public UsuarioController(IUsuarioService service, IConfiguration config)
        {
            _service = service;
            _config = config;
        }
        
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDTO login)
        {
            var user = _service.Login(login.Email, login.Senha);

            if (user == null)
                return Unauthorized(new { mensagem = "Usuário ou senha inválidos" });

            var token = GerarToken(user);

            return Ok(new
            {
                token,
                usuario = new
                {
                    nome = user.Nome,
                    email = user.Email,
                    role = user.Role ?? "User"
                }
            });
        }
       
        [HttpPost("cadastrar")]
        public IActionResult Cadastrar([FromBody] Usuario usuario)
        {            
            if (string.IsNullOrEmpty(usuario.Role))
                usuario.Role = "User";

            _service.Cadastrar(usuario);

            return Ok(new { mensagem = "Usuário cadastrado com sucesso" });
        }
        
        [HttpPut("redefinir-senha")]
        public IActionResult RedefinirSenha([FromBody] RedefinirSenhaDTO dto)
        {
            var sucesso = _service.RedefinirSenha(dto.Email, dto.NovaSenha);

            if (!sucesso)
                return NotFound(new { mensagem = "Usuário não encontrado" });

            return Ok(new { mensagem = "Senha atualizada com sucesso" });
        }
        
        private string GerarToken(Usuario user)
        {
            var jwtKey = _config["Jwt:Key"];

            if (string.IsNullOrEmpty(jwtKey))
                throw new Exception("JWT Key não configurada no appsettings.json");

            var key = Encoding.ASCII.GetBytes(jwtKey);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Nome),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role ?? "User")
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(8),
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}