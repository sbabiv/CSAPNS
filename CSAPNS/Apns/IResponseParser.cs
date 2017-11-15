namespace CSAPNS.Apns
{
    public interface IResponseParser
    {
        PushResponse Parse(ResponseWrapper responseWrapper);
    }
}
