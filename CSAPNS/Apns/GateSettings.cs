using System.Security.Cryptography.X509Certificates;

namespace CSAPNS.Apns
{
    public class GateSettings
    {
        public X509Certificate2 Certificate { get; }

        public readonly string Host = "gateway.push.apple.com";
        public readonly int Port = 2195;
        public readonly int WorkerPoolSize = 1;

        public GateSettings(X509Certificate2 cert, int workerPoolSize = 1)
        {
            Certificate = cert;
            WorkerPoolSize = workerPoolSize;
        }

        public GateSettings(X509Certificate2 cert, string host, int port, int workerPoolSize)
        {
            Certificate = cert;
            Host = host;
            Port = port;
            WorkerPoolSize = workerPoolSize;
        }
    }
}
