using System;
using System.Collections.Generic;
using System.Threading;
using System.Timers;
using Timer = System.Timers.Timer;

namespace CSAPNS.Apns
{
    public class ApnsGate : IGate
    {
        private readonly IResponseParser _responseParser;
        private readonly List<ApnsChannel> _channels;
        private readonly GateSettings _settings;
        private int _index;

        public event ProcessedHandler OnProcessed;

        public ApnsGate(GateSettings settings, IResponseParser responseParser)
        {
            var timer = new Timer(TimeSpan.FromHours(1).TotalMilliseconds);

            _settings = settings;
            _responseParser = responseParser;
            _channels = new List<ApnsChannel>();

            for (int i = 0; i < settings.WorkerPoolSize; i++)
            {
                var channel = new ApnsChannel(settings.Certificate, settings.Host, settings.Port);
                channel.OnResponseHandler += OnResponseHandler;
                _channels.Add(channel);
            }

            timer.Elapsed += CloseChannels;
            timer.Start();
        }

        public void Enqueue(Push push)
        {
            GetChannel().Send(new ApnsPush(push));
        }

        public void Close()
        {
            _channels.ForEach(channel => channel.Close());
        }

        private ApnsChannel GetChannel()
        {
            var channel = _channels[_index%_settings.WorkerPoolSize];
            
            Interlocked.CompareExchange(ref _index, 0, Int32.MaxValue);
            Interlocked.Increment(ref _index);

            return channel;
        }

        private void OnResponseHandler(ResponseWrapper responseWrapper)
        {
            OnProcessed?.Invoke(_responseParser.Parse(responseWrapper));
        }

        private void CloseChannels(object sender, ElapsedEventArgs e)
        {
            if (DateTime.Now.Hour == 3) 
                _channels.ForEach(channel => channel.Close());
        }
    }
}
