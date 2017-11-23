namespace CSAPNS.Apns.Response
{
    /// <summary>
    /// push transport code
    /// </summary>
    public enum ResponseCodes
    {
        /// <summary>
        /// transport error
        /// message not sent
        /// </summary>
        TransportError = -1,

        /*default response message code*/
        NoErrorsEncountered = 0,
        ProcessingError = 1,
        MissingDeviceToken = 2,
        MissingTopic = 3,
        MissingPayload = 4,
        InvalidTokenSize = 5,
        InvalidTopicSize = 6,
        InvalidPayloadSize = 7,
        InvalidToken = 8,
        Shutdown = 10,
        
        Unknown = 255
    }
}
