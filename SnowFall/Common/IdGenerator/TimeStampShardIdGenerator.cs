
using SnowFall.Common.IdGenerator;

namespace Application.Helper;

public class TimeStampShardIdGenerator : IIdGenerator<long>
{
    public const long Twepoch = 1288834974000L;

    // change from 5 to 3  
    private const int WorkerIdBits = 3;

    // change from 5 to 2  
    private const int DatacenterIdBits = 2;

    // change from 12 to 8  
    private const int SequenceBits = 8;

    private const long MaxWorkerId = -1L ^ (-1L << WorkerIdBits);

    private const long MaxDatacenterId = -1L ^ (-1L << DatacenterIdBits);

    private const long SequenceMask = -1L ^ (-1L << SequenceBits);

    private const int WorkerIdShift = SequenceBits;

    private const int DatacenterIdShift = SequenceBits + WorkerIdBits;

    public const int TimestampLeftShift = SequenceBits + WorkerIdBits + DatacenterIdBits;

    private long _sequence = 0L;
    private long _lastTimestamp = -1L;

    public long WorkerId { get; protected set; }

    public long DatacenterId { get; protected set; }

    public long Sequence
    {
        get { return _sequence; }
        internal set { _sequence = value; }
    }

    private readonly static object _lock = new object();

    public TimeStampShardIdGenerator(long workerId, long datacenterId, long sequence = 0L)
    {
        if (workerId > MaxWorkerId || workerId < 0)
        {
            throw new ArgumentException($"worker Id must greater than or equal 0 and less than or equal {MaxWorkerId}");
        }

        if (datacenterId > MaxDatacenterId || datacenterId < 0)
        {
            throw new ArgumentException($"datacenter Id must greater than or equal 0 and less than or equal {MaxDatacenterId}");
        }

        WorkerId = workerId;
        DatacenterId = datacenterId;
        _sequence = sequence;
    }

    public long GenerateId()
    {
        return NextId();
    }

    public long NextId()
    {
        lock (_lock)
        {
            var timestamp = TimeGen();
            if (timestamp < _lastTimestamp)
            {
                throw new Exception($"timestamp error");
            }

            if (_lastTimestamp == timestamp)
            {
                _sequence = (_sequence + 1) & SequenceMask;
                if (_sequence == 0)
                {
                    timestamp = TilNextMillis(_lastTimestamp);
                }
            }
            else
            {
                _sequence = 0;
            }

            _lastTimestamp = timestamp;
            return ((timestamp - Twepoch) << TimestampLeftShift) | (DatacenterId << DatacenterIdShift) | (WorkerId << WorkerIdShift) | _sequence;
        }
    }

    private long TilNextMillis(long lastTimestamp)
    {
        var timestamp = TimeGen();
        while (timestamp <= lastTimestamp)
        {
            timestamp = TimeGen();
        }

        return timestamp;
    }

    private long TimeGen()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
}