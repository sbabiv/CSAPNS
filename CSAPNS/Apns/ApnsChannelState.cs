using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace CSAPNS.Apns
{
    class ApnsChannelState
    {
        private readonly object _sync;
        private readonly Queue<ApnsPush> _storage;

        public readonly BlockingCollection<ResponseWrapper> ResponseQueue;
        public readonly BlockingCollection<ApnsPush> ResendQueue;

        public ApnsChannelState()
        {
            ResendQueue = new BlockingCollection<ApnsPush>();
            ResponseQueue = new BlockingCollection<ResponseWrapper>();

            _sync = new object();
            _storage = new Queue<ApnsPush>();

            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(10));
                    if (_storage.Count > 0) Cleanup();
                }
            }, TaskCreationOptions.LongRunning);
        }

        public void Enqueue(ApnsPush push)
        {
            lock (_sync)
            {
                _storage.Enqueue(push);
            }
        }

        public void Invalid(ApnsErrorResponse invalid)
        {
            lock (_sync)
            {
                try
                {
                    if (_storage.Count == 0)
                        throw new Exception("channel state is empty");
                    do
                    {
                        var value = _storage.Dequeue();
                        if (value.NID == invalid.NID)
                        {
                            ResponseQueue.Add(new ResponseWrapper(new MessageResponse(value.Instance, new[] {(byte) invalid.Status}), new TransportResponse(HttpStatusCode.OK), value.SentAt));
                            break;
                        }
                        ResponseQueue.Add(new ResponseWrapper(new MessageResponse(value.Instance), new TransportResponse(HttpStatusCode.OK), value.SentAt));

                    } while (_storage.Count > 0);

                    /*resend push queue*/
                    while (_storage.Count > 0) 
                        ResendQueue.Add(_storage.Dequeue());
                }
                catch (Exception e)
                {
                    //LogFactory.Instance.Debug(GetType(), e.Message);
                }   
            }
        }

        private void Cleanup()
        {
            lock (_sync)
            {
                var now = DateTime.Now;
                try
                {
                    while (_storage.Count > 0)
                    {
                        var value = _storage.Peek();
                        if ((now - value.SentAt).TotalSeconds < 7) break;

                        value = _storage.Dequeue();
                        ResponseQueue.Add(new ResponseWrapper(new MessageResponse(value.Instance), new TransportResponse(HttpStatusCode.OK), value.SentAt));
                    }
                }
                catch (InvalidOperationException invalidOperationException)
                {
                    //LogFactory.Instance.Debug(GetType(), "cleanup exc: " + invalidOperationException);
                }
            }
        }
    }
}
