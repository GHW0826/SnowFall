
namespace SnowFall.Common;

public class BaseConfig
{
    private Dictionary<string, string> _settings { get; set; } = new Dictionary<string, string>();

    public string this[string key]
    {
        get { return _settings[key]; }
        set { _settings[key] = value; }
    }

    public enum Environment
    {
        Local,
        Dev,
        QA,
        Stage,
        Real
    }

    public string ApplicationName { get; set; } = string.Empty;

    public Environment Env { get; set; }

    public string KeyVault { get; set; } = string.Empty;

    public string EnableCors { get; set; } = string.Empty;

    public string TitleId { get; set; } = string.Empty;
}
