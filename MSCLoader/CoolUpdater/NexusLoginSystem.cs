using System;
using System.Diagnostics;
using WebSocketSharp;

namespace CoolUpdater
{
    class NexusLoginSystem
    {
        WebSocket webSocket;

        const string Remote = "wss://sso.nexusmods.com";

        string uuid, token, apikey;
        bool closed;

        public NexusLoginSystem(string token = "")
        {
            if (!string.IsNullOrEmpty(token))
            {
                this.token = token;
            }
            else
            {
                this.token = null;
            }
            uuid = Guid.NewGuid().ToString();

            webSocket = new WebSocket(Remote);
            webSocket.SslConfiguration.EnabledSslProtocols = (System.Security.Authentication.SslProtocols)(192 | 768 | 3072);
            webSocket.OnClose += (sender, e) => Close();
            webSocket.OnError += (sender, e) => { Console.Write("ERROR : " + e.ToString()); Close(); };
            webSocket.OnMessage += WebSocket_OnMessage;
            
            webSocket.OnOpen += (sender, e) =>
            {
                webSocket.Send(CreateData(this.uuid, this.token));
                try
                {
                    Process.Start(GetBase(this.uuid));
                }
                catch
                {
                    Process.Start("IEXPLORE", GetBase(this.uuid));
                }
            };
            webSocket.Connect();
            Console.ReadKey();
        }

        private void WebSocket_OnMessage(object sender, MessageEventArgs e)
        {
            string[] output = e.Data.ToString().Replace(",\"", ",\n\"").Replace(":{", ":\n{\n").Replace("},", "\n},").Replace(":[{", ":[{\n").Replace("}],", "\n}],").Split('\n');
            foreach (string s in output)
            {
                if (s.Contains("connection_token"))
                {
                    token = s.Split(':')[1].Replace("\"", "").Replace(",", "").Trim();
                }
                else if (s.Contains("api_key"))
                {
                    apikey = s.Split(':')[1].Replace("\"", "").Replace(",", "").Trim();
                    Close();
                    Console.WriteLine("apikey: " + apikey + "\ntoken: " + token);
                    Environment.Exit(0);
                }
            }
        }

        void Close()
        {
            if (closed) return;
            closed = true;
            webSocket.Close();
        }

        string GetBase(string uuid)
        {
            return "https://www.nexusmods.com/sso?id=" + uuid + "&application=mscmodloaderpro";
        }
        
        string CreateData(string uuid, string token)
        {
            return $"{{\n  \"id\": \"{uuid}\",\n  \"token\": {(string.IsNullOrEmpty(token) ? "null" : $"\"{token}\"")},\n  \"protocol\": 2\n}}";
        }
    }
}
