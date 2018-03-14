using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading;

namespace Monaco.Wpf
{

    public class EmbeddedHttpServer
    {



        static EmbeddedHttpServer _singelton;
        public static void EnsureStarted()
        {
            if (_singelton == null)
            {

                _singelton = new EmbeddedHttpServer();


            }
        }
        public static void AddHandler(IRequestHandler handler)
        {
            _singelton._handlers.Add(handler);
        }
        public static string EditorUri => $"http://localhost:{_singelton._port.ToString()}/editor.html";
        public static string DiffUri => $"http://localhost:{_singelton._port.ToString()}/diff.html";


        private Thread _serverThread;
        private HttpListener _listener;
        private List<IRequestHandler> _handlers;
        private int _port;

        private EmbeddedHttpServer()
        {
            _handlers = new List<IRequestHandler> { new EmbeddedFilesHandler() };
            Initialize();
        }
        private void Initialize()
        {
            
            _serverThread = new Thread(this.Listen);
            _serverThread.Start();
        }
        public void Stop()
        {
            _serverThread.Abort();
            _listener.Stop();
        }
        private void Listen()
        {

            _port = 52392;
            var isCreated = false;
            while (!isCreated)
            {
                _port++;
                try
                {
                    _listener = new HttpListener();
                    _listener.Prefixes.Add("http://localhost:" + _port.ToString() + "/");
                    _listener.Start();

                    isCreated = true;
                }
                catch (Exception)
                {

                }
            }
            
            while (true)
            {
                try
                {
                    HttpListenerContext context = _listener.GetContext();
                    Process(context);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
        private void Process(HttpListenerContext context)
        {

            bool isHandled = false;
            foreach (var handler in _handlers)
            {
                try
                {
                    isHandled = handler.Handle(context);
                    if (isHandled)
                        break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    isHandled = true;
                    break;
                }
            }

            if (isHandled)
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            }

            context.Response.OutputStream.Close();
        }







    }


}
