using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

using CSAPNS.Apns;
using CSAPNS.Apns.Response;

namespace CSAPNS
{
    class Program
    {
        static void Main(string[] args)
        {
            var settings = new GateSettings(new X509Certificate2(@"path\cert.p12", "pwd"), workerPoolSize:1);
            var gate = new ApnsGate(settings, new ApnsResponseParser());
            gate.OnProcessed += OnProcessed;

            var payload = @"{""aps"":{""alert"":{""title"":""Game Request"",""body"":""Bob wants to play poker"",""action-loc-key"":""PLAY""},""badge"":1}}";

            gate.Enqueue(new Push("deviceToken", payload, new Dictionary<string, object>()
            {
                /*custom params*/
                {"ID", Guid.NewGuid()},
                {"Server", "my-test-server-name"},
            }));

            Console.ReadLine();
            gate.Close();
        }

        private static void OnProcessed(PushResponse response)
        {
            var id = (Guid) response.Instance.Attributes["ID"];
            var server = response.Instance.Attributes["Server"];

            var sb = new StringBuilder()
                .AppendLine("response")
                .AppendLine("ID: " + id)
                .AppendLine("Server: " + server)
                .AppendLine("Token " + response.Instance.Token)
                .AppendLine("CreateDate: " + response.CrateDate)
                .AppendLine("Status: " + response.Status)
                .AppendLine("Body: " + response.Body);

            Console.WriteLine(sb.ToString());
        }
    }
}
