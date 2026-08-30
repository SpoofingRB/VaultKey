using appointmentapi.DTOs;
using appointmentapi.Models.PasswordEntity;
using appointmentapi.Services.Interface;
using System.Linq;
using System.Text.RegularExpressions;

namespace appointmentapi.Services
{
    public class PasswordService : IPasswordService
    {
        private readonly HashSet<string> _senhasComuns;
        private readonly HashSet<string> _nomesComuns;
        private readonly HashSet<string> _palavrasComuns;
        private readonly PasswordPatternAnalyzer _patternAnalyzer = new();
        private readonly IBreachCheckService _breachCheck;

        public PasswordService(IWebHostEnvironment env, IBreachCheckService breachCheck)
        {
            _senhasComuns = CarregarArquivo(env, "senhas-comuns.txt");
            _nomesComuns = CarregarArquivo(env, "nomes-comuns.txt");
            _palavrasComuns = CarregarArquivo(env, "palavras-comuns-pt.txt");

            _breachCheck = breachCheck;
        }

        private HashSet<string> CarregarArquivo(IWebHostEnvironment env, string nomeArquivo)
        {
            var caminho = Path.Combine(env.ContentRootPath, "Data", nomeArquivo);
            return File.Exists(caminho)
                ? new HashSet<string>(File.ReadAllLines(caminho).Select(l => l.Trim().ToLower()))
                : new HashSet<string>();
        }

        public async Task<PasswordAnalysisResult> AnalisarAsync(PasswordCheckDTO dto)
        {
            var senha = dto.Password ?? string.Empty;

            var entropia = CalcularEntropia(senha);
            var padroes = _patternAnalyzer.DetectarPadroes(senha, _senhasComuns, _nomesComuns, _palavrasComuns);

            var qtdVazamentos = await _breachCheck.VerificarVazamentoAsync(senha);
            var comprometida = qtdVazamentos > 0;

            var (score, classificacao) = ClassificarForca(entropia, comprometida, padroes);

            return new PasswordAnalysisResult
            {
                Score = score,
                Classificacao = classificacao,
                EntropiaBits = entropia,
                TempoParaQuebrar = EstimarTempoQuebra(entropia, comprometida, padroes),
                Comprometida = comprometida,
                QuantidadeVazamentos = qtdVazamentos > 0 ? qtdVazamentos : 0,
                PadroesDetectados = padroes,
                Melhorias = SugerirMelhorias(senha, comprometida, padroes, qtdVazamentos)
            };
        }

        private double CalcularEntropia(string senha)
        {
            int espacoCaracteres = 0;
            if (Regex.IsMatch(senha, "[a-z]")) espacoCaracteres += 26;
            if (Regex.IsMatch(senha, "[A-Z]")) espacoCaracteres += 26;
            if (Regex.IsMatch(senha, "[0-9]")) espacoCaracteres += 10;
            if (Regex.IsMatch(senha, "[^a-zA-Z0-9]")) espacoCaracteres += 32;

            if (espacoCaracteres == 0 || senha.Length == 0) return 0;

            return senha.Length * Math.Log2(espacoCaracteres);
        }

        private (int score, string classificacao) ClassificarForca(double entropia, bool comprometida, List<string> padroes)
        {
            if (comprometida) return (0, "Fraca (vazada)");

            bool padraoForte = padroes.Any(p =>
                p.Contains("concatenação de nomes") ||
                p.Contains("Nome + Ano") ||
                p.Contains("palavra comum do português"));

            if (padraoForte) return (1, "Fraca (baseada em nomes/palavras reais)");

            if (padroes.Count >= 2) return (0, "Fraca (padrões previsíveis)");
            if (padroes.Count == 1 && entropia < 60) return (1, "Fraca (padrão previsível)");

            if (entropia < 28) return (0, "Muito Fraca");
            if (entropia < 36) return (1, "Fraca");
            if (entropia < 60) return (2, "Média");
            if (entropia < 80) return (3, "Forte");
            return (4, "Muito Forte");
        }

        private string EstimarTempoQuebra(double entropiaBits, bool comprometida, List<string> padroes)
        {
            bool padraoForte = padroes.Any(p =>
                p.Contains("concatenação de nomes") ||
                p.Contains("Nome + Ano") ||
                p.Contains("palavra comum do português"));

            if (comprometida || padroes.Count >= 2 || padraoForte)
                return "Minutos a horas (ataque de dicionário)";

            const double tentativasPorSegundo = 10_000_000_000;
            double combinacoes = Math.Pow(2, entropiaBits);
            double segundos = combinacoes / tentativasPorSegundo;

            if (segundos < 1) return "Instantâneo";
            if (segundos < 60) return $"{segundos:F0} segundos";
            if (segundos < 3600) return $"{segundos / 60:F0} minutos";
            if (segundos < 86400) return $"{segundos / 3600:F0} horas";
            if (segundos < 31536000) return $"{segundos / 86400:F0} dias";

            double anos = segundos / 31536000;
            return anos > 1_000_000 ? "Milhões de anos" : $"{anos:N0} anos";
        }

        private List<string> SugerirMelhorias(string senha, bool comprometida, List<string> padroes, int qtdVazamentos)
        {
            var sugestoes = new List<string>();

            if (comprometida)
                sugestoes.Add($"Essa senha já apareceu em {qtdVazamentos:N0} vazamentos conhecidos. Troque imediatamente.");
            else if (qtdVazamentos == -1)
                sugestoes.Add("Não foi possível verificar vazamentos no momento (serviço indisponível).");

            foreach (var padrao in padroes)
                sugestoes.Add($"Detectado: {padrao}");

            if (senha.Length < 12)
                sugestoes.Add("Use pelo menos 12 caracteres.");
            if (!Regex.IsMatch(senha, "[A-Z]"))
                sugestoes.Add("Adicione letras maiúsculas.");
            if (!Regex.IsMatch(senha, "[0-9]"))
                sugestoes.Add("Adicione números.");
            if (!Regex.IsMatch(senha, "[^a-zA-Z0-9]"))
                sugestoes.Add("Adicione símbolos (ex: !@#$%).");

            return sugestoes;
        }
    }
}