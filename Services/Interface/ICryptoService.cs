
namespace appointmentapi.Services.Interface
{
    public interface ICryptoService
    {
        (string cifrado, string iv) Criptografar(string textoPuro);
        string Descriptografar(string cifrado, string iv);
    }
}