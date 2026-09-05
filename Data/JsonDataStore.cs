using System.Text.Json;

namespace appointmentapi.Data
{
    public class JsonDataStore
    {
        private readonly string _pastaDados;
        private static readonly SemaphoreSlim _lock = new(1, 1);

        public JsonDataStore(IWebHostEnvironment env)
        {
            _pastaDados = Path.Combine(env.ContentRootPath, "Data", "Storage");
            Directory.CreateDirectory(_pastaDados);
        }

        public async Task<List<T>> LerAsync<T>(string nomeArquivo)
        {
            var caminho = Path.Combine(_pastaDados, nomeArquivo);

            await _lock.WaitAsync();
            try
            {
                if (!File.Exists(caminho)) return new List<T>();

                var json = await File.ReadAllTextAsync(caminho);
                if (string.IsNullOrWhiteSpace(json)) return new List<T>();

                return JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task SalvarAsync<T>(string nomeArquivo, List<T> dados)
        {
            var caminho = Path.Combine(_pastaDados, nomeArquivo);

            await _lock.WaitAsync();
            try
            {
                var json = JsonSerializer.Serialize(dados, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(caminho, json);
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}