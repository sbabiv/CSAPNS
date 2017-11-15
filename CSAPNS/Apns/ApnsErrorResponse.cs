using System;
using System.Linq;

namespace CSAPNS.Apns
{
    public class ApnsErrorResponse
    {
        public int Status { get; private set; }
        public int NID { get; private set; }

        public ApnsErrorResponse(byte[] response)
        {
            Status = Convert.ToInt32(response[1]);
            NID = BitConverter.ToInt32(response.Skip(2).Take(4).Reverse().ToArray(), 0);
        }
    }
}
