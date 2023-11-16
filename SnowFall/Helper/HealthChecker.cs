
using Microsoft.AspNetCore.Hosting;

namespace SnowFall.Helper;
public class HealthChecker
{
    private IWebHostEnvironment _environment;
    public IWebHostEnvironment Environment => _environment;

    public HealthChecker(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public string Get()
    {
        string path = Path.Combine(_environment.ContentRootPath, "healthcheck.html");
        return (File.Exists(path) ? File.ReadAllText(path) : "down").ToLower();
    }

    public bool Is(string text)
    {
        return Get() == text.ToLower();
    }

    public bool IsUp()
    {
        return Is("up");
    }

    public bool IsDown()
    {
        return Is("down");
    }
}
