
namespace SnowFall.Exceptions;

public class AuthorizationException : Exception
{
    /// <summary>
    /// 사용되지 않습니다. 
    /// </summary>
    /// <param name="code"></param>
    /// <param name="message"></param>
    /// <param name="innerException"></param>
    [Obsolete("Code를 지정하는 생성자는 더 이상 사용할 수 없습니다.")]
    public AuthorizationException(string code, string message, Exception? innerException)
        : base(message, innerException)
    { }

    /// <summary>
    /// 사용되지 않습니다. 
    /// </summary>
    /// <param name="code"></param>
    /// <param name="message"></param>
    [Obsolete("Code를 지정하는 생성자는 더 이상 사용할 수 없습니다.")]
    private AuthorizationException(string code, string message)
        : this(code, message, null)
    { }

    /// <summary>
    /// AuthorizationException 를 초기화 합니다.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="innerException"></param>
    public AuthorizationException(string message, Exception? innerException)
        : base(message, innerException)
    { }

    /// <summary>
    /// AuthorizationException 를 초기화 합니다.
    /// </summary>
    /// <param name="message"></param>
    public AuthorizationException(string message)
        : this(message, innerException: null)
    { }

    /// <summary>
    /// AuthorizationException 를 초기화 합니다.
    /// </summary>
    /// <param name="innerException"></param>
    public AuthorizationException(Exception? innerException)
        : this("인증정보가 없거나 정확하지 않습니다.", innerException)
    { }

    /// <summary>
    /// AuthorizationException 를 초기화 합니다.
    /// </summary>
    public AuthorizationException()
        : this(innerException : null)
    { }

    /// <summary>
    /// 예외코드를 가져옵니다.
    /// </summary>
    public string Code => "1000";

    public override string ToString()
    {
        return string.IsNullOrEmpty(Code) ? base.ToString() : $"Exception Code : {Code}\r\n {base.ToString()}";
    }

    public AuthorizationException WithData(object key, object value)
    {
        this.Data[key] = value;
        return this;
    }
}
