using System;
using System.Threading.Tasks;
using System.Reactive.Linq;
using VisionLib;
using Husty.IO;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {

            var errors = new Errors(double.NaN, double.NaN);
            var composer = new VisualComposer(true, true);
            Task.Run(async () =>
            {
                Console.WriteLine("Wait for client ...");
                var server = new TcpSocketServer(5001);
                var stream = server.GetStream();
                Console.WriteLine("Connected.");
                while (true)
                {
                    if (stream.ReadString() is "") break;
                    errors = await composer.GetCurrentErrors();
                    var lateral = errors.LateralError / 1000.0;
                    stream.WriteString($"{-lateral},{-errors.HeadingError}");
                    Console.WriteLine($"LateralE = {-lateral:f2}  : HeadingE = {-errors.HeadingError:f2}");
                }
            });

            while (Console.ReadKey().Key is not ConsoleKey.Enter) ;
            composer.Dispose();

            Console.WriteLine("Press key to exit ...");
            Console.ReadKey();

        }
    }
}
