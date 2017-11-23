namespace CSAPNS.Apns.Response
{
    public interface IResponseParser
    {
        PushResponse Parse(ResponseWrapper responseWrapper);
    }
}
