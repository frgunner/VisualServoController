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
            Console.WriteLine("Wait for client ...");
            var server = new TcpSocketServer(5001);
            var stream = server.GetStream();
            Console.WriteLine("Connected.");
            Task.Run(async () =>
            {
                while (true)
                {
                    if (stream.ReadString() is "") break;
                    errors = await composer.GetCurrentErrors();
                    stream.WriteString($"{errors.LateralError},{errors.HeadingError}");
                    Console.WriteLine($"LateralE = {errors.LateralError:f2}  : HeadingE = {errors.HeadingError:f2}");
                }
            });

            while (Console.ReadKey().Key is not ConsoleKey.Enter) ;
            composer.Dispose();

            Console.WriteLine("Press key to exit ...");
            Console.ReadKey();

        }
    }
}
