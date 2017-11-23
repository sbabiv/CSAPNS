using System;

namespace CSAPNS.Apns.Response
{
    /// <summary>
    /// response data wrapper
    /// </summary>
    public class ResponseWrapper
    {
        public DateTime CreationDate { private set; get; }
        public MessageResponse MessageDataResp { private set; get; }
        public TransportResponse TransportDataResponse { private set; get; }

        public ResponseWrapper(MessageResponse messageDataResp, TransportResponse transportDataResp, DateTime? creationDate = null)
        {
            CreationDate = creationDate ?? DateTime.Now;

            MessageDataResp = messageDataResp;
            TransportDataResponse = transportDataResp;
        }
    }
}
