using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Redis;

public class RedisSubscribeService
{
    private readonly ISubscriber Subscriber;

    public RedisSubscribeService(ISubscriber subscriber)
    {
        Subscriber = subscriber;
    }

    /// <summary>
    /// Redis Publish 메세지를 받아 공통 설정값을 새로 불러옴.
    /// </summary>
    public async Task SubscribeAsync(string channelString)
    {
        RedisChannel subChannel = new(channelString, RedisChannel.PatternMode.Auto);
        await Subscriber.SubscribeAsync(subChannel, (channel, message) =>
        {
            Console.WriteLine("Message received from this channel : " + message + " : " + channel);

            //메세지에 따라 처리

        });
    }
}
