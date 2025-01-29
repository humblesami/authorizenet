using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections;

namespace ANetEmvDesktopSdk.Sample
{
    public partial class WebServer
    {
        private readonly HttpListener _listener;
        private readonly Logger logger = new Logger();
        private readonly HashSet<string> _processedRequests = new HashSet<string>(); // To track processed requests

        public event Action<Dictionary<string, string>> InputReceived;
        public static Dictionary<string, string> NoDeviceOrServerDown = new Dictionary<string, string>();
        public WebServer()
        {
            try
            {
                _listener = new HttpListener();
                _listener.Prefixes.Add("http://localhost:8060/");
                _listener.Start();
                logger.log("Web server started successfully.");
                Task.Run(() => HandleRequests());
            }
            catch (Exception ex)
            {
                logger.log("Error starting web server", ex);
            }
        }

        public async void SendResponseToApi(Dictionary<string, string> data)
        {
            var queryString = $"&order_id={data["pos_order_id"]}&amount={data["amount"]}&token={data["token"]}&olp={data["olp"]}";
            queryString += $"&status={data["status"]}";
            var apiUrl = $"{data["host_url"]}/authorize-net/callback?{queryString}";
            logger.log(apiUrl);
            using (var client = new HttpClient())
            {
                var result = await client.GetAsync(apiUrl);
                var a = 1;
            }
        }

        public void StopServer()
        {
            try
            {
                _listener.Stop();
                _listener.Close();
                logger.log("Web server stopped successfully.");
            }
            catch (Exception ex)
            {
                logger.log("Error stopping web server", ex);
            }
        }

        private async Task HandleRequests()
        {
            while (_listener.IsListening)
            {
                try
                {
                    var context = await _listener.GetContextAsync();
                    await ProcessRequestAsync(context); // Process the request
                }
                catch (Exception ex)
                {
                    logger.log("Error handling request", ex);
                }
            }
        }

        private async Task ProcessRequestAsync(HttpListenerContext context)
        {
            try
            {
                //context.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
                //context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");
                context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                context.Response.Headers.Add("Access-Control-Allow-Headers", "*");

                if (!context.Request.RawUrl.StartsWith("/?pos_"))
                {
                    using (var writer = new StreamWriter(context.Response.OutputStream))
                        await writer.WriteLineAsync("Invalid url => " + context.Request.RawUrl);
                    return;
                }                
                
                // Parse query parameters
                var queryParams = System.Web.HttpUtility.ParseQueryString(context.Request.Url.Query);
                var jsonInput = new Dictionary<string, string>();

                foreach (string key in queryParams.AllKeys)
                {
                    jsonInput[key] = queryParams[key];
                }
                string actualValue;
                if (!jsonInput.TryGetValue("amount", out actualValue))
                {
                    using (var writer = new StreamWriter(context.Response.OutputStream))
                        await writer.WriteLineAsync("Invalid amount");
                    return;
                }
                if (!jsonInput.TryGetValue("pos_order_id", out actualValue))
                {
                    using (var writer = new StreamWriter(context.Response.OutputStream))
                        await writer.WriteLineAsync("Invalid pos_order_id");
                    return;
                }
                NoDeviceOrServerDown = jsonInput;
                jsonInput["status"] = "ok";
                SendResponseToApi(jsonInput);
                //InputReceived?.Invoke(jsonInput);

                // Write response
                context.Response.ContentType = "text/plain";
                context.Response.ContentEncoding = Encoding.UTF8;

                using (var writer = new StreamWriter(context.Response.OutputStream))
                {
                    await writer.WriteLineAsync("done");
                }
                logger.log("Succefuly processed", null);
            }
            catch (Exception ex)
            {
               logger.log("Error processing request", ex);
            }
            finally
            {
                context.Response.Close();
            }
        }
    }
}
