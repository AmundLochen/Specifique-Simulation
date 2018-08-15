using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Sockets;

namespace ClientLibrary
{

    public class Client
    {

        public static void Main()
        {

            System.Net.Sockets.TcpClient clientSocket = new System.Net.Sockets.TcpClient();
            NetworkStream serverStream;

            Console.WriteLine("Client Started");
            clientSocket.Connect("192.168.1.17", 8888);
            Console.WriteLine("Client Socket Program - Server Connected ...");

            while (true)
            {
                serverStream = clientSocket.GetStream();
                System.IO.BinaryWriter writer = new BinaryWriter(serverStream);

                string readInput = Console.ReadLine();
                writer.Write(readInput);
                writer.Flush();


                System.IO.BinaryReader reader = new BinaryReader(clientSocket.GetStream());
                string message = reader.ReadString();

                Console.WriteLine("Data from Server : " + message);
            }
        }

    }
}
