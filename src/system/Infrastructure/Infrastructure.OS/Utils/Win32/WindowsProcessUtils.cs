using System;
using System.Management;
using System.Runtime.Versioning;

namespace Infrastructure.OS.Utils
{
    [SupportedOSPlatform("windows")]
    internal static class WindowsProcessUtils
    {
        public static ProcessParameters GetProcessParameters(int pid)
        {
            string wql = $"SELECT ExecutablePath, CommandLine FROM Win32_Process WHERE ProcessId = {pid}";

            string? executable = null;
            string? commandLine = null;

            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(wql))
            using (ManagementObjectCollection results = searcher.Get())
            {
                foreach (ManagementBaseObject mo in results)
                {
                    executable = mo["ExecutablePath"]?.ToString();
                    commandLine = mo["CommandLine"]?.ToString();

                    if (!string.IsNullOrWhiteSpace(executable) && !string.IsNullOrWhiteSpace(commandLine))
                    {
                        break;
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(executable) || string.IsNullOrWhiteSpace(commandLine))
            {
                throw new InvalidOperationException($"Didn't get process parameters for process {pid}");
            }

            int executableStartIndex = commandLine.IndexOf(executable);

            string beginQuotes = commandLine.Substring(0, executableStartIndex);
            string endQuotes = ReverseString(beginQuotes);
            int endQuotesStartIndex = commandLine.IndexOf(endQuotes, executableStartIndex);

            int argsStartIndex = endQuotesStartIndex + endQuotes.Length;
            string args = commandLine.Substring(argsStartIndex, commandLine.Length - argsStartIndex);
            args = args.Trim();

            return new ProcessParameters(executable, args);
        }

        private static string ReverseString(string str)
        {
            string reversed = string.Create(
                str.Length,
                str,
                (chars, state) =>
                {
                    var pos = 0;
                    for (int i = state.Length - 1; i >= 0; i--)
                    {
                        chars[pos++] = state[i];
                    }
                });

            return reversed;
        }
    }
}
