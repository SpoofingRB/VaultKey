using System.Text.RegularExpressions;

namespace appointmentapi.Services
{
    public class PasswordPatternAnalyzer
    {
        private static readonly string[] SequenciasTeclado =
        {
            "qwertyuiop", "asdfghjkl", "zxcvbnm",
            "1234567890", "0987654321",
            "qwertyuiop´[", "asdfghjklç~",
            "!@#$%¨&*()_+", "\"!@#$%¨&*(" 
        };

        public List<string> DetectarPadroes(
            string senha,
            HashSet<string> senhasComuns,
            HashSet<string> nomesComuns,
            HashSet<string> palavrasComuns)
        {
            var padroes = new List<string>();

            if (ContemSequenciaTeclado(senha))
                padroes.Add("Sequência de teclado (ex: qwerty, asdf)");

            if (ContemSequenciaAlfabeticaOuNumerica(senha))
                padroes.Add("Sequência alfabética ou numérica (ex: abcde, 12345)");

            if (ContemRepeticaoExcessiva(senha))
                padroes.Add("Caracteres repetidos em excesso");

            if (ContemBlocoRepetido(senha))
                padroes.Add("Bloco repetido (ex: abcabc, 1212)");

            if (ContemDataComum(senha))
                padroes.Add("Padrão de data (ex: ano ou data de nascimento)");

            if (ContemLeetSpeakDeSenhaComum(senha, senhasComuns))
                padroes.Add("Variação disfarçada de senha comum (ex: P@ssw0rd)");

            if (ContemPalavrasConcatenadas(senha))
                padroes.Add("Parece concatenação de nomes/palavras (ex: NomeSobrenome) — atacantes testam combinações de nomes reais, não caracteres aleatórios");

            if (ContemNomeMaisAno(senha, nomesComuns))
                padroes.Add("Padrão \"Nome + Ano\" (ex: Joao1998) — um dos padrões mais comuns e testados primeiro em ataques reais");

            if (ContemPalavraComumPortugues(senha, palavrasComuns))
                padroes.Add("Contém palavra comum do português (ex: futebol, familia) — palavras de dicionário são testadas antes de combinações aleatórias");

            if (ContemSubstituicaoDeEspaco(senha))
                padroes.Add("Parece frase com separador no lugar de espaço (ex: meu.cachorro.2020)");

            return padroes;
        }

        private bool ContemSequenciaTeclado(string senha)
        {
            var senhaLower = senha.ToLower();
            foreach (var seq in SequenciasTeclado)
            {
                for (int i = 0; i <= seq.Length - 4; i++)
                {
                    var trecho = seq.Substring(i, 4);
                    if (senhaLower.Contains(trecho))
                        return true;
                }
            }
            return false;
        }

        private bool ContemSequenciaAlfabeticaOuNumerica(string senha)
        {
            for (int i = 0; i < senha.Length - 3; i++)
            {
                bool crescente = true;
                bool decrescente = true;

                for (int j = 0; j < 3; j++)
                {
                    if (senha[i + j + 1] - senha[i + j] != 1) crescente = false;
                    if (senha[i + j] - senha[i + j + 1] != 1) decrescente = false;
                }

                if (crescente || decrescente) return true;
            }
            return false;
        }

        private bool ContemRepeticaoExcessiva(string senha)
        {
            return Regex.IsMatch(senha, @"(.)\1{2,}");
        }

        private bool ContemBlocoRepetido(string senha)
        {
            for (int tamanhoBloco = 2; tamanhoBloco <= 4; tamanhoBloco++)
            {
                if (senha.Length < tamanhoBloco * 2) continue;

                for (int i = 0; i <= senha.Length - tamanhoBloco * 2; i++)
                {
                    var bloco1 = senha.Substring(i, tamanhoBloco);
                    var bloco2 = senha.Substring(i + tamanhoBloco, tamanhoBloco);

                    if (bloco1 == bloco2)
                        return true;
                }
            }
            return false;
        }

        private bool ContemDataComum(string senha)
        {
            return Regex.IsMatch(senha, @"(19[5-9]\d|20[0-2]\d)") ||
                   Regex.IsMatch(senha, @"(0[1-9]|[12]\d|3[01])(0[1-9]|1[0-2])(19|20)\d{2}");
        }

        private bool ContemLeetSpeakDeSenhaComum(string senha, HashSet<string> senhasComuns)
        {
            var normalizada = senha.ToLower()
                .Replace("@", "a")
                .Replace("4", "a")
                .Replace("3", "e")
                .Replace("1", "i")
                .Replace("!", "i")
                .Replace("0", "o")
                .Replace("$", "s")
                .Replace("5", "s")
                .Replace("7", "t");

            return senhasComuns.Contains(normalizada);
        }

        private bool ContemPalavrasConcatenadas(string senha)
        {
            var matches = Regex.Matches(senha, @"[A-ZÀ-Ý][a-zà-ÿ]{2,}");
            return matches.Count >= 2;
        }

        private bool ContemNomeMaisAno(string senha, HashSet<string> nomesComuns)
        {

            var match = Regex.Match(senha, @"^([a-zA-ZÀ-ÿ]+)(\d{2,4})$");
            if (!match.Success) return false;

            var parteTexto = match.Groups[1].Value.ToLower();
            return nomesComuns.Contains(parteTexto);
        }

        private bool ContemPalavraComumPortugues(string senha, HashSet<string> palavrasComuns)
        {
            var senhaLower = senha.ToLower();
            var blocosTexto = Regex.Split(senhaLower, @"[^a-zà-ÿ]+")
                .Where(b => b.Length >= 4);

            foreach (var bloco in blocosTexto)
            {
                if (palavrasComuns.Contains(bloco))
                    return true;
            }
            return false;
        }

        private bool ContemSubstituicaoDeEspaco(string senha)
        {
            return Regex.IsMatch(senha, @"^[a-zA-ZÀ-ÿ]+[._-][a-zA-ZÀ-ÿ]+[._-]?\d*$");
        }
    }
}