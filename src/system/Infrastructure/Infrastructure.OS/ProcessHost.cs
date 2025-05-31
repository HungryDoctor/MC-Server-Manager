using Infrastructure.OS.Utils;
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
        private const int c_waitForProcessExitInMs = 10000;

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
            if (m_disposed)
            {
                return;
            }

            GC.SuppressFinalize(this);

            if (m_process is null)
            {
                return;
            }

            if (!m_process.HasExited)
            {
                try
                {
                    await StopAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    m_logger.LogError(ex, "Error occurred on stopping process {PID}", m_process?.Id);
                }
            }

            m_disposed = true;
        }


        public int Start()
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
            return process.Id;
        }

        public async Task ReattachAsync(int pid, CancellationToken ct = default)
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

            ProcessParameters processParameters = await ProcessUtils.GetGetProcessParametersAsync(pid, ct).ConfigureAwait(false);
            if (processParameters.Executable != m_executable.FullName)
            {
                throw new InvalidOperationException($"Process {pid} has different executable. Expected '{m_executable.FullName}' but having '{process.Modules[0].FileName}'");
            }

            if (processParameters.Arguments != (m_args ?? ""))
            {
                throw new InvalidOperationException($"Process {pid} has different executable. Expected '{m_args}' but having '{processParameters.Arguments}'");
            }

            if (m_process != null)
            {
                m_process.Exited -= OnProcessExited;
                m_process.Dispose();
            }

            process.EnableRaisingEvents = true;
            process.Exited += OnProcessExited;
            m_process = process;

            Status = ProcessStatus.Running;
        }

        public Task StopAsync(CancellationToken ct = default)
        {
            return StopAsync(TimeSpan.FromSeconds(c_waitForProcessExitInMs), ct);
        }

        public async Task StopAsync(TimeSpan waitForExit, CancellationToken ct = default)
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

            if (Status != ProcessStatus.Running || m_process == null)
            {
                m_logger.LogWarning(
                    "Can't stop the process '{ProcessPath}' with args {Args} inside working directory '{WorkingDirectory}' with pid {PID}",
                    m_executable.FullName,
                    m_args,
                    m_workingDir.FullName,
                    m_process?.Id);

                return;
            }

            try
            {
                m_process.Kill(true);

                using (CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(ct))
                {
                    cts.CancelAfter(waitForExit);
                    await m_process.WaitForExitAsync(cts.Token).ConfigureAwait(false);
                }
            }
            finally
            {
                m_process.Exited -= OnProcessExited;
                m_process.Dispose();

                Status = ProcessStatus.Exited;
            }
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

            if (m_process is null)
            {
                Status = ProcessStatus.Exited;
                return;
            }

            if (!m_process.HasExited && !m_process.WaitForExit(c_waitForProcessExitInMs))
            {
                m_logger.LogWarning("Didn't wait for process '{ProcessPath}' to exit. Proceeding...", m_executable.FullName);
            }
            Status = ProcessStatus.Exited;

            int exitCode = ProcessUtils.GetExitCodeAsync(m_process).GetAwaiter().GetResult();
            Exited?.Invoke(this, new ProcessExitedEventArgs(exitCode));
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
