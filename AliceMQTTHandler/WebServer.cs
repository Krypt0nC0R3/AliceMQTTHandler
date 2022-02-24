using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace AliceMQTTHandler
{
    public class WebServer
    {
        private string prefix = "";
        private bool enabled = false;
        private Thread th_server = null;
        private int port = 60;
        private int timeout = 8;
        private Encoding charEncoder = Encoding.UTF8;
        private Socket serverSocket;

        public delegate string eClientAccepted(string Method, string url, Dictionary<string, string> param);
        public event eClientAccepted OnClientAccepted;

        public WebServer(int port, string prefix, bool autostart = false)
        {
            this.prefix = prefix;
            this.port = port;
            th_server = new(AcceptConnections);
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(new IPEndPoint(IPAddress.Parse("0.0.0.0"), port));
            serverSocket.ReceiveTimeout = timeout;
            serverSocket.SendTimeout = timeout;
            if (autostart) Start();
        }

        private void AcceptConnections()
        {
            while (enabled)
            {
                Socket clientSocket;
                try
                {
                    clientSocket = serverSocket.Accept();
                    // Создаем новый поток для нового клиента и продолжаем слушать сокет.
                    Thread requestHandler = new Thread(() =>
                    {
                        clientSocket.ReceiveTimeout = timeout;
                        clientSocket.SendTimeout = timeout;
                        handleTheRequest(clientSocket);
                    });
                    requestHandler.Start();
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                    //enabled = false;
                }
            }
        }

        public void Start()
        {
            if (enabled) return;
            enabled = true;
            serverSocket.Listen(10);
            th_server.Start();
        }

        private void handleTheRequest(Socket clientSocket)
        {
            try
            {
                byte[] buffer = new byte[10240]; // 10 kb, just in case
                int receivedBCount = clientSocket.Receive(buffer); // Получаем запрос
                string strReceived = charEncoder.GetString(buffer, 0, receivedBCount);

                // Парсим запрос
                string httpMethod = strReceived.Substring(0, strReceived.IndexOf(" "));

                int start = strReceived.IndexOf(httpMethod) + httpMethod.Length + 1;
                int length = strReceived.LastIndexOf("HTTP") - start - 1;
                string requestedUrl = strReceived.Substring(start, length);

                string url = "";
                Dictionary<string, string> param = new();

                if (httpMethod.Equals("GET") || httpMethod.Equals("POST"))
                {
                    url = requestedUrl.Split('?')[0];
                    if (requestedUrl.Contains('?'))
                    {
                        try
                        {
                            string[] prms = requestedUrl.Split('?')[1].Split('=');
                            for (int i = 0; i < prms.Length; i += 2)
                            {
                                try
                                {
                                    param.Add(prms[i], prms[i + 1]);
                                }
                                catch { }
                            }
                            
                        }
                        catch (Exception e)
                        {
                            Logger.Error(e);
                        }
                    }
                    string answer = OnClientAccepted?.Invoke(httpMethod, url, param) ?? "{}";
                    
                    sendResponse(clientSocket, answer, "200 OK", "application/json");
                }
                else
                {
                    notImplemented(clientSocket);
                    return;
                }
            }
            catch { }
            /*
            requestedFile = requestedFile.Replace("/", "\\").Replace("\\..", ""); // Not to go back
            start = requestedFile.LastIndexOf('.') + 1;
            if (start > 0)
            {
                length = requestedFile.Length - start;
                string extension = requestedFile.Substring(start, length);
                if (File.Exists(root + requestedFile)) // Если да
                                                       // ТО отсылаем запрашиваемы контент:
                    sendOkResponse(clientSocket, File.ReadAllBytes(root + requestedFile), "text/html");
                else
                    notFound(clientSocket); // Мы не поддерживаем данный контент.
            }
            else
            {
                // Если файл не указан, пробуем послать index.html
                // Вы можете добавить больше(например "default.html")
                if (requestedFile.Substring(length - 1, 1) != "\\")
                    requestedFile += "\\";
                if (File.Exists(root + requestedFile + "index.htm"))
                    sendOkResponse(clientSocket, File.ReadAllBytes(root + requestedFile + "\\index.htm"), "text/html");
                else if (File.Exists(root + requestedFile + "index.html"))
                    sendOkResponse(clientSocket, File.ReadAllBytes(root + requestedFile + "\\index.html"), "text/html");
                else
                    notFound(clientSocket);
            }
            */
        }
        private void notImplemented(Socket clientSocket)
        {

            sendResponse(clientSocket, "<html><head><meta http - equiv =\"Content-Type\" content=\"text/html; charset = utf - 8\"></head><body>501 - Method Not Implemented</body></html> ", "501 Not Implemented", "text/html");

        }

        private void notFound(Socket clientSocket)
        {

            sendResponse(clientSocket, "<html><head><metahttp - equiv =\"Content-Type\" content=\"text/html; charset = utf - 8\"></head><body>404 - Not Found</body></html>", "404 Not Found", "text/html");
        }

        private void sendOkResponse(Socket clientSocket, byte[] bContent, string contentType)
        {
            sendResponse(clientSocket, bContent, "200 OK", contentType);
        }

        private void sendResponse(Socket clientSocket, string strContent, string responseCode, string contentType)
        {
            byte[] bContent = charEncoder.GetBytes(strContent);
            sendResponse(clientSocket, bContent, responseCode, contentType);
        }

        private void sendResponse(Socket clientSocket, byte[] bContent, string responseCode, string contentType)
        {
            try
            {
                byte[] bHeader = charEncoder.GetBytes(
                                    "HTTP/1.1 " + responseCode + "\r\n"
                                  + "Server: AliceMQTTHandler" + "\r\n"
                                  + "Content-Length: " + bContent.Length.ToString() + "\r\n"
                                  + "Connection: close\r\n"
                                  + "Content-Type: " + contentType + "\r\n\r\n");
                clientSocket.Send(bHeader);
                clientSocket.Send(bContent);
                clientSocket.Close();
            }
            catch { }
        }
    }
}
