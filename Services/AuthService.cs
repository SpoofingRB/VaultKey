using appointmentapi.Data;
using appointmentapi.DTOs.Auth;
using appointmentapi.Models.AuthEntity;
using appointmentapi.Services.Interface;
using appointmentapi.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace appointmentapi.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly JwtSettings _jwtSettings;

        public AuthService(AppDbContext context, IOptions<JwtSettings> jwtSettings)
        {
            _context = context;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<AuthResponseDTO?> RegistrarAsync(RegisterDTO dto)
        {
            var emailJaExiste = await _context.Users.AnyAsync(u => u.Email == dto.Email);
            if (emailJaExiste) return null;

            var novoUsuario = new User
            {
                Email = dto.Email,
                SenhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha)
            };

            _context.Users.Add(novoUsuario);
            await _context.SaveChangesAsync();

            return GerarToken(novoUsuario);
        }

        public async Task<AuthResponseDTO?> LoginAsync(LoginDTO dto)
        {
            var usuario = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (usuario == null) return null;

            bool senhaValida = BCrypt.Net.BCrypt.Verify(dto.Senha, usuario.SenhaHash);
            if (!senhaValida) return null;

            return GerarToken(usuario);
        }

        private AuthResponseDTO GerarToken(User usuario)
        {
            var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, usuario.Email)
            };

            var expiracao = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: expiracao,
                signingCredentials: credenciais
            );

            return new AuthResponseDTO
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Email = usuario.Email,
                ExpiraEm = expiracao
            };
        }
    }
}