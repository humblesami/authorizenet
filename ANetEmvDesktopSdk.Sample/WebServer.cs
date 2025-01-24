using System;
using System.IO;
using System.Net;
using System.Text;
using System.Windows;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;


namespace ANetEmvDesktopSdk.Sample
{
    public partial class WebServer
    {
        private HttpListener _listener;
        public delegate void InputReceivedEventHandler(Dictionary<string, string> input);
        public event InputReceivedEventHandler InputReceived;

        public WebServer()
        {
            // Configure the listener
            _listener = new HttpListener();
            _listener.Prefixes.Add("http://localhost:8060/");

            // Start listening for requests asynchronously
            _listener.Start();
            Task.Run(() => HandleRequests());
        }

        public void StopServer()
        {
            try
            {
                this._listener.Stop();
                this._listener.Close();
            }
            catch
            {
                MessageBox.Show("Failed to stop/close");
            }            
        }

        private async Task HandleRequests()
        {
            while (true)
            {
                // Wait for a request
                var context = await _listener.GetContextAsync();

                // Handle the request asynchronously
                Task.Run(() => ProcessRequest(context));
            }
        }

        private async Task<Task> ProcessRequest(HttpListenerContext context)
        {
            try
            {
                DateTime dt1 = DateTime.Now;
                // Get the request and response objects
                var request = context.Request;                
                string queryString = context.Request.Url.Query;
                var queryParams = System.Web.HttpUtility.ParseQueryString(queryString);
                Dictionary<string, string> json_input = new Dictionary<string, string>();
                foreach (string key in queryParams.AllKeys)
                {
                    string value = queryParams[key];
                    json_input[key] = value;
                }

                var response = context.Response;
                response.ContentType = "text/plain";
                response.ContentEncoding = Encoding.UTF8;
                var delay = Convert.ToDouble(json_input["delay"]);
                var delay_int = Convert.ToInt32((double)delay);
                await Task.Delay(TimeSpan.FromSeconds(delay));
                this.InputReceived?.Invoke(json_input);
                using (var writer = new StreamWriter(response.OutputStream))
                {
                    writer.WriteLine("done");
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., log, display error)
                Console.WriteLine($"Error processing request: {ex.Message}");
            }
            finally
            {
                // Close the response
                context.Response.Close();
            }
            return Task.CompletedTask;
        }

        public void SendResponseToRestApi(Dictionary<string, string> data)
        {
            using (var client = new HttpClient())
            {
                string apiUrl = "http://localhost:8017/authorize-net/response?status=" + data["status"]+"&order_id="+data["order_id"];
                try
                {
                    client.GetAsync(apiUrl);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex.Message}");
                }
            }
        }

        private void LogErrors()
        {

        }
    }
}