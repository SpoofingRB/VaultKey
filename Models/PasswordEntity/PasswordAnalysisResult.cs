namespace appointmentapi.Models.PasswordEntity
{
    public class PasswordAnalysisResult
    {
        public int Score { get; set; }
        public string Classificacao { get; set; } = string.Empty;
        public double EntropiaBits { get; set; }
        public string TempoParaQuebrar { get; set; } = string.Empty;
        public bool Comprometida { get; set; }
        public int QuantidadeVazamentos { get; set; }
        public List<string> PadroesDetectados { get; set; } = new List<string>();
        public List<string> Melhorias { get; set; } = new List<string>();
    }
}