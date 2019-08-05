using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using Newtonsoft.Json;


//https://codehosting.net/blog/BlogEngine/post/Simple-C-Web-Server
namespace ACE.Server.Command.SimpleWebServer
{
    [JsonObject]
    public class CmdRequest
    {
        [JsonProperty]
        public string commandLine { get; set; }

        public Guid RequestId { get; set; }
    }

    [JsonObject]
    public class CmdResponse
    {
        [JsonProperty]
        public Guid RequestId { get; set; }

        [JsonProperty]
        public string commandLine { get; set; }

        [JsonProperty]
        public string command { get; set; }

        [JsonProperty]
        public string[] param { get; set; }

        [JsonProperty]
        public string result { get; set; }

        [JsonProperty]
        public string error { get; set; }
    }

    public class WebService
    {
        public static string Host { get; set; }
        public static int Port { get; set; }
        static Thread _workerThread;

        public static void Start()
        {
            WebServer ws = new WebServer(SendResponse, $"http://{Host}:{Port}/command/");
            _workerThread = new Thread(ws.Run);
            _workerThread.Start();
            /*ws.Run();
            Console.WriteLine("A simple webserver. Press a key to quit.");
            Console.ReadKey();
            ws.Stop();*/
        }

        public static void Stop()
        {
            _workerThread.Join();
        }

        private static string readRequestBody(HttpListenerRequest request)
        {
            if (!request.HasEntityBody)
            {
                return null;
            }
            using (System.IO.Stream body = request.InputStream) // here we have data
            {
                using (System.IO.StreamReader reader = new System.IO.StreamReader(body, request.ContentEncoding))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        public static string SendResponse(HttpListenerRequest httpRequest)
        {
            if (httpRequest.HttpMethod == "GET")
            {
                return "{\"status\": \"ready\"}";
            }
            else
            {
                //Guid RequestId = Guid.NewGuid();
                string body = readRequestBody(httpRequest);
                CmdRequest request = JsonConvert.DeserializeObject<CmdRequest>(body);
                request.RequestId = Guid.NewGuid();
                CommandManager.History.OnRequest(request);

                CmdResponse response;
                RunCommand(request, out response);

                response = CommandManager.History.GetResponse(request);
                return JsonConvert.SerializeObject(response);
            }
        }
        private static void RunCommand(CmdRequest request, out CmdResponse response)
        {
            response = new CmdResponse();
            string commandLine = request.commandLine;
            CommandManager.History.OnCommand(commandLine);

            string command = null;
            string[] parameters = null;
            try
            {
                CommandManager.ParseCommand(commandLine, out command, out parameters);
                response.commandLine = commandLine;
                response.command = command;
                response.param = parameters;
                response.result = null;
                response.error = null;
                CommandManager.History.OnParse(command, parameters);
            }
            catch (Exception ex)
            {
                //log.Error($"Exception while parsing command: {commandLine}", ex);
                response.error = $"Exception while parsing command: {commandLine}";
                CommandManager.History.OnException($"Exception while parsing command: {commandLine}", ex);
                return;
            }
            try
            {
                if (CommandManager.GetCommandHandler(null, command, parameters, out var commandHandler) == CommandHandlerResponse.Ok)
                {
                    try
                    {
                        if (commandHandler.Attribute.IncludeRaw)
                        {
                            parameters = CommandManager.StuffRawIntoParameters(commandLine, command, parameters);
                        }
                        // Add command to world manager's main thread...
                        ((CommandHandler)commandHandler.Handler).Invoke(null, parameters);
                        CommandManager.History.OnSuccess();
                    }
                    catch (Exception ex)
                    {
                        response.error = $"Exception while invoking command handler for: {commandLine}";
                        CommandManager.History.OnException($"Exception while invoking command handler for: {commandLine}", ex);
                        //log.Error($"Exception while invoking command handler for: {commandLine}", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                response.error = $"Exception while getting command handler for: {commandLine}";
                CommandManager.History.OnException($"Exception while getting command handler for: {commandLine}", ex);
                //log.Error($"Exception while getting command handler for: {commandLine}", ex);
            }
        }

    public class WebServer
    {
        private readonly HttpListener _listener = new HttpListener();
        private readonly Func<HttpListenerRequest, string> _responderMethod;

        public WebServer(string[] prefixes, Func<HttpListenerRequest, string> method)
        {
            if (!HttpListener.IsSupported)
                throw new NotSupportedException(
                    "Needs Windows XP SP2, Server 2003 or later.");

            // URI prefixes are required, for example 
            // "http://localhost:8080/index/".
            if (prefixes == null || prefixes.Length == 0)
                throw new ArgumentException("prefixes");

            // A responder method is required
            if (method == null)
                throw new ArgumentException("method");

            foreach (string s in prefixes)
                _listener.Prefixes.Add(s);

            _responderMethod = method;
            _listener.Start();
        }

        public WebServer(Func<HttpListenerRequest, string> method, params string[] prefixes)
            : this(prefixes, method) { }

        public void Run()
        {
            ThreadPool.QueueUserWorkItem((o) =>
            {
                Console.WriteLine("Webserver running...");
                try
                {
                    while (_listener.IsListening)
                    {
                        ThreadPool.QueueUserWorkItem((c) =>
                        {
                            var ctx = c as HttpListenerContext;
                            try
                            {
                                string rstr = _responderMethod(ctx.Request);
                                byte[] buf = Encoding.UTF8.GetBytes(rstr);
                                ctx.Response.ContentLength64 = buf.Length;
                                ctx.Response.OutputStream.Write(buf, 0, buf.Length);
                            }
                            catch { } // suppress any exceptions
                            finally
                            {
                                // always close the stream
                                ctx.Response.OutputStream.Close();
                            }
                        }, _listener.GetContext());
                    }
                }
                catch { } // suppress any exceptions
            });
        }

        public void Stop()
        {
            _listener.Stop();
            _listener.Close();
        }
    }

    

    }
}