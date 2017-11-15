using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace CSAPNS.Apns
{
    public class ApnsPush : ChannelPushBase
    {
        #region members
        /// <summary>
        /// time to live in hours
        /// </summary>
        private const int Ttl = 12;

        private readonly int _nid;
        public int NID
        {
            get { return _nid; }
        }

        private DateTime _sentAt;
        public DateTime SentAt
        {
            get { return _sentAt; }
        }

        public void Sent()
        {
            _sentAt = DateTime.Now;
        }

        private static int _nextId;
        private static int SequenceId()
        {
            Interlocked.CompareExchange(ref _nextId, 0, Int32.MaxValue);
            return Interlocked.Increment(ref _nextId);
        }
        #endregion members

        public ApnsPush(Push push) : base(push)
        {
            _nid = SequenceId();
        }
        
        public override byte[] ToData()
        {
            var deviceToken = HexToData(Instance.Token);
            var payload = Encoding.UTF8.GetBytes(Instance.Token);
            
            var nid = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(_nid));
            var expired = GetExpired(Ttl);
            var data = new List<byte>();

            /*device token*/
            data.Add(1);
            data.Add(0);
            data.Add(32);
            data.AddRange(deviceToken);

            /*payload*/
            data.Add(2);
            data.AddRange(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)payload.Length)));
            data.AddRange(payload);

            /*identifier*/
            data.Add(3);
            data.Add(0);
            data.Add(4);
            data.AddRange(nid);

            /*Expiration*/
            data.Add(4);
            data.Add(0);
            data.Add(4);
            data.AddRange(expired);

            /*Priority*/
            data.Add(5);
            data.Add(0);
            data.Add(1);
            data.Add(10);

            /*frame. 2 command notification format*/
            var frame = new List<byte>();
            frame.Add(2);
            frame.AddRange(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(data.Count)));
            frame.AddRange(data);

            return frame.ToArray();
        }

        private byte[] GetExpired(int hours)
        {
            var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            /*var concreteExpireDateUtc = DateTime.UtcNow.AddDays(days).ToUniversalTime();*/
            var concreteExpireDateUtc = DateTime.UtcNow.AddHours(hours).ToUniversalTime();
            var epochTimeSpan = concreteExpireDateUtc - unixEpoch;

            return BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int) epochTimeSpan.TotalSeconds));
        }

        private byte[] HexToData(string hexString)
        {
            if (hexString == null) return null;

            if (hexString.Length%2 == 1)
                hexString = '0' + hexString;

            byte[] data = new byte[hexString.Length/2];

            for (int i = 0; i < data.Length; i++)
                data[i] = Convert.ToByte(hexString.Substring(i*2, 2), 16);

            return data;
        }
    }
}
