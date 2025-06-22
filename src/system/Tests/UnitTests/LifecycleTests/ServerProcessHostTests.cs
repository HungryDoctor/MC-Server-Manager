using Contracts.Lifecycle;
using Infrastructure.OS.Processes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ProcessTestsBase;
using Services.Lifecycle.ServerProcess;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LifecycleTests
{
    public class ServerProcessHostTests : DummyProcessTestBase
    {
        [Test]
        public async Task Status_Reflects_Lifecycle_Async()
        {
            bool statusChanged = false;
            bool idChanged = false;

            await using (ServerProcessHost host = CreateServerProcessHost())
            {
                host.PropertyChanged += PropertyChanged;

                await Assert.That(host.Status).IsEqualTo(ProcessStatus.NotStarted);

                host.Start();
                await Assert.That(host.Status).IsEqualTo(ProcessStatus.Running);
                await Assert.That(host.ProcessId).IsGreaterThan(0);

                await host.StopAsync();
                await Assert.That(host.Status).IsEqualTo(ProcessStatus.Exited);
                await Assert.That(statusChanged).IsTrue();
                await Assert.That(idChanged).IsTrue();
            }


            void PropertyChanged(object? sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName == nameof(IServerProcessHost.Status))
                {
                    statusChanged = true;
                }

                if (e.PropertyName == nameof(IServerProcessHost.ProcessId))
                {
                    idChanged = true;
                }
            }
        }

        [Test]
        public async Task OutputBuffer_Streams_NormalAndErrorLines_Async()
        {
            AutoResetEvent autoResetEvent = null!;
            await using (ServerProcessHost host = CreateServerProcessHost("-explode"))
            using (autoResetEvent = new AutoResetEvent(false))
            {
                List<ConsoleOutput> outputList = new List<ConsoleOutput>();

                Task reader = Task.Run(async () =>
                {
                    await foreach (ConsoleOutput line in host.GetOutputBufferAsync())
                    {
                        outputList.Add(line);
                    }

                    autoResetEvent.Set();
                });

                host.Start();
                autoResetEvent.WaitOne(c_waitForProcessExitInMs);

                await Assert.That(host.Status).IsEqualTo(ProcessStatus.Exited);
                await reader.ConfigureAwait(false);

                await Assert.That(outputList.Count).IsGreaterThan(0);
                await Assert.That(outputList.Any(o => o.OutputLevel == OutputLevel.Normal)).IsTrue();
                await Assert.That(outputList.Any(o => o.OutputLevel == OutputLevel.Error)).IsTrue();
            }
        }

        [Test]
        public async Task OutputBuffer_CompletesGracefully_OnDispose_Async()
        {
            AutoResetEvent autoResetEvent = null!;
            await using (ServerProcessHost host = CreateServerProcessHost())
            using (CancellationTokenSource cts = new CancellationTokenSource())
            using (autoResetEvent = new AutoResetEvent(false))
            {
                List<ConsoleOutput> outputList = new List<ConsoleOutput>();

                Task readingTask = Task.Run(async () =>
                {
                    await foreach (ConsoleOutput line in host.GetOutputBufferAsync(cts.Token))
                    {
                        outputList.Add(line);
                    }
                });

                host.Start();
                await Task.Delay(c_waitForProcessToStartInMs).ConfigureAwait(false);
                await host.DisposeAsync().ConfigureAwait(false);

                autoResetEvent.WaitOne(c_waitForProcessExitInMs);
                await readingTask.ConfigureAwait(false);

                await Assert.That(host.Status).IsEqualTo(ProcessStatus.Exited);
                await Assert.That(outputList.Count).IsGreaterThan(0);
            }
        }

        [Test]
        public async Task OutputBuffer_Replays_Async()
        {
            await using (ServerProcessHost host = CreateServerProcessHost())
            {
                host.Start();

                await Task.Delay(c_waitForProcessToStartInMs * 2).ConfigureAwait(false);
                await host.StopAsync().ConfigureAwait(false);

                List<ConsoleOutput> outputList = new List<ConsoleOutput>();
                await foreach (ConsoleOutput line in host.GetOutputBufferAsync())
                {
                    outputList.Add(line);
                }

                await Assert.That(outputList.Count).IsGreaterThan(0);
                await Assert.That(outputList.Any(x => !string.IsNullOrWhiteSpace(x.OutputString))).IsTrue();
            }
        }


        private static ServerProcessHost CreateServerProcessHost(string? args = null)
        {
            DirectoryInfo workDir = new DirectoryInfo("./");
            ILogger<ProcessHost> processHostLogger = NullLoggerFactory.Instance.CreateLogger<ProcessHost>();
            ILogger<ServerProcessHost> serverHostLogger = NullLoggerFactory.Instance.CreateLogger<ServerProcessHost>();

            ProcessHost processHost = new ProcessHost(processHostLogger, s_dummyConsoleAppFileInfo, workDir, args);
            return new ServerProcessHost(serverHostLogger, processHost);
        }
    }
}
