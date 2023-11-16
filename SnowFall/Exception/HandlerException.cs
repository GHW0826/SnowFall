
namespace SnowFall.Exceptions;

/// <summary>
/// Handler에서 처리할 수 없는 경우가 발생될 때 throw되는 예외입니다.
/// </summary>
public class HandlerException : Exception
{
    /// <summary>
    /// HandlerException을 초기화 합니다.
    /// </summary>
    /// <param name="code">예외에 대한 이유를 정의하는 4자리의 코드입니다.</param>
    public HandlerException(string code)
        : base("요청을 처리할 수 없습니다.")
    {
        Code = code;
    }

    /// <summary>
    /// HandlerException을 초기화 합니다.
    /// </summary>
    /// <param name="code">예외에 대한 이유를 정의하는 코드입니다.</param>
    /// <param name="message">예외에 대한 이유를 설명하는 오류 메시지입니다.</param>
    public HandlerException(string code, string message)
        : base(message)
    {
        Code = code;
    }

    /// <summary>
    /// HandlerException을 초기화 합니다.
    /// </summary>
    /// <param name="code">예외에 대한 이유를 정의하는 4자리의 코드입니다.</param>
    /// <param name="innerException">현재 예외의 원인인 예외입니다.</param>
    public HandlerException(string code, Exception innerException)
        : base("요청을 처리할 수 없습니다.", innerException)
    {
        Code = code;
    }

    /// <summary>
    /// HandlerException을 초기화 합니다.
    /// </summary>
    /// <param name="code">예외에 대한 이유를 정의하는 4자리의 코드입니다.</param>
    /// <param name="message">예외에 대한 이유를 설명하는 오류 메시지입니다.</param>
    /// <param name="innerException">현재 예외의 원인인 예외입니다.</param>
    public HandlerException(string code, string message, Exception innerException)
        : base(message, innerException)
    {
        Code = code;
    }

    /// <summary>
    /// 예외 코드를 가져옵니다.
    /// </summary>
    public string Code { get; set; }

    public override string ToString()
    {
        return string.IsNullOrEmpty(Code) ? base.ToString() : $"Exception Code : {Code}\r\n {base.ToString()}";
    }

    public HandlerException WithData(object key, object value)
    {
        this.Data[key] = value;
        return this;
    }
}