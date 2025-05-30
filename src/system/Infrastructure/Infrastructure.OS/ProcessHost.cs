using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.OS
{
    public class ProcessHost : IAsyncDisposable
    {
        private const int c_waitForExitDelayInMs = 1000;
        private const int c_waitForExitRetries = 5;

        private readonly ILogger<ProcessHost> m_logger;
        private readonly FileInfo m_executable;
        private readonly DirectoryInfo m_workingDir;
        private readonly string? m_args;

        private Process? m_process;
        private bool m_disposed = false;

        public ProcessStatus Status { get; private set; } = ProcessStatus.NotStarted;

        public int ProcessId
        {
            get
            {
                return m_process?.Id ?? throw new InvalidOperationException($"Process '{m_executable.FullName}' is not started");
            }
        }

        public event EventHandler<ProcessExitedEventArgs>? Exited;


        public ProcessHost(ILogger<ProcessHost> logger, FileInfo executable, DirectoryInfo workingDir, string? args)
        {
            m_logger = logger;
            m_executable = executable;
            m_workingDir = workingDir;
            m_args = args;
        }

        public async ValueTask DisposeAsync()
        {
            m_disposed = true;
            GC.SuppressFinalize(this);

            if (m_process is null)
            {
                return;
            }

            if (!m_process.HasExited)
            {
                await StopAsync();
            }
            m_process.Dispose();
        }


        public void Start()
        {
            ObjectDisposedException.ThrowIf(m_disposed, this);

            m_logger.LogInformation("Starting process '{ProcessPath}' with args {Args} inside working directory '{WorkingDirectory}'", m_executable.FullName, m_args, m_workingDir.FullName);

            if (Status == ProcessStatus.Starting ||
                Status == ProcessStatus.Running)
            {
                throw new InvalidOperationException($"Process '{m_executable.FullName}' is already running with id {m_process?.Id}");
            }

            Status = ProcessStatus.Starting;

            ValidatePaths();

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = m_executable.FullName,
                Arguments = m_args,
                WorkingDirectory = m_workingDir.FullName,
                RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process process = new Process()
            {
                StartInfo = startInfo,
                EnableRaisingEvents = true
            };

            process.Exited += OnProcessExited;
            m_process = process;

            if (!process.Start())
            {
                Status = ProcessStatus.FailedToStart;
                throw new InvalidOperationException($"Failed to start process '{m_executable.FullName}'");
            }

            Status = ProcessStatus.Running;
        }

        public void Reattach(int pid)
        {
            ObjectDisposedException.ThrowIf(m_disposed, this);

            m_logger.LogInformation(
                "Reattaching to the process '{ProcessPath}' with args {Args} inside working directory '{WorkingDirectory}' with pid {PID}",
                m_executable.FullName,
                m_args,
                m_workingDir.FullName,
                pid);

            if (Status != ProcessStatus.NotStarted)
            {
                throw new InvalidOperationException($"Can reattach only when status is {ProcessStatus.NotStarted}");
            }

            ValidatePaths();

            Process process = Process.GetProcessById(pid);
            if (process == null)
            {
                throw new InvalidOperationException($"Process {pid} is not found");
            }

            ProcessStartInfo processStartInfo = process.StartInfo;
            if (processStartInfo.FileName != m_executable.FullName)
            {
                throw new InvalidOperationException($"Process {pid} has different executable. Expected '{m_executable.FullName}' but having '{processStartInfo.FileName}'");
            }

            if (processStartInfo.WorkingDirectory != m_workingDir.FullName)
            {
                throw new InvalidOperationException($"Process {pid} has different working directory. Expected '{m_workingDir.FullName}' but having '{processStartInfo.WorkingDirectory}'");
            }

            if (processStartInfo.Arguments != m_args)
            {
                throw new InvalidOperationException($"Process {pid} has different executable. Expected '{m_args}' but having '{processStartInfo.Arguments}'");
            }

            process.Exited += OnProcessExited;
        }

        public async Task StopAsync(CancellationToken ct = default)
        {
            ObjectDisposedException.ThrowIf(m_disposed, this);

            m_logger.LogInformation(
                "Stopping process '{ProcessPath}' with args {Args} inside working directory '{WorkingDirectory}' with pid {PID}",
                m_executable.FullName,
                m_args,
                m_workingDir.FullName,
                m_process?.Id);

            if (Status == ProcessStatus.Starting)
            {
                for (int i = c_waitForExitRetries - 1; i >= 0; i--)
                {
                    if (Status == ProcessStatus.Starting)
                    {
                        await Task.Delay(c_waitForExitDelayInMs, ct).ConfigureAwait(false);
                    }
                }
            }

            if (Status == ProcessStatus.Running && m_process != null)
            {
                m_process.Kill(true);
                return;
            }

            m_logger.LogWarning(
                "Can't stop the process '{ProcessPath}' with args {Args} inside working directory '{WorkingDirectory}' with pid {PID}",
                m_executable.FullName,
                m_args,
                m_workingDir.FullName,
                m_process?.Id);
        }

        public async Task SendCommandAsync(string command)
        {
            ObjectDisposedException.ThrowIf(m_disposed, this);

            m_logger.LogInformation(
                "Sending command '{Command}' to the process '{ProcessPath}' with args {Args} inside working directory '{WorkingDirectory}' with pid {PID}",
                command,
                m_executable.FullName,
                m_args,
                m_workingDir.FullName,
                m_process?.Id);

            if (Status != ProcessStatus.Running || m_process == null)
            {
                throw new InvalidOperationException($"Process '{m_executable.FullName}' is not running");
            }

            await m_process!.StandardInput.WriteLineAsync(command).ConfigureAwait(false);
            await m_process!.StandardInput.FlushAsync().ConfigureAwait(false);
        }


        private void OnProcessExited(object? sender, EventArgs e)
        {
            m_logger.LogInformation(
                "Process '{ProcessPath}' with args {Args} inside working directory '{WorkingDirectory}' with pid {PID} has exited",
                m_executable.FullName,
                m_args,
                m_workingDir.FullName,
                m_process?.Id);

            Status = ProcessStatus.Exited;

            if (m_process is null)
            {
                return;
            }

            Exited?.Invoke(this, new ProcessExitedEventArgs(m_process.ExitCode));
        }

        private void ValidatePaths()
        {
            m_executable.Refresh();
            if (!m_executable.Exists)
            {
                throw new FileNotFoundException($"Executable '{m_executable.FullName}' not found");
            }

            m_workingDir.Refresh();
            if (!m_workingDir.Exists)
            {
                throw new DirectoryNotFoundException($"Directory '{m_workingDir.FullName}' not found");
            }
        }
    }
}
