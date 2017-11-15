using System.Collections.Generic;

namespace CSAPNS.Apns
{
    public class Push
    {
        /// <summary>
        /// Device token
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Additional data
        /// </summary>
        public Dictionary<string, object> Attributes;

        private readonly string _payload;
        
        public Push(string token, string payload, Dictionary<string, object> attributes = null)
        {
            _payload = payload;

            Token = token;
            Attributes = attributes;
        }

        public override string ToString()
        {
            return _payload.Trim();
        }
    }
}
