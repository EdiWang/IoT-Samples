using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Edi.RPi.BMP180WebServer.Core;

namespace Edi.RPi.BMP180WebServer.WebServer
{
    public sealed class HttpServer : IDisposable
    {
        private const uint BufferSize = 8192;

        private readonly StreamSocketListener listener;

        public int Port { get; }

        public delegate void HttpRequestReceivedEvent(HTTPRequest request);
        public event HttpRequestReceivedEvent OnRequestReceived;

        public HttpServer(int serverPort)
        {
            if (listener == null)
            {
                listener = new StreamSocketListener();
            }
            Port = serverPort;

            listener.ConnectionReceived += (s, e) => ProcessRequestAsync(e.Socket);
        }

        public void Dispose()
        {
            listener?.Dispose();
        }

        internal async void StartServer()
        {
            //#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            try
            {
                await listener.BindServiceNameAsync(Port.ToString());
                System.Diagnostics.Debug.WriteLine("Http Server binded to service {0}.", Port);
            }
            catch (Exception ex)
            {
                SocketErrorStatus status = SocketError.GetStatus(ex.HResult);
                if (status == SocketErrorStatus.AddressAlreadyInUse)
                {
                    await Task.Delay(1000 * 30);
                    StartServer();
                }
                System.Diagnostics.Debug.WriteLine("Http Server could not bind to service {0}. Error {1}", Port, ex.Message);
            }
            //#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        private async void ProcessRequestAsync(StreamSocket socket)
        {
            HTTPRequest request;
            using (IInputStream stream = socket.InputStream)
            {
                HttpRequestParser parser = new HttpRequestParser();
                request = await parser.GetHttpRequestForStream(stream);
                OnRequestReceived.Invoke(request);
            }

            using (IOutputStream output = socket.OutputStream)
            {
                if (request.Method == "GET")
                {
                    await WriteResponseAync(request.URL, output);
                }
                else
                {
                    Debug.WriteLine("HTTP method not supported: " + request.Method);
                }
            }
        }

        private async Task WriteResponseAync(string request, IOutputStream output)
        {
            string temperatureText = String.Empty;
            string pressureText = String.Empty;

            try
            {
                await Bmp180Reader.Instance.RefreshReadingsAsync();
                temperatureText += Bmp180Reader.Instance.Temperature;
                pressureText += Bmp180Reader.Instance.Pressure;
            }
            catch (Exception ex)
            {
                temperatureText = "Sensor Error: " + ex.Message;
                pressureText = "Sensor Error: " + ex.Message;
            }

            using (Stream resp = output.AsStreamForWrite())
            {
                byte[] bodyArray = Encoding.UTF8.GetBytes("<html>" +
                                                          "<head>" +
                                                          "    <title>I2C BMP180 Digital Barometric Pressure Sensor Web Server</title>" +
                                                          "</head>" +
                                                          "<body>" +
                                                          "    <h1>I2C BMP180 Digital Barometric Pressure Sensor Web Server</h1>" +
                                                         $"    <p><strong>Temperature:</strong> {temperatureText}</p>" +
                                                         $"    <p><strong>Pressure:   </strong> {pressureText}</p>" +
                                                          "</body>" +
                                                          "</html>");
                MemoryStream stream = new MemoryStream(bodyArray);
                string header = string.Format("HTTP/1.1 200 OK\r\n" +
                                                "Content-Length: {0}\r\n" +
                                                "Connection: close\r\n\r\n",
                                                stream.Length);
                byte[] headerArray = Encoding.UTF8.GetBytes(header);
                await resp.WriteAsync(headerArray, 0, headerArray.Length);
                await stream.CopyToAsync(resp);
                await resp.FlushAsync();
            }
        }
    }
}
