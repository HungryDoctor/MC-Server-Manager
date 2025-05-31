using Infrastructure.OS.Utils.Linux;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.OS.Utils
{
    public static class ProcessUtils
    {
        public static async Task<ProcessParameters> GetGetProcessParametersAsync(int pid, CancellationToken ct = default)
        {
            if (OperatingSystem.IsWindows())
            {
                return WindowsProcessUtils.GetProcessParameters(pid);
            }

            if (OperatingSystem.IsLinux())
            {
                return await LinuxProcessUtils.GetProcessParameters(pid, ct).ConfigureAwait(false);
            }

            throw new PlatformNotSupportedException($"Platform '{Environment.OSVersion.Platform}' is not supported");
        }

        public static async Task<int> GetExitCodeAsync(Process process, CancellationToken ct = default)
        {
            int exitCode;
            try
            {
                exitCode = process.ExitCode;
            }
            catch (Exception ex) when (OperatingSystem.IsLinux())
            {
                try
                {
                    exitCode = await LinuxProcessUtils.GetExitCodeAsync(process.Id, ct).ConfigureAwait(false);
                }
                catch (Exception linuxEx)
                {
                    throw new AggregateException(linuxEx, ex);
                }
            }

            return exitCode;
        }
    }
}
