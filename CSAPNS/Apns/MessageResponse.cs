namespace CSAPNS.Apns
{
    public class MessageResponse
    {
        /// <summary>
        /// source push notification
        /// </summary>
        public Push Source { get; set; }

        /// <summary>
        /// push notification response
        /// </summary>
        public byte[] Body { get; set; }

        public MessageResponse(Push source, byte[] body = null)
        {
            Source = source;
            Body = body;
        }
    }
}
