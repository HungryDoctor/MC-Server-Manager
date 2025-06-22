using System;
using System.Threading.Tasks;

namespace DummyConsoleApp
{
    public static class Program
    {
        static async Task Main(string[] args)
        {
            await Console.Out.WriteLineAsync($"Args: {string.Join(" ", args)}").ConfigureAwait(false);
            await Console.Out.WriteLineAsync("Dummy logline").ConfigureAwait(false);
            await Console.Out.WriteLineAsync("Enter something").ConfigureAwait(false);

            if (args?.Length == 1 && string.Equals(args[0], "-explode"))
            {
                throw new CustomException("Booom");
            }

            string? line;
            while ((line = Console.ReadLine()) != null)
            {
                await Console.Out.WriteLineAsync("Echo").ConfigureAwait(false);
                await Console.Out.WriteLineAsync(line).ConfigureAwait(false);
                await Console.Out.WriteLineAsync().ConfigureAwait(false);

                if (string.Equals(line, "stop", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }
            }
        }

        private sealed class CustomException : Exception
        {
            public CustomException() : base()
            {
            }

            public CustomException(string? message) : base(message)
            {
            }

            public CustomException(string? message, Exception? innerException) : base(message, innerException)
            {
            }
        }
    }
}
