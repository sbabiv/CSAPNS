namespace CSAPNS.Apns.Channel
{
    public abstract class ChannelPushBase
    {
        /// <summary>
        /// source push reference
        /// </summary>
        public Push Instance { get; }

        protected ChannelPushBase(Push push)
        {
            Instance = push;
        }

        /// <summary>
        /// get data
        /// </summary>
        /// <returns></returns>
        public abstract byte[] ToData();
    }
}
