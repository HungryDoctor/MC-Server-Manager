using Contracts.Lifecycle;
using Infrastructure.OS.Processes;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Lifecycle.ServerProcess
{
    public class ServerProcessHost : IServerProcessHost
    {
        private readonly ILogger<ServerProcessHost> m_logger;
        private readonly ProcessHost m_processHost;
        private readonly ReplaySubject<ConsoleOutput> m_outputBuffer;
        private int m_activeReaders;
        private bool m_disposed;

        public ProcessStatus Status => m_processHost.Status;
        public int ProcessId => m_processHost.ProcessId;

        public event PropertyChangedEventHandler? PropertyChanged;


        public ServerProcessHost(ILogger<ServerProcessHost> logger, ProcessHost processHost)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(processHost);

            m_logger = logger;
            m_processHost = processHost;
            m_processHost.OutputReceived += ProcessHost_OutputReceived;
            m_processHost.ErrorReceived += ProcessHost_ErrorReceived;
            m_processHost.Exited += ProcessHost_Exited;
            m_processHost.PropertyChanged += ProcessHost_PropertyChanged;

            m_outputBuffer = new ReplaySubject<ConsoleOutput>();
        }

        public async ValueTask DisposeAsync()
        {
            if (m_disposed)
            {
                return;
            }

            try
            {
                m_processHost.OutputReceived -= ProcessHost_OutputReceived;
                m_processHost.ErrorReceived -= ProcessHost_ErrorReceived;
                m_processHost.Exited -= ProcessHost_Exited;
                m_processHost.PropertyChanged -= ProcessHost_PropertyChanged;

                await m_processHost.DisposeAsync().ConfigureAwait(false);

                PropertyChanged = null;

                if (Volatile.Read(ref m_activeReaders) == 0)
                {
                    m_outputBuffer.Dispose();
                }
            }
            catch (Exception ex)
            {
                m_logger.LogError(ex, "Error occurred during dispose");
            }

            GC.SuppressFinalize(this);
            m_disposed = true;
        }


        public int Start()
        {
            ObjectDisposedException.ThrowIf(m_disposed, this);

            return m_processHost.Start();
        }

        public Task StopAsync(CancellationToken ct = default)
        {
            ObjectDisposedException.ThrowIf(m_disposed, this);

            return m_processHost.StopAsync(ct);
        }

        public Task StopAsync(TimeSpan waitForExit, CancellationToken ct = default)
        {
            ObjectDisposedException.ThrowIf(m_disposed, this);

            return m_processHost.StopAsync(waitForExit, ct);
        }

        public Task SendCommandAsync(string command, CancellationToken ct = default)
        {
            ObjectDisposedException.ThrowIf(m_disposed, this);

            return m_processHost.SendCommandAsync(command, ct);
        }

        public async IAsyncEnumerable<ConsoleOutput> GetOutputBufferAsync([EnumeratorCancellation] CancellationToken ct = default)
        {
            ObjectDisposedException.ThrowIf(m_disposed, this);

            Interlocked.Increment(ref m_activeReaders);
            try
            {
                await foreach (ConsoleOutput line in m_outputBuffer.ToAsyncEnumerable().WithCancellation(ct))
                {
                    yield return line;
                }
            }
            finally
            {
                if (Interlocked.Decrement(ref m_activeReaders) == 0 && m_disposed)
                {
                    m_outputBuffer.Dispose();
                }
            }
        }

        private void ProcessHost_Exited(object? sender, ProcessExitedEventArgs e)
        {
            if (m_disposed)
            {
                return;
            }

            m_outputBuffer.OnCompleted();
        }

        private void ProcessHost_OutputReceived(object? sender, ProcessDataReceivedEventArgs e)
        {
            if (m_disposed)
            {
                return;
            }

            if (e.Data == null)
            {
                return;
            }

            m_outputBuffer.OnNext(new ConsoleOutput(OutputLevel.Normal, e.Data));
        }

        private void ProcessHost_ErrorReceived(object? sender, ProcessDataReceivedEventArgs e)
        {
            if (m_disposed)
            {
                return;
            }

            if (e.Data == null)
            {
                return;
            }

            m_outputBuffer.OnNext(new ConsoleOutput(OutputLevel.Error, e.Data));
        }

        private void ProcessHost_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            string? propertyName = e.PropertyName;
            if (propertyName == nameof(IServerProcessHost.Status) ||
                propertyName == nameof(IServerProcessHost.ProcessId))
            {
                NotifyPropertyChanged(propertyName);
            }
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
