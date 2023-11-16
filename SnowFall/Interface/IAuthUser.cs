
namespace SnowFall.Interfaces;

public interface IAuthUser
{
    bool isAuthenticated { get; }

    string pid { get; }

    string name { get; }

    string userIp { get; }

    /// <summary>
    /// 리소스에 액세스할 수 있는 쉼표로 구분된 role 목록.
    /// </summary>
    string roles { get; }
}
