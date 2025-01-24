using System;
using System.IO;
using System.Net;
using System.Web;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Windows;
using System.Threading;


namespace ANetEmvDesktopSdk.Sample
{    
    public partial class WebServer
    {
        private HttpListener _listener;
        public delegate void InputReceivedEventHandler(string input);
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

        public void OnTcpInput(Dictionary<string, string> json_input)
        {
            string amt = json_input["amount"];
            this.InputReceived?.Invoke(amt);
        }

        private async Task<Task> ProcessRequest(HttpListenerContext context)
        {
            try
            {
                // Get the request and response objects
                var request = context.Request;                
                string queryString = context.Request.Url.Query;
                var queryParams = System.Web.HttpUtility.ParseQueryString(queryString);
                Dictionary<string, string> param_dict = new Dictionary<string, string>();
                foreach (string key in queryParams.AllKeys)
                {
                    string value = queryParams[key];
                    param_dict[key] = value;
                    Console.WriteLine($"Key: {key}, Value: {value}");
                }

                var response = context.Response;
                response.ContentType = "text/plain";
                response.ContentEncoding = Encoding.UTF8;
                var delay = Convert.ToDouble(param_dict["delay"]);
                var delay_int = Convert.ToInt32((double)delay);
                await Task.Delay(TimeSpan.FromSeconds(delay));
                using (var writer = new StreamWriter(response.OutputStream))
                {
                    this.OnTcpInput(param_dict);
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
    }
}