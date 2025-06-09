using Infrastructure.OS.Processes.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.OS.Processes
{
    public class ProcessHost : IProcessHost, IAsyncDisposable, INotifyPropertyChanged
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
        private bool m_processDisposed = false;
        private ProcessStatus m_statusField = ProcessStatus.NotStarted;

        public ProcessStatus Status
        {
            get => m_statusField;

            private set
            {
                if (value != m_statusField)
                {
                    m_statusField = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int ProcessId
        {
            get
            {
                return m_process?.Id ?? -1;
            }
        }

        public event EventHandler<ProcessExitedEventArgs>? Exited;
        public event EventHandler<ProcessDataReceivedEventArgs>? ErrorReceived;
        public event EventHandler<ProcessDataReceivedEventArgs>? OutputReceived;
        public event PropertyChangedEventHandler? PropertyChanged;

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

            await SafeDisposeProcessAsync().ConfigureAwait(false);

            Exited = null;
            ErrorReceived = null;
            OutputReceived = null;
            PropertyChanged = null;

            GC.SuppressFinalize(this);
            m_disposed = true;


            async Task SafeDisposeProcessAsync()
            {
                if (m_process is null)
                {
                    return;
                }

                if (m_processDisposed)
                {
                    return;
                }

                try
                {
                    if (m_process.HasExited)
                    {
                        m_process.Exited -= OnProcessExited;
                        m_process.ErrorDataReceived -= OnErrorReceived;
                        m_process.OutputDataReceived -= OnDataReceived;
                        m_process.Dispose();
                    }
                    else
                    {
                        try
                        {
                            await StopAsync().ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            m_logger.LogError(ex, "Error occurred on disposing process host");
                        }
                    }
                }
                catch (Exception ex)
                {
                    m_logger.LogError(ex, "Error occurred on disposing process host");
                }
            }
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
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process process = new Process()
            {
                StartInfo = startInfo,
                EnableRaisingEvents = true
            };

            process.Disposed += Process_Disposed;
            process.Exited += OnProcessExited;
            process.ErrorDataReceived += OnErrorReceived;
            process.OutputDataReceived += OnDataReceived;

            m_processDisposed = false;
            m_process = process;

            if (!process.Start())
            {
                Status = ProcessStatus.FailedToStart;
                throw new InvalidOperationException($"Failed to start process '{m_executable.FullName}'");
            }

            process.BeginErrorReadLine();
            process.BeginOutputReadLine();
            NotifyPropertyChanged(nameof(ProcessId));

            Status = ProcessStatus.Running;
            return process.Id;
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
                m_process.ErrorDataReceived -= OnErrorReceived;
                m_process.OutputDataReceived -= OnDataReceived;
                m_process.Dispose();

                Status = ProcessStatus.Exited;
            }
        }

        public async Task SendCommandAsync(string command, CancellationToken ct = default)
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

            await m_process!.StandardInput.WriteLineAsync(command.AsMemory(), ct).ConfigureAwait(false);
            await m_process!.StandardInput.FlushAsync(ct).ConfigureAwait(false);
        }


        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Process_Disposed(object? sender, EventArgs e)
        {
            m_processDisposed = true;
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
                NotifyPropertyChanged(nameof(ProcessId));
                Status = ProcessStatus.Exited;

                return;
            }

            if (!m_process.HasExited && !m_process.WaitForExit(c_waitForProcessExitInMs))
            {
                m_logger.LogWarning("Didn't wait for process '{ProcessPath}' with pid {PID} to exit. Proceeding...", m_executable.FullName, m_process.Id);
            }

            NotifyPropertyChanged(nameof(ProcessId));
            Status = ProcessStatus.Exited;

            int? exitCode = null;
            try
            {
                exitCode = ProcessUtils.GetExitCodeAsync(m_process).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                m_logger.LogWarning(ex, "Failed to get exit code for '{ProcessPath}' with pid {PID} to exit. Proceeding...", m_executable.FullName, m_process.Id);
            }

            Exited?.Invoke(this, new ProcessExitedEventArgs(m_process?.Id, exitCode));
        }

        private void OnErrorReceived(object sender, DataReceivedEventArgs e)
        {
            ErrorReceived?.Invoke(this, new ProcessDataReceivedEventArgs(m_process?.Id, e.Data));
        }

        private void OnDataReceived(object sender, DataReceivedEventArgs e)
        {
            OutputReceived?.Invoke(this, new ProcessDataReceivedEventArgs(m_process?.Id, e.Data));
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
