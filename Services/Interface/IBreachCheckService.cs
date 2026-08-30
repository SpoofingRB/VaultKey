namespace appointmentapi.Services.Interface
{
    public interface IBreachCheckService
    {
        Task<int> VerificarVazamentoAsync(string senha);
    }
}