using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace AsyncDemos
{
    public class _AsyncServer
    {
        public static void Main()
        {
            const int PORT = 2012;

            TcpListener server = new TcpListener(IPAddress.Any, PORT);
            server.Start();

            Console.WriteLine("Server waiting on port {0}", PORT);

            for (; ; )
            {
                TcpClient connection = server.AcceptTcpClient();
                NetworkStream stream = connection.GetStream();

                Console.WriteLine("Connection with {0}: WAITING", connection.Client.RemoteEndPoint);

                byte[] data = new byte[1024];
                
                
                stream.BeginRead(data, 0, 1024, (iar) =>
                {
                    try
                    {
                        int len = stream.EndRead(iar);
                        Console.WriteLine("Connection with {0}: SUCCEEDED ({1} bytes received)", connection.Client.RemoteEndPoint, len);
                        Thread.Sleep(3000);
                    }
                    catch
                    {
                        Console.WriteLine("Connection with {0}: CANCELED", connection.Client.RemoteEndPoint);
                    }
                    Console.WriteLine("Connection with {0}: CLOSED", connection.Client.RemoteEndPoint);
                    stream.Close();
                    connection.Close();
                }, null);
            }
        }
    }
}
