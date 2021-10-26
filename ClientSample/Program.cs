using System;
using System.Threading;
using System.Threading.Tasks;
using Husty.IO;

namespace ClientSample
{
    class Program
    {
        static void Main(string[] args)
        {

            var client = new TcpSocketClient("127.0.0.1", 5001);
            var stream = client.GetStream();

            Task.Run(() =>
            {
                while (true)
                {
                    var sendMsg = "Request";
                    Thread.Sleep(200);
                    stream.WriteString(sendMsg);
                    Console.WriteLine(sendMsg);
                    var rcv = stream.ReadString();
                    Console.WriteLine(rcv);
                }
            });

            Console.WriteLine("Press Enter key to exit.");
            while (Console.ReadKey().Key is not ConsoleKey.Enter) ;

        }
    }
}
