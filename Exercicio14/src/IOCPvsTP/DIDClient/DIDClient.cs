using System;
using System.Net;
using System.Net.Sockets;

namespace AsyncDemos
{
    public class DIDClient
    {
        public static void Main(String[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("use: DIDClient HOST PORT NUM");
                Environment.Exit(1);
            }

            string HOST = args[0];
            int PORT = Int32.Parse(args[1]);
            int NUM = Int32.Parse(args[2]);

            Console.WriteLine("Creating {0} connections to {1}:{2}", NUM, HOST, PORT);

            TcpClient[] connections = new TcpClient[NUM];
            for (int i = 0; i < NUM; ++i)
            {
                connections[i] = new TcpClient();
                connections[i].Connect(HOST, PORT);
            }

            Console.WriteLine("Press ENTER to send data on all connections");

            Console.ReadLine();

            Console.WriteLine("Sending data on all connections");

            byte[] data = new byte[4];
            for (int i = 0; i < NUM; ++i)
            {
                NetworkStream stream = connections[i].GetStream();
                stream.Write(data, 0, 4);
                stream.Flush();
            }

            Console.WriteLine("Eliminating all connections");

            for (int i = 0; i < NUM; ++i)
            {
                connections[i].GetStream().Close();
                connections[i].Close();
                connections[i] = null;
            }
        }
    }
}
