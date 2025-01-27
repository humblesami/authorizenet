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
        private readonly Logger _logger = new Logger();
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
                _logger.log("Web server started successfully.");
                Task.Run(() => HandleRequests());
            }
            catch (Exception ex)
            {
                _logger.log("Error starting web server", ex);
            }
        }

        public void StopServer()
        {
            try
            {
                _listener.Stop();
                _listener.Close();
                _logger.log("Web server stopped successfully.");
            }
            catch (Exception ex)
            {
                _logger.log("Error stopping web server", ex);
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
                    _logger.log("Error handling request", ex);
                }
            }
        }

        private async Task ProcessRequestAsync(HttpListenerContext context)
        {
            try
            {
                if(!context.Request.RawUrl.StartsWith("/?pos_"))
                {
                    return;
                }
                string requestId = Guid.NewGuid().ToString(); // Generate a unique ID for the request
                _logger.log($"Processing request: {requestId}, Method: {context.Request.HttpMethod}, URL: {context.Request.Url}");

                if (_processedRequests.Contains(requestId))
                {
                    _logger.log($"Duplicate request detected: {requestId}");
                    return; // Ignore duplicate requests
                }

                _processedRequests.Add(requestId);

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
                InputReceived?.Invoke(jsonInput);

                // Write response
                context.Response.ContentType = "text/plain";
                context.Response.ContentEncoding = Encoding.UTF8;

                using (var writer = new StreamWriter(context.Response.OutputStream))
                {
                    await writer.WriteLineAsync("done");
                }
            }
            catch (Exception ex)
            {
                _logger.log("Error processing request", ex);
            }
            finally
            {
                context.Response.Close();
            }
        }
    }
}
