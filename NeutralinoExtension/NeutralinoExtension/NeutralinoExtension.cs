using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace NeutralinoDotNetExtension
{
    public class NeutralinoExtension
    {
        private const string version = "1.0.0";
        private const bool debugTermColors = true; // Use terminal colors
        private const string debugTermColorIN = "\x1B[32m"; // Green: All incoming events, except function calls
        private const string debugTermColorCALL = "\x1B[33m"; // Yellow: Incoming function calls
        private const string debugTermColorOUT = "\x1B[34m"; // Blue: Outgoing events 
        private const string debugTermColorERROR = "\x1B[91m"; // Red: Errors
        private const string debugTermColorRESET = "\x1B[0m";
        private const bool terminateOnWindowsClose = true;

        private bool debug;
        private string port;
        private string token;
        private string idExtension;
        private string connectToken;
        private string urlSocket;
        private SimpleWebSocket socket;
        
        private Dictionary<string, Action<string>> handlers = new Dictionary<string, Action<string>>();

        public NeutralinoExtension(bool debug = false)
        {
            this.debug = debug;
            string[] args = Environment.GetCommandLineArgs();

            if (args.Length > 1)
            {
                // Getting parameters from command line
                try
                {
                    int portPos = Array.IndexOf(args, "--nl-port");
                    int tokenPos = Array.IndexOf(args, "--nl-token");
                    int extensionIdPos = Array.IndexOf(args, "--nl-extension-id");

                    this.port = args[portPos + 1];
                    this.token = args[tokenPos + 1];
                    this.idExtension = args[extensionIdPos + 1];
                    this.connectToken = "";
                    urlSocket = $"ws://localhost:{port}?extensionId={idExtension}&connectToken={connectToken}";
                
                    DebugLog("---");
                    DebugLog("Received extension config via command line arguments:");
                    DebugLog(string.Join(" ", args));
                    DebugLog("WebSocket URL is:");
                    DebugLog(urlSocket);
                }
                catch (Exception e)
                {
                    DebugLog("Error in arguments", "error");
                    DebugLog(e.Message, "error");
                }

            }
            else
            {
                // Getting parameters from stdin
                string? line = Console.ReadLine();

                try
                {
                    JsonDocument doc = JsonDocument.Parse(line);
                    JsonElement root = doc.RootElement;

                    port = root.GetProperty("nlPort").GetString();
                    token = root.GetProperty("nlToken").GetString();
                    idExtension = root.GetProperty("nlExtensionId").GetString();
                    connectToken = root.GetProperty("nlConnectToken").GetString();
                    urlSocket = $"ws://localhost:{port}?extensionId={idExtension}&connectToken={connectToken}";

                    DebugLog("---");
                    DebugLog("Received extension config via stdin:");
                    DebugLog(line);
                    DebugLog("WebSocket URL is:");
                    DebugLog(urlSocket);
                }
                catch (Exception e)
                {
                    DebugLog("Error in JSON format", "error");
                    DebugLog(e.Message, "error");
                }
            }

            DebugLog($"{this.idExtension} running on port {this.port}");
            DebugLog("---");
        }

        public void AddEvent(string eventName, Action<string> callback)
        {
            handlers[eventName] = callback;
        }

        public void RunForever()
        {
            Run();

            while (true) // forever loop
            {
                Thread.Sleep(1000); 
            }
        }

        public void Run()
        {
            socket = new SimpleWebSocket();
            socket.OnOpen += OnOpen;
            socket.OnMessage += OnMessage;
            socket.OnError += OnError;
            socket.OnClose += OnClose;
            _ = socket.ConnectAsync(urlSocket);
        }

        private void OnClose(object? sender, EventArgs e)
        {
            DebugLog($"WebSocket_Event onClose : StatusCode");
            Environment.Exit(0);
        }

        private void OnError(object? sender, ErrorEventArgs e)
        {
            // When disconnected suddenly from the UI we don't get an OnClose event
            // but an OnError, so we should exit here to avoid orphaned processes 
            // running in the background.
            DebugLog($"WebSocket_Event onError : ErrorMessage {e.Error}", "error");
            Environment.Exit(0);
        }

        private void OnMessage(object? sender, MessageEventArgs e)
        {
            // The JSON we receive should be in this format:
            // {"data":{"function":"prueba","parameter":"parametro"},"event":"runDotNet"}

            DebugLog(e.Message, "in");

            string myEvent = GetEvent(e.Message);

            if (terminateOnWindowsClose) 
            {
                if(myEvent == "windowClose" || myEvent == "appClose")
                {
                    DebugLog("Closing DotNet Extension");
                    Environment.Exit(0);
                }
            }

            // Unlike the original extension, we process the message here
            // and call the registered function

            if(myEvent == "runDotNet")
            {
                JsonDocument doc = JsonDocument.Parse(e.Message);
                if(doc.RootElement.TryGetProperty("data", out JsonElement data) &&
                    data.TryGetProperty("function", out JsonElement fun) &&
                    data.TryGetProperty("parameter", out JsonElement par))
                {
                    string function = fun.ToString();
                    string param = par.ToString();

                    if (handlers.ContainsKey(function))
                    {
                        // Call the Action
                        handlers[function](param);
                    }
                    else
                    {
                        DebugLog($"Function {function} not registered in Events", "error");
                    }                    
                }
                else
                {
                    DebugLog($"JSON message format invalid: {e.Message}", "error");
                }
            }
        }

        private void OnOpen(object? sender, EventArgs e)
        {
            DebugLog("WebSocket_Event onOpen");
        }

        public void DebugLog(string msg, string tag = "info")
        {
            if (this.debug)
            {
                string cIN = "";
                string cCALL = "";
                string cOUT = "";
                string cRST = "";
                string cERR = "";

                if (debugTermColors)
                {
                    cIN = debugTermColorIN;
                    cCALL = debugTermColorCALL;
                    cOUT = debugTermColorOUT;
                    cRST = debugTermColorRESET;
                    cERR = debugTermColorERROR;
                }

                if (tag == "in")
                {
                    if (msg.Contains("runDotNet"))
                    {
                        Console.WriteLine($"{cCALL}IN: {msg}{cRST}");
                    }
                    else
                    {
                        Console.WriteLine($"{cIN}IN: {msg}{cRST}");
                    }
                }
                else if (tag == "out")
                {
                    Console.WriteLine($"{cOUT}OUT: {msg}{cRST}");
                }
                else if (tag == "error")
                {
                    Console.WriteLine($"{cERR}ERROR: {msg}{cRST}");
                }
                else
                {
                    Console.WriteLine(msg);
                }
            }
        }

        public async void SendMessage(string eventName, string dataObject)
        {
            Guid guid = Guid.NewGuid();
            string myguid = guid.ToString();

            string json = $@"{{""id"":""{Guid.NewGuid()}"",""method"":""app.broadcast"",""accessToken"":""{token}"",""data"":{{""event"":""{eventName}"",""data"":""{dataObject}""}}}}";

            DebugLog(json, "out");
            await socket.SendAsync(json);
        }

        private static string GetEvent(string eData)
        {
            JsonDocument doc = JsonDocument.Parse(eData);
            if (doc.RootElement.TryGetProperty("event", out JsonElement elem))
            {
                if (elem.ValueKind == JsonValueKind.String)
                {
                    return elem.GetString();
                }
            }
            return "";
        }
    }

    public class MessageEventArgs : EventArgs
    {
        public string Message { get; }

        public MessageEventArgs(string message)
        {
            Message = message;
        }
    }

    public class ErrorEventArgs : EventArgs
    {
        public string Error { get; }

        public ErrorEventArgs(string error)
        {
            Error = error;
        }
    }

    public class SimpleWebSocket
    {
        private ClientWebSocket socket;
        private CancellationTokenSource cToken;

        public event EventHandler? OnOpen;
        public event EventHandler<MessageEventArgs>? OnMessage;
        public event EventHandler<ErrorEventArgs>? OnError;
        public event EventHandler? OnClose;

        public WebSocketState State
        {
            get
            {
                if (socket?.State == null)
                {
                    return WebSocketState.None;
                }
                else
                {
                    return socket.State;
                }
            }
        }

        public async Task ConnectAsync(string url)
        {
            socket = new ClientWebSocket();
            cToken = new CancellationTokenSource();

            try
            {
                await socket.ConnectAsync(new Uri(url), cToken.Token);
                OnOpen?.Invoke(this, EventArgs.Empty);

                // Suppresses compiler warning: “You didn’t await this task”
                // Otherwise, what we're doing is not using "await" and let the task run in background
                _ = Task.Run(ReceiveLoop);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new ErrorEventArgs(ex.Message));
            }
        }

        private async Task ReceiveLoop()
        {
            var buffer = new byte[4096];

            try
            {
                while (socket.State == WebSocketState.Open)
                {
                    using var ms = new MemoryStream();

                    WebSocketReceiveResult result;

                    do
                    {
                        result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), cToken.Token);

                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            await CloseAsync();
                            OnClose?.Invoke(this, EventArgs.Empty);
                            return;
                        }

                        ms.Write(buffer, 0, result.Count);

                    } while (!result.EndOfMessage);

                    string message = Encoding.UTF8.GetString(ms.ToArray());
                    OnMessage?.Invoke(this, new MessageEventArgs(message));
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new ErrorEventArgs(ex.Message));
            }
        }

        public async Task SendAsync(string message)
        {
            if (socket.State != WebSocketState.Open)
                return;

            byte[] bytes = Encoding.UTF8.GetBytes(message);

            await socket.SendAsync(
                new ArraySegment<byte>(bytes),
                WebSocketMessageType.Text,
                true,
                cToken.Token
            );
        }

        public async Task CloseAsync()
        {
            try
            {
                if (socket.State == WebSocketState.Open)
                {
                    await socket.CloseAsync(
                        WebSocketCloseStatus.NormalClosure,
                        "Closing",
                        cToken.Token
                    );
                }
            }
            catch { }

            cToken.Cancel();
        }
    }
}
