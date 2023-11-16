
namespace SnowFall.Http;

public abstract class BaseClient
{
    private readonly HttpClient _client; 
    
    protected BaseClient(HttpClient client)
    {
        _client = client;
    }
}
