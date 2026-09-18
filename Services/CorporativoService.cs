using appointmentapi.Data;
using appointmentapi.DTOs.Corporativo;
using appointmentapi.Models.AuthEntity;
using appointmentapi.Models.CorporativoEntity;
using appointmentapi.Services.Interface;
using appointmentapi.Settings;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace appointmentapi.Services
{
    public class CorporativoService : ICorporativoService
    {
        private readonly JsonDataStore _store;
        private readonly ICryptoService _crypto;
        private readonly IAuthService _authService;
        private readonly string _dominio;
        private const string ARQUIVO_CONTAS = "contas-corporativas.json";
        private const string ARQUIVO_USUARIOS = "usuarios.json";

        public CorporativoService(
            JsonDataStore store,
            ICryptoService crypto,
            IAuthService authService,
            IOptions<CompanySettings> companySettings)
        {
            _store = store;
            _crypto = crypto;
            _authService = authService;
            _dominio = companySettings.Value.Dominio;
        }

        public async Task<List<ContaCorporativaResponseDTO>> ListarAsync()
        {
            var contas = await _store.LerAsync<ContaCorporativa>(ARQUIVO_CONTAS);

            return contas
                .OrderByDescending(c => c.CriadoEm)
                .Select(MapearParaDTO)
                .ToList();
        }

        public async Task<ContaCorporativaResponseDTO?> CriarFuncionarioAsync(CriarFuncionarioDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.NomeCompleto) || string.IsNullOrWhiteSpace(dto.Cpf))
                return null;

            var cpfLimpo = Regex.Replace(dto.Cpf, @"[^0-9]", "");

            if (!CpfValido(cpfLimpo))
                return null;

            var contasExistentes = await _store.LerAsync<ContaCorporativa>(ARQUIVO_CONTAS);
            if (contasExistentes.Any(c => c.Cpf == cpfLimpo))
                return null;

            var partesNome = dto.NomeCompleto.Trim().Split(' ', 2);
            var nome = partesNome[0];
            var sobrenome = partesNome.Length > 1 ? partesNome[1] : "";

            return await CriarContaAsync(nome, sobrenome, cpfLimpo, dto.Departamento);
        }

        public async Task<ContaCorporativaResponseDTO?> CriarViaOnboardingAsync(OnboardingDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.NomeCompleto) || string.IsNullOrWhiteSpace(dto.Cpf) || string.IsNullOrWhiteSpace(dto.Departamento))
                return null;

            var cpfLimpo = Regex.Replace(dto.Cpf, @"[^0-9]", "");

            if (!CpfValido(cpfLimpo))
                return null;

            var contasExistentes = await _store.LerAsync<ContaCorporativa>(ARQUIVO_CONTAS);
            if (contasExistentes.Any(c => c.Cpf == cpfLimpo))
                return null;

            var partesNome = dto.NomeCompleto.Trim().Split(' ', 2);
            var nome = partesNome[0];
            var sobrenome = partesNome.Length > 1 ? partesNome[1] : "";

            return await CriarContaAsync(nome, sobrenome, cpfLimpo, dto.Departamento);
        }

        private async Task<ContaCorporativaResponseDTO> CriarContaAsync(string nome, string sobrenome, string cpf, string departamento)
        {
            var emailBase = NormalizarParaEmail($"{nome}.{sobrenome}");
            var emailCorporativo = await GerarEmailUnicoAsync(emailBase);

            var senhaGerada = GerarSenhaSegura();
            var (senhaCifrada, iv) = _crypto.Criptografar(senhaGerada);

            var usuario = await _authService.CriarUsuarioFuncionarioAsync(emailCorporativo, senhaGerada);

            var contas = await _store.LerAsync<ContaCorporativa>(ARQUIVO_CONTAS);
            var novoId = contas.Count > 0 ? contas.Max(c => c.Id) + 1 : 1;

            var conta = new ContaCorporativa
            {
                Id = novoId,
                UserId = usuario.Id,
                Nome = nome,
                Sobrenome = sobrenome,
                Cpf = cpf,
                Departamento = departamento,
                EmailCorporativo = emailCorporativo,
                SenhaCriptografada = senhaCifrada,
                SenhaIv = iv,
                Ativo = true,
                CriadoEm = DateTime.UtcNow
            };

            contas.Add(conta);
            await _store.SalvarAsync(ARQUIVO_CONTAS, contas);

            var resultado = MapearParaDTO(conta);
            resultado.SenhaVisivel = senhaGerada;
            return resultado;
        }

        public async Task<string?> RevelarSenhaAsync(int contaId)
        {
            var contas = await _store.LerAsync<ContaCorporativa>(ARQUIVO_CONTAS);
            var conta = contas.FirstOrDefault(c => c.Id == contaId);
            if (conta == null) return null;

            return _crypto.Descriptografar(conta.SenhaCriptografada, conta.SenhaIv);
        }

        public async Task<string?> ResetarSenhaAsync(int contaId)
        {
            var contas = await _store.LerAsync<ContaCorporativa>(ARQUIVO_CONTAS);
            var conta = contas.FirstOrDefault(c => c.Id == contaId);
            if (conta == null) return null;

            var novaSenha = GerarSenhaSegura();
            var (senhaCifrada, iv) = _crypto.Criptografar(novaSenha);

            conta.SenhaCriptografada = senhaCifrada;
            conta.SenhaIv = iv;
            conta.AtualizadoEm = DateTime.UtcNow;

            await _store.SalvarAsync(ARQUIVO_CONTAS, contas);

            var usuarios = await _store.LerAsync<User>(ARQUIVO_USUARIOS);
            var usuario = usuarios.FirstOrDefault(u => u.Id == conta.UserId);
            if (usuario != null)
            {
                usuario.SenhaHash = BCrypt.Net.BCrypt.HashPassword(novaSenha);
                await _store.SalvarAsync(ARQUIVO_USUARIOS, usuarios);
            }

            return novaSenha;
        }

        public async Task<bool> AlterarStatusAsync(int contaId, bool ativo)
        {
            var contas = await _store.LerAsync<ContaCorporativa>(ARQUIVO_CONTAS);
            var conta = contas.FirstOrDefault(c => c.Id == contaId);
            if (conta == null) return false;

            conta.Ativo = ativo;
            conta.AtualizadoEm = DateTime.UtcNow;

            await _store.SalvarAsync(ARQUIVO_CONTAS, contas);
            return true;
        }

        public async Task<bool> ExcluirAsync(int contaId)
        {
            var contas = await _store.LerAsync<ContaCorporativa>(ARQUIVO_CONTAS);
            var conta = contas.FirstOrDefault(c => c.Id == contaId);
            if (conta == null) return false;

            contas.Remove(conta);
            await _store.SalvarAsync(ARQUIVO_CONTAS, contas);

            var usuarios = await _store.LerAsync<User>(ARQUIVO_USUARIOS);
            var usuario = usuarios.FirstOrDefault(u => u.Id == conta.UserId);
            if (usuario != null)
            {
                usuarios.Remove(usuario);
                await _store.SalvarAsync(ARQUIVO_USUARIOS, usuarios);
            }

            return true;
        }

        private ContaCorporativaResponseDTO MapearParaDTO(ContaCorporativa c) => new()
        {
            Id = c.Id,
            Nome = c.Nome,
            Sobrenome = c.Sobrenome,
            Cpf = c.Cpf,
            Departamento = c.Departamento,
            EmailCorporativo = c.EmailCorporativo,
            Ativo = c.Ativo,
            CriadoEm = c.CriadoEm
        };

        private string NormalizarParaEmail(string texto)
        {
            var semAcento = texto
                .Replace("á", "a").Replace("à", "a").Replace("ã", "a").Replace("â", "a")
                .Replace("é", "e").Replace("ê", "e")
                .Replace("í", "i")
                .Replace("ó", "o").Replace("õ", "o").Replace("ô", "o")
                .Replace("ú", "u")
                .Replace("ç", "c");

            var minusculo = semAcento.ToLower();
            return Regex.Replace(minusculo, @"[^a-z0-9.]", "");
        }

        private async Task<string> GerarEmailUnicoAsync(string emailBase)
        {
            var usuarios = await _store.LerAsync<User>(ARQUIVO_USUARIOS);
            var emailCandidato = $"{emailBase}@{_dominio}";
            int contador = 1;

            while (usuarios.Any(u => u.Email == emailCandidato))
            {
                emailCandidato = $"{emailBase}{contador}@{_dominio}";
                contador++;
            }

            return emailCandidato;
        }

        private string GerarSenhaSegura()
        {
            const string maiusculas = "ABCDEFGHJKLMNPQRSTUVWXYZ";
            const string minusculas = "abcdefghijkmnpqrstuvwxyz";
            const string numeros = "23456789";
            const string simbolos = "!@#$%&*-_+=?";
            const string todos = maiusculas + minusculas + numeros + simbolos;
            const int tamanho = 16;

            var senha = new List<char>
            {
                maiusculas[RandomIndice(maiusculas.Length)],
                minusculas[RandomIndice(minusculas.Length)],
                numeros[RandomIndice(numeros.Length)],
                simbolos[RandomIndice(simbolos.Length)]
            };

            for (int i = senha.Count; i < tamanho; i++)
                senha.Add(todos[RandomIndice(todos.Length)]);

            for (int i = senha.Count - 1; i > 0; i--)
            {
                int j = RandomIndice(i + 1);
                (senha[i], senha[j]) = (senha[j], senha[i]);
            }

            return new string(senha.ToArray());
        }

        private int RandomIndice(int max) => RandomNumberGenerator.GetInt32(max);

        private bool CpfValido(string cpf)
        {
            if (cpf.Length != 11) return false;
            if (cpf.Distinct().Count() == 1) return false;

            var multiplicadores1 = new[] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            var multiplicadores2 = new[] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

            var tempCpf = cpf.Substring(0, 9);
            var soma = 0;
            for (int i = 0; i < 9; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicadores1[i];

            var resto = soma % 11;
            var digito1 = resto < 2 ? 0 : 11 - resto;

            tempCpf += digito1;
            soma = 0;
            for (int i = 0; i < 10; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicadores2[i];

            resto = soma % 11;
            var digito2 = resto < 2 ? 0 : 11 - resto;

            return cpf.EndsWith($"{digito1}{digito2}");
        }
    }
}