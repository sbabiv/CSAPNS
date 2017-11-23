using System;

namespace CSAPNS.Apns.Response
{
    public class PushResponse
    {
        public Push Instance { get; private set; }
        public DateTime CrateDate { get; private set; }

        public ResponseCodes Status { get; private set; }
        public string Body { get; private set; }

        public PushResponse(Push instance, ResponseCodes status, string body, DateTime createDate)
        {
            Instance = instance;
            Status = status;
            Body = body;
            CrateDate = createDate;
        }
    }
}
