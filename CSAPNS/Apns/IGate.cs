namespace CSAPNS.Apns
{
    public delegate void ProcessedHandler(PushResponse response);

    internal interface IGate
    {
        /// <summary>
        /// push notification proccessed event
        /// </summary>
        event ProcessedHandler OnProcessed;

        /// <summary>
        /// enqueue push 
        /// </summary>
        /// <param name="push"></param>
        void Enqueue(Push push);

        /// <summary>
        /// Close all connections
        /// </summary>
        void Close();
    }
}
