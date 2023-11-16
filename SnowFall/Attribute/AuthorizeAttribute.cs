
namespace SnowFall.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class AuthorizeAttribute : Attribute
{
    /// <summary>
    /// 리소스에 액세스할 수 있는 쉼표로 구분된 역할 목록을 가져오거나 설정.
    /// </summary>
    public string? Roles { get; set; } = string.Empty;
}
