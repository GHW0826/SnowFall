
namespace SnowFall.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class CachingAttribute : Attribute
{
    /// <summary>
    /// Query나 Command 출력을 캐시하는 데 사용하는 정보를 정의.
    /// 60초 후 캐시가 만료.
    /// </summary>
    public CachingAttribute()
        : this(String.Empty, 60)
    { }

    /// <summary>
    /// Query나 Command 출력을 캐시하는 데 사용하는 정보를 정의.
    /// </summary>
    /// <param name="interval">캐시가 만료되는 간격(초). 10초에서 86400초(하루)까지 설정할 수 있으며 범위를 벗어나면 최대값이나 최소값으로 조정됨.</param>
    public CachingAttribute(uint interval)
        : this(string.Empty, interval)
    { }

    /// <summary>
    /// Query나 Command 출력을 캐시하는 데 사용하는 정보를 정의.
    /// </summary>
    /// <param name="cacheKey">항목을 식별하는 문자열</param>
    /// <param name="interval">캐시가 만료되는 간격(초). 10초에서 86400초(하루)까지 설정할 수 있으며 범위를 벗어나면 최대값이나 최소값으로 조정됨.</param>
    public CachingAttribute(string cacheKey, uint interval)
    {
        if (interval < 10)//min. 캐시 유지 기간은 최소 10초 이상으로 강제한다.
            interval = 10;// throw new ArgumentOutOfRangeException("");

        if (interval > 60 * 60 * 24)//max. 캐시 유지 기간은 최대 하루 이하로 강제한다.
            interval = 60 * 60 * 24;// throw new ArgumentOutOfRangeException("");

        CacheKey = cacheKey;
        Interval = interval;
    }

    /// <summary>
    /// 항목을 식별하는 문자열을 가져옴.
    /// </summary>
    public string CacheKey { get; }

    /// <summary>
    /// 캐시가 만료되는 간격(초)을 가져옴.
    /// </summary>
    public uint Interval { get; }

    /// <summary>
    /// 캐시된 항목의 절대 만료 시간을 반환.
    /// </summary>
    /// <returns></returns>
    public DateTimeOffset GetAbsoluteExpiration()
    {
        int currentSeconds = DateTime.Now.Hour * 60 * 60;   //시
        currentSeconds += DateTime.Now.Minute * 60;         //분
        currentSeconds += DateTime.Now.Second;              //초

        return DateTime.Now.AddSeconds(Interval - (currentSeconds % Interval));
    }
}

