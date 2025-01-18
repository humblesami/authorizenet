using System;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Net.Http;
using System.Collections.Generic;
using Newtonsoft.Json;


namespace ANetEmvDesktopSdk.Sample
{
    public class AuthorizeTcpListner
    {

        private TcpListener tcpServer;
        private bool isRunning = true;
        public delegate void InputReceivedEventHandler(string input);
        public event InputReceivedEventHandler InputReceived;

        public void StopServer()
        {
            this.isRunning = false;
            this.tcpServer?.Stop();
        }

        public void RunTcpTask()
        {
            var self = this;
            Task.Run(() =>
            {
                try
                {
                    int port = 5000;
                    self.tcpServer = new TcpListener(IPAddress.Any, port);
                    self.tcpServer.Start();

                    while (self.isRunning)
                    {
                        if (self.tcpServer.Pending())
                        {
                            self.AcceptTcpRequest();
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (self.isRunning) // Avoid showing error if the server was intentionally stopped
                    {
                        Console.WriteLine($"Server error: {ex.Message}");
                    }
                }
            });
        }

        public void OnTcpInput(Dictionary<string, string> json_input)
        {
            string amt = json_input["amount"];
            InputReceived?.Invoke(amt);
        }

        private void AcceptTcpRequest()
        {
            var client = this.tcpServer.AcceptTcpClient();
            try
            {
                using (NetworkStream stream = client.GetStream())
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
                {
                    string input = reader.ReadLine();
                    Dictionary<string, string> json_input = JsonConvert.DeserializeObject<Dictionary<string, string>>(input);
                    this.OnTcpInput(json_input);
                }
            }
            catch (Exception ex)
            {
                if (this.isRunning) // Ignore client errors if the server is shutting down
                {
                    Console.WriteLine($"Client error: {ex.Message}");
                }
            }
            finally
            {
                client.Close();
            }
        }

        public void SendResponseToRestApi()
        {
            using (var client = new HttpClient())
            {
                string apiUrl = "http://localhost:8014/authorize-net/response";
                try
                {
                    var responseGet = client.GetAsync(apiUrl);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex.Message}");
                }                
                //string postData = "{'key1': 'value1', 'key2': 'value2'}"; // Replace with your actual POST data
                //var content = new StringContent(postData, System.Text.Encoding.UTF8, "application/json");
                //client.PostAsync(apiUrl, content);
            }
        }
    }
}