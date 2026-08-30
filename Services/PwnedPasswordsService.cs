using System.Security.Cryptography;
using System.Text;
using appointmentapi.Services.Interface;

namespace appointmentapi.Services
{
    public class PwnedPasswordsService : IBreachCheckService
    {
        private readonly HttpClient _httpClient;

        public PwnedPasswordsService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://api.pwnedpasswords.com/");
        }

        public async Task<int> VerificarVazamentoAsync(string senha)
        {
            if (string.IsNullOrEmpty(senha)) return 0;

            var hash = CalcularSha1(senha);
            var prefixo = hash.Substring(0, 5);
            var sufixo = hash.Substring(5);

            try
            {
                var resposta = await _httpClient.GetStringAsync($"range/{prefixo}");
                var linhas = resposta.Split('\n');

                foreach (var linha in linhas)
                {
                    var partes = linha.Split(':');
                    if (partes.Length != 2) continue;

                    if (partes[0].Trim().Equals(sufixo, StringComparison.OrdinalIgnoreCase))
                    {
                        return int.Parse(partes[1].Trim());
                    }
                }

                return 0;
            }
            catch (HttpRequestException)
            {
                return -1;
            }
        }

        private string CalcularSha1(string input)
        {
            using var sha1 = SHA1.Create();
            var bytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
            var sb = new StringBuilder();
            foreach (var b in bytes)
                sb.Append(b.ToString("X2"));
            return sb.ToString();
        }
    }
}