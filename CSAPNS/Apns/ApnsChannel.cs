using System;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace CSAPNS.Apns
{
    public delegate void ResponseHandler(ResponseWrapper responseWrapper);

    public class ApnsChannel
    {
        public readonly Guid UID;

        private TcpClient _client;
        private SslStream _channel;
        private readonly ApnsChannelState _channelState;

        private readonly X509Certificate2Collection _certificates;
        private bool _connected;
        private readonly object _channelLock;

        private readonly string _host;
        private readonly int _port;
        
        public event ResponseHandler OnResponseHandler;
        
        public ApnsChannel(X509Certificate2 cert, string host, int port)
        {
            _host = host;
            _port = port;

            UID = Guid.NewGuid();

            _channelLock = new object();
            _connected = false;
            _certificates = new X509Certificate2Collection(cert);
            _channelState = new ApnsChannelState();

            Consuming();
        }

        public void Send(ApnsPush push)
        {
            try
            {
                lock (_channelLock)
                {
                    Connect();

                    _channel.Write(push.ToData());
                    _channel.Flush();
                    
                    push.Sent();
                    _channelState.Enqueue(push);

                    //LogFactory.Instance.Debug(GetType(), string.Format("ID:{0} APNS PUSH SENT: {1}", UID, push.SentAt.ToString("HH:mm:ss:fff")));
                }
            }
            catch(Exception exc)
            {
                //LogFactory.Instance.Debug(GetType(), string.Format("ID:{0} APNS SEND EXC: {1}", UID, exc));
                OnResponseHandler?.Invoke(new ResponseWrapper(new MessageResponse(push.Instance), new TransportResponse(HttpStatusCode.BadRequest)));
            }
        }

        private void Consuming()
        {
            Task.Factory.StartNew(() =>
            {
                foreach (var resp in _channelState.ResponseQueue.GetConsumingEnumerable())
                {
                    var msg = string.Format(resp.MessageDataResp.Body != null
                        ? "sentat: {0}, invalid token: {1}"
                        : "sentat: {0}, token: {1}", resp.CreationDate.ToString("HH:mm:ss:fff"),
                        resp.MessageDataResp.Source.Token);

                    //LogFactory.Instance.Debug(GetType(), msg);

                    OnResponseHandler?.Invoke(resp);
                }
            }, TaskCreationOptions.LongRunning);

            Task.Factory.StartNew(() =>
            {
                foreach (var item in _channelState.ResendQueue.GetConsumingEnumerable())
                {
                    Task.Run(() => { Send(new ApnsPush(item.Instance)); });
                }
            }, TaskCreationOptions.LongRunning);
        }

        private void Connect()
        {
            if (_client != null && _client.Client.IsConnected() == false)
            {
                //LogFactory.Instance.Debug(GetType(), string.Format("ID:{0} APNS CHANNEL DISCONNECTED", UID));
                _connected = false;
            }

            if (_connected == false)
            {
                if(_client != null) Disconnect();
                
                //LogFactory.Instance.Debug(GetType(), string.Format("ID:{0} APNS CHANNEL CONNECT", UID));

                _client = new TcpClient(_host, _port);
                _client.SetSocketKeepAliveValues(20 * 60 * 1000, 30 * 1000); 

                _channel = new SslStream(_client.GetStream(), false, ValidateServerCertificate);
                _channel.AuthenticateAsClient(_host, _certificates, SslProtocols.Tls, true);

                _connected = true;
                
                Reader();
            }
        }

        private void Disconnect()
        {
            _channel.Close();
            _client.Close();

            _channel = null;
            _client = null;

            //LogFactory.Instance.Debug(GetType(), string.Format("ID:{0} APNS CHANNEL DISCONNECT", UID));
        }

        public void Close()
        {
            lock (_channelLock)
            {
                if (_channel != null && _client != null) Disconnect();

                _connected = false;

                //LogFactory.Instance.Debug(GetType(), string.Format("ID:{0} APNS CHANNEL CLOSE", UID));
            }
        }

        private void Reader()
        {
            try
            {
                var buffer = new byte[6];
                _channel.BeginRead(buffer, 0, buffer.Length, ar =>
                {
                    //LogFactory.Instance.Debug(GetType(), string.Format("ID:{0} APNS READER", UID));
                    lock (_channelState)
                    {
                        try
                        {
                            var result = _channel.EndRead(ar);
                            if (result == 6) 
                                _channelState.Invalid(new ApnsErrorResponse(buffer));
                        }
                        catch (Exception exc)
                        {
                            //LogFactory.Instance.Debug(GetType(), string.Format("ID:{0} APNS END READER EXC: {1}", UID, exc.ToString()));
                        }
                        finally
                        {
                            _connected = false;
                        }
                    }
                }, _channel);
            }
            catch(Exception exc)
            {
                //LogFactory.Instance.Debug(GetType(), string.Format("ID:{0} APNS READER EXC: {1}", UID, exc));
                lock (_channelState)
                {
                    _connected = false;    
                }
            }
        }

        private bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return sslPolicyErrors == SslPolicyErrors.None;
        }
    }

    public static class TcpExtensions
    {
        public static void SetSocketKeepAliveValues(this TcpClient tcpc, int KeepAliveTime, int KeepAliveInterval)
        {
            uint dummy = 0; 
            byte[] inOptionValues = new byte[System.Runtime.InteropServices.Marshal.SizeOf(dummy) * 3]; 
            bool OnOff = true;

            BitConverter.GetBytes((uint)(OnOff ? 1 : 0)).CopyTo(inOptionValues, 0);
            BitConverter.GetBytes((uint)KeepAliveTime).CopyTo(inOptionValues, System.Runtime.InteropServices.Marshal.SizeOf(dummy));
            BitConverter.GetBytes((uint)KeepAliveInterval).CopyTo(inOptionValues, System.Runtime.InteropServices.Marshal.SizeOf(dummy) * 2);
            
            tcpc.Client.IOControl(IOControlCode.KeepAliveValues, inOptionValues, null);
        }

        public static bool IsConnected(this Socket socket)
        {
            try
            {
                return !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);
            }
            catch (SocketException)
            {
                return false;
            }
        }
    }
}
