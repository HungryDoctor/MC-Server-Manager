using System;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.OS.Utils.Linux
{
    [SupportedOSPlatform("linux")]
    internal static class LinuxProcessUtils
    {
        public static async Task<ProcessParameters> GetProcessParameters(int pid, CancellationToken ct = default)
        {
            string cmdlinePath = $"/proc/{pid}/cmdline";

            byte[] cmdlineBytes;
            try
            {
                cmdlineBytes = await File.ReadAllBytesAsync(cmdlinePath, ct).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Unable to read {cmdlinePath}", ex);
            }

            if (cmdlineBytes.Length == 0)
            {
                throw new InvalidOperationException($"{cmdlinePath} is empty");
            }

            string allArgs = Encoding.UTF8.GetString(cmdlineBytes);
            string[] parts = allArgs.Split('\0', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                throw new InvalidOperationException($"{cmdlinePath} contained no non‐zero bytes");
            }

            string executable = parts[0];
            string arguments = parts.Length > 1 ? string.Join(" ", parts.Skip(1)).Trim() : string.Empty;

            return new ProcessParameters(executable, arguments);
        }

        public static async Task<int> GetExitCodeAsync(int pid, CancellationToken ct = default)
        {
            string statPath = $"/proc/{pid}/stat";
            if (!File.Exists(statPath))
            {
                throw new FileNotFoundException(statPath);
            }

            string text = await File.ReadAllTextAsync(statPath, ct).ConfigureAwait(false);
            string[] parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 13 && int.TryParse(parts[13], out int statusField))
            {
                return (statusField >> 8) & 0xFF;
            }

            throw new InvalidOperationException($"Can't parse {statPath}");
        }
    }
}
