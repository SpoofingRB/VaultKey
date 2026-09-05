using appointmentapi.Data;
using appointmentapi.DTOs.Auth;
using appointmentapi.Models.AuthEntity;
using appointmentapi.Services.Interface;

namespace appointmentapi.Services
{
    public class AuthService : IAuthService
    {
        private readonly JsonDataStore _store;
        private const string ARQUIVO_USUARIOS = "usuarios.json";

        public AuthService(JsonDataStore store)
        {
            _store = store;
        }

        public async Task<User?> SeedAdminAsync(RegisterDTO dto)
        {
            var usuarios = await _store.LerAsync<User>(ARQUIVO_USUARIOS);
            if (usuarios.Count > 0) return null;

            var admin = new User
            {
                Id = 1,
                Email = dto.Email,
                SenhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha),
                Role = "Admin"
            };

            usuarios.Add(admin);
            await _store.SalvarAsync(ARQUIVO_USUARIOS, usuarios);

            return admin;
        }

        public async Task<User?> ValidarLoginAsync(LoginDTO dto)
        {
            var usuarios = await _store.LerAsync<User>(ARQUIVO_USUARIOS);
            var usuario = usuarios.FirstOrDefault(u => u.Email == dto.Email);
            if (usuario == null) return null;

            bool senhaValida = BCrypt.Net.BCrypt.Verify(dto.Senha, usuario.SenhaHash);
            if (!senhaValida) return null;

            return usuario;
        }

        public async Task<User> CriarUsuarioFuncionarioAsync(string email, string senha)
        {
            var usuarios = await _store.LerAsync<User>(ARQUIVO_USUARIOS);

            var novoId = usuarios.Count > 0 ? usuarios.Max(u => u.Id) + 1 : 1;

            var usuario = new User
            {
                Id = novoId,
                Email = email,
                SenhaHash = BCrypt.Net.BCrypt.HashPassword(senha),
                Role = "Funcionario"
            };

            usuarios.Add(usuario);
            await _store.SalvarAsync(ARQUIVO_USUARIOS, usuarios);

            return usuario;
        }
    }
}