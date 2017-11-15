using System;
using System.Net;
using System.Xml.Linq;

namespace CSAPNS.Apns
{
    public class ApnsResponseParser : IResponseParser
    {
        public PushResponse Parse(ResponseWrapper responseWrapper)
        {
            if (responseWrapper.TransportDataResponse.TransportCode == HttpStatusCode.OK)
            {
                var resp = GetMessageResp(responseWrapper.MessageDataResp);
                return new PushResponse(responseWrapper.MessageDataResp.Source, resp.Item2, resp.Item1, responseWrapper.CreationDate);
            }

            return new PushResponse(responseWrapper.MessageDataResp.Source, ResponseCodes.TransportError,
                GetTransportResp(responseWrapper.MessageDataResp), responseWrapper.CreationDate);
        }

        private Tuple<string, ResponseCodes> GetMessageResp(MessageResponse message)
        {
            var status = ResponseCodes.NoErrorsEncountered;
            var resp = new XElement("response",
                new XAttribute("token", message.Source.Token),
                new XAttribute("platform", "apns"));

            if (message.Body != null)
            {
                status = ((ResponseCodes) Convert.ToInt32(message.Body[0]));
                resp.Add(new XElement("msg_result", new XAttribute("error", status.ToString())));
            }

            return new Tuple<string, ResponseCodes>(new XDocument(resp).ToString(), status);
        }

        private string GetTransportResp(MessageResponse message)
        {
            return new XElement("response",
                new XAttribute("token", message.Source.Token),
                new XAttribute("platform", "apns"),
                new XElement("transport_result",
                    new XAttribute("tcp", "network error"))).ToString();
        }
    }
}
