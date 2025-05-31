using System;
using System.Threading.Tasks;

namespace ConsoleApp
{
    public static class Program
    {
        static async Task Main(string[] args)
        {
            await Console.Out.WriteLineAsync($"Args: {args}").ConfigureAwait(false);
            await Console.Out.WriteLineAsync("Dummy logline").ConfigureAwait(false);
            await Console.Out.WriteAsync("Enter something: ").ConfigureAwait(false);

            if (args?.Length == 1 && string.Equals(args[0], "-explode"))
            {
                throw new Exception("Boom");
            }

            string? readLine = Console.ReadLine();
            await Console.Out.WriteLineAsync($"You have entered '{readLine}'").ConfigureAwait(false);

            await Console.Out.WriteAsync($"Press any key to exit").ConfigureAwait(false);
            Console.Read();
        }
    }
}
