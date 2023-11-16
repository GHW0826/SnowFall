
namespace SnowFall.Model;

public class ResponseModel
{
    public int code { get; set; }
    public string msg { get; set; } = string.Empty;
    public Object? data { get; set; }
}
