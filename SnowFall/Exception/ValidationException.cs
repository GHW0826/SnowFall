
using FluentValidation.Results;
using System.Text.Json;

namespace SnowFall.Exceptions;

public class ValidationException : Exception
{
    public ValidationException()
        : base("하나 이상의 유효하지 않은 입력값이 있습니다.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(IEnumerable<ValidationFailure> failures)
        : this()
    {
        Errors = failures
            .GroupBy(e => JsonNamingPolicy.CamelCase.ConvertName(e.PropertyName), e => e.ErrorMessage)
            .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray());

    }

    public IDictionary<string, string[]> Errors { get; }

    public string Code => "4000";

    public override string ToString()
    {
        return string.IsNullOrEmpty(Code) ? base.ToString() : $"Exception Code : {Code}\r\n {base.ToString()}";
    }

    public ValidationException WithData(object key, object value)
    {
        this.Data[key] = value;
        return this;
    }
}