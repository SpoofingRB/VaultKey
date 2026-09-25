using appointmentapi.Data;
using appointmentapi.DTOs.Auth;
using appointmentapi.Models.AuthEntity;
using appointmentapi.Models.CorporativoEntity;
using appointmentapi.Services.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace appointmentapi.Services
{
    public class AuthService : IAuthService
    {
        private readonly JsonDataStore _store;
        private readonly ICryptoService _crypto;
        private const string ARQUIVO_USUARIOS = "usuarios.json";
        private const string ARQUIVO_CONTAS = "contas-corporativas.json";
        private const int MAX_TENTATIVAS = 5;
        private const int MINUTOS_BLOQUEIO = 15;

        public AuthService(JsonDataStore store, ICryptoService crypto)
        {
            _store = store;
            _crypto = crypto;
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

        public async Task<(User? usuario, string? erro)> ValidarLoginAsync(LoginDTO dto)
        {
            var usuarios = await _store.LerAsync<User>(ARQUIVO_USUARIOS);
            var usuario = usuarios.FirstOrDefault(u =>
                u.Email.Equals(dto.Email, StringComparison.OrdinalIgnoreCase));

            if (usuario == null)
                return (null, "E-mail ou senha inválidos.");

            if (usuario.BloqueadoAte.HasValue && usuario.BloqueadoAte.Value > DateTime.UtcNow)
            {
                var minutosRestantes = Math.Ceiling((usuario.BloqueadoAte.Value - DateTime.UtcNow).TotalMinutes);
                return (null, $"Conta temporariamente bloqueada. Tente novamente em {minutosRestantes} minuto(s).");
            }

            if (usuario.BloqueadoAte.HasValue && usuario.BloqueadoAte.Value <= DateTime.UtcNow)
            {
                usuario.TentativasFalhas = 0;
                usuario.BloqueadoAte = null;
            }

            bool senhaValida = BCrypt.Net.BCrypt.Verify(dto.Senha, usuario.SenhaHash);

            if (!senhaValida)
            {
                usuario.TentativasFalhas++;

                if (usuario.TentativasFalhas >= MAX_TENTATIVAS)
                {
                    usuario.BloqueadoAte = DateTime.UtcNow.AddMinutes(MINUTOS_BLOQUEIO);
                    await _store.SalvarAsync(ARQUIVO_USUARIOS, usuarios);
                    return (null, $"Muitas tentativas incorretas. Conta bloqueada por {MINUTOS_BLOQUEIO} minutos.");
                }

                await _store.SalvarAsync(ARQUIVO_USUARIOS, usuarios);
                var restantes = MAX_TENTATIVAS - usuario.TentativasFalhas;
                return (null, $"E-mail ou senha inválidos. {restantes} tentativa(s) restante(s) antes do bloqueio.");
            }

            if (usuario.TentativasFalhas > 0 || usuario.BloqueadoAte.HasValue)
            {
                usuario.TentativasFalhas = 0;
                usuario.BloqueadoAte = null;
                await _store.SalvarAsync(ARQUIVO_USUARIOS, usuarios);
            }

            return (usuario, null);
        }

        public async Task<User> CriarUsuarioFuncionarioAsync(string email, string senha)
        {
            var usuarios = await _store.LerAsync<User>(ARQUIVO_USUARIOS);
            var novoId = usuarios.Count > 0 ? usuarios.Max(u => u.Id) + 1 : 1;

            var novo = new User
            {
                Id = novoId,
                Email = email,
                SenhaHash = BCrypt.Net.BCrypt.HashPassword(senha),
                Role = "Funcionario"
            };

            usuarios.Add(novo);
            await _store.SalvarAsync(ARQUIVO_USUARIOS, usuarios);

            return novo;
        }

        public async Task<bool> RedefinirSenhaAsync(string cpf, string novaSenha)
        {
            var contas = await _store.LerAsync<ContaCorporativa>(ARQUIVO_CONTAS);
            var conta = contas.FirstOrDefault(c => c.Cpf == cpf);
            if (conta == null) return false;

            var usuarios = await _store.LerAsync<User>(ARQUIVO_USUARIOS);
            var usuario = usuarios.FirstOrDefault(u => u.Id == conta.UserId);
            if (usuario == null) return false;

            usuario.SenhaHash = BCrypt.Net.BCrypt.HashPassword(novaSenha);
            usuario.TentativasFalhas = 0;
            usuario.BloqueadoAte = null;
            await _store.SalvarAsync(ARQUIVO_USUARIOS, usuarios);

            var (senhaCifrada, iv) = _crypto.Criptografar(novaSenha);
            conta.SenhaCriptografada = senhaCifrada;
            conta.SenhaIv = iv;
            conta.AtualizadoEm = DateTime.UtcNow;
            await _store.SalvarAsync(ARQUIVO_CONTAS, contas);

            return true;
        }
    }
}