using System.Threading.Tasks;

namespace Centrifugo.Sample.TokenProvider
{
    public interface ICentrifugoTokenProvider
    {
        Task<string> GenerateTokenAsync(
            string clientId,
            string? clientProvidedInfo = null
        );
    }
}