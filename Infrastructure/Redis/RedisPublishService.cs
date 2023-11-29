using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Redis;

public class RedisPublishService
{
    private readonly ISubscriber Subscriber;

    public RedisPublishService(ISubscriber subscriber)
    {
        Subscriber = subscriber;
    }

    /// <summary>
    /// Subscriber에게 메세지를 보냄.
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="message"></param>
    /// <returns>수신받은 수</returns>
    public async Task<long> PublishAsync(string channelString, string message)
    {
        RedisChannel channel = new (channelString, RedisChannel.PatternMode.Auto);
        return await Subscriber.PublishAsync(channel, message);
    }
}
