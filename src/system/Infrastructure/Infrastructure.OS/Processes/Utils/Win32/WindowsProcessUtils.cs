using Infrastructure.Extensions;
using Infrastructure.OS.Processes.Utils;
using System;
using System.Management;
using System.Runtime.Versioning;

namespace Infrastructure.OS.Processes.Utils.Win32
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
            string beginQuotes = commandLine[..executableStartIndex];
            string endQuotes = beginQuotes.ReverseString();
            int endQuotesStartIndex = commandLine.IndexOf(endQuotes, executableStartIndex);
            int argsStartIndex = endQuotesStartIndex + endQuotes.Length;

            string args = commandLine[argsStartIndex..];
            args = args.Trim();

            return new ProcessParameters(executable, args);
        }
    }
}
