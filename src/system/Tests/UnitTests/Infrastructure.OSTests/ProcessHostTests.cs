using Infrastructure.OS.Processes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.OSTests
{
    public class ProcessHostTests
    {
        private const int c_waitForProcessExitInMs = 20000;
        private const int c_waitForProcessOutputInMs = 5000;
        private const int c_waitForProcessToStartInMs = 1000;
        private static FileInfo s_dummyConsoleAppFileInfo = null!;


        [Before(HookType.Class)]
        public static void InitializeTest()
        {
            const string dummyConsoleAppName = "./DummyConsoleApp";

            if (OperatingSystem.IsWindows())
            {
                s_dummyConsoleAppFileInfo = new FileInfo($"{dummyConsoleAppName}.exe");
            }
            else if (OperatingSystem.IsLinux())
            {
                s_dummyConsoleAppFileInfo = new FileInfo(dummyConsoleAppName);
            }
            else
            {
                throw new PlatformNotSupportedException("Unknown OS for testing.");
            }

            s_dummyConsoleAppFileInfo.Refresh();
            if (!s_dummyConsoleAppFileInfo.Exists)
            {
                throw new InvalidOperationException($"Dummy console app is missing by path '{s_dummyConsoleAppFileInfo.FullName}'");
            }
        }


        [Test]
        public async Task Status_NotStarted_After_Creation_Async()
        {
            await using (ProcessHost host = CreateProcessHost())
            {
                await Assert.That(host.Status).IsEqualTo(ProcessStatus.NotStarted);
            }
        }

        [Test]
        public async Task Status_Running_After_Start_Async()
        {
            await using (ProcessHost host = CreateProcessHost())
            {
                host.Start();

                await Assert.That(host.Status).IsEqualTo(ProcessStatus.Running);
                await Assert.That(host.ProcessId).IsGreaterThan(0);
            }
        }

        [Test]
        public async Task PropertyChanged_After_StatusChange_Async()
        {
            bool statusChanged = false;
            bool processIdChanged = false;

            await using (ProcessHost host = CreateProcessHost())
            {
                host.PropertyChanged += Host_PropertyChanged;
                host.Start();

                await Assert.That(statusChanged).IsTrue();
                await Assert.That(processIdChanged).IsTrue();
            }


            void Host_PropertyChanged(object? sender, PropertyChangedEventArgs e)
            {
                if (e?.PropertyName == nameof(IProcessHost.Status))
                {
                    statusChanged = true;
                }

                if (e?.PropertyName == nameof(IProcessHost.ProcessId))
                {
                    processIdChanged = true;
                }
            }
        }

        [Test]
        public async Task Status_Exited_After_Stop_Async()
        {
            AutoResetEvent autoResetEvent = null!;
            ProcessExitedEventArgs processExitedEventArgs = null!;
            ProcessHost exitHost = null!;

            using (autoResetEvent = new AutoResetEvent(false))
            await using (ProcessHost host = CreateProcessHost())
            {
                host.Exited += Host_Exited;

                host.Start();
                await host.StopAsync().ConfigureAwait(false);
                autoResetEvent.WaitOne(c_waitForProcessExitInMs);

                await Assert.That(host.Status).IsEqualTo(ProcessStatus.Exited);
                await Assert.That(processExitedEventArgs.ExitCode).IsNotEqualTo(0);
                await Assert.That(exitHost).IsEqualTo(host);
            }


            void Host_Exited(object? sender, ProcessExitedEventArgs e)
            {
                exitHost = (sender as ProcessHost)!;
                processExitedEventArgs = e;
                autoResetEvent.Set();
            }
        }

        [Test]
        public async Task Status_Exited_After_StartAndError_Async()
        {
            AutoResetEvent autoResetEvent = null!;
            ProcessExitedEventArgs processExitedEventArgs = null!;

            using (autoResetEvent = new AutoResetEvent(false))
            await using (ProcessHost host = CreateProcessHost(s_dummyConsoleAppFileInfo, new DirectoryInfo("./"), "-explode"))
            {
                host.Exited += Host_Exited;

                host.Start();
                autoResetEvent.WaitOne(c_waitForProcessExitInMs);

                await Assert.That(host.Status).IsEqualTo(ProcessStatus.Exited);
                await Assert.That(processExitedEventArgs.ExitCode).IsNotEqualTo(0);
            }


            void Host_Exited(object? _, ProcessExitedEventArgs e)
            {
                processExitedEventArgs = e;
                autoResetEvent.Set();
            }
        }

        [Test]
        public async Task Start_Twice_ThrowsInvalidOperationException_Async()
        {
            await using (ProcessHost host = CreateProcessHost())
            {
                host.Start();

                Assert.Throws<InvalidOperationException>(() => host.Start());
            }
        }

        [Test]
        public async Task DisposeAsync_StopsProcess_Async()
        {
            AutoResetEvent autoResetEvent = null!;
            ProcessExitedEventArgs processExitedEventArgs = null!;

            using (autoResetEvent = new AutoResetEvent(false))
            await using (ProcessHost host = CreateProcessHost())
            {
                host.Exited += Host_Exited;

                host.Start();
                await host.DisposeAsync().ConfigureAwait(false);

                await Assert.That(host.Status).IsEqualTo(ProcessStatus.Exited);
                await Assert.That(processExitedEventArgs.ExitCode).IsNotEqualTo(0);
            }


            void Host_Exited(object? _, ProcessExitedEventArgs e)
            {
                processExitedEventArgs = e;
                autoResetEvent.Set();
            }
        }

        [Test]
        public async Task DisposeAsync_WithoutStart_DoesNotThrow()
        {
            await using (ProcessHost host = CreateProcessHost())
            {
                await host.DisposeAsync().ConfigureAwait(false);

                await Assert.That(host.Status).IsEqualTo(ProcessStatus.NotStarted);
            }
        }

        [Test]
        public async Task Status_Running_After_StartStop_Async()
        {
            AutoResetEvent autoResetEvent = null!;
            ProcessExitedEventArgs processExitedEventArgs = null!;

            using (autoResetEvent = new AutoResetEvent(false))
            await using (ProcessHost host = CreateProcessHost())
            {
                host.Exited += Host_Exited;

                host.Start();
                await Assert.That(host.Status).IsEqualTo(ProcessStatus.Running);

                await host.StopAsync().ConfigureAwait(false);
                autoResetEvent.WaitOne(c_waitForProcessExitInMs);

                await Assert.That(host.Status).IsEqualTo(ProcessStatus.Exited);
                await Assert.That(processExitedEventArgs.ExitCode).IsNotEqualTo(0);

                host.Start();
                await Assert.That(host.Status).IsEqualTo(ProcessStatus.Running);
            }


            void Host_Exited(object? _, ProcessExitedEventArgs e)
            {
                processExitedEventArgs = e;
                autoResetEvent.Set();
            }
        }

        [Test]
        public async Task OutputStreams_CanBeRead_Async()
        {
            AutoResetEvent errorAutoResetEvent = null!;
            AutoResetEvent dataAutoResetEvent = null!;
            ProcessDataReceivedEventArgs errorEventArgs = null!;
            ProcessDataReceivedEventArgs dataEventArgs = null!;

            using (errorAutoResetEvent = new AutoResetEvent(false))
            using (dataAutoResetEvent = new AutoResetEvent(false))
            await using (ProcessHost host = CreateProcessHost(s_dummyConsoleAppFileInfo, new DirectoryInfo("./"), "-explode"))
            {
                host.ErrorReceived += Host_ErrorReceived;
                host.OutputReceived += Host_OutputReceived;

                host.Start();
                errorAutoResetEvent.WaitOne(c_waitForProcessOutputInMs);
                dataAutoResetEvent.WaitOne(c_waitForProcessOutputInMs);
            }

            await Assert.That(errorEventArgs.Data).IsNotEmpty();
            await Assert.That(dataEventArgs.Data).IsNotEmpty();


            void Host_ErrorReceived(object? _, ProcessDataReceivedEventArgs e)
            {
                errorEventArgs = e;
                errorAutoResetEvent.Set();
            }

            void Host_OutputReceived(object? _, ProcessDataReceivedEventArgs e)
            {
                dataEventArgs = e;
                dataAutoResetEvent.Set();
            }
        }

        [Test]
        public async Task SendCommandAsync_SendsCommand_Async()
        {
            AutoResetEvent dataAutoResetEvent = null!;
            ProcessDataReceivedEventArgs dataEventArgs = null!;
            string command = null!;

            using (dataAutoResetEvent = new AutoResetEvent(false))
            await using (ProcessHost host = CreateProcessHost(s_dummyConsoleAppFileInfo, new DirectoryInfo("./"), "SomeArgs"))
            {
                host.OutputReceived += Host_OutputReceived;

                host.Start();

                await Task.Delay(c_waitForProcessToStartInMs).ConfigureAwait(false);

                command = "command1";
                await host.SendCommandAsync(command).ConfigureAwait(false);
                dataAutoResetEvent.WaitOne();
                await Assert.That(dataEventArgs.Data).IsEqualTo(command);

                command = "command2";
                await host.SendCommandAsync(command).ConfigureAwait(false);
                dataAutoResetEvent.WaitOne();
                await Assert.That(dataEventArgs.Data).IsEqualTo(command);

                command = "command3";
                await host.SendCommandAsync(command).ConfigureAwait(false);
                dataAutoResetEvent.WaitOne();
                await Assert.That(dataEventArgs.Data).IsEqualTo(command);
            }


            void Host_OutputReceived(object? _, ProcessDataReceivedEventArgs e)
            {
                if (string.Equals(e.Data, command))
                {
                    dataEventArgs = e;
                    dataAutoResetEvent.Set();
                }
            }
        }

        [Test]
        public async Task Stop_WhenNotStarted_DoesNotThrow_Async()
        {
            await using (ProcessHost host = CreateProcessHost())
            {
                await host.StopAsync().ConfigureAwait(false);
                await Assert.That(host.Status).IsEqualTo(ProcessStatus.NotStarted);
            }
        }

        [Test]
        public async Task Stop_WhenAlreadyExited_DoesNotThrow()
        {
            AutoResetEvent autoResetEvent = null!;

            using (autoResetEvent = new AutoResetEvent(false))
            await using (ProcessHost host = CreateProcessHost(s_dummyConsoleAppFileInfo, new DirectoryInfo("./"), null))
            {
                host.Exited += Host_Exited;
                host.Start();

                await Task.Delay(c_waitForProcessToStartInMs).ConfigureAwait(false);
                await host.SendCommandAsync("stop").ConfigureAwait(false);

                autoResetEvent.WaitOne(c_waitForProcessExitInMs);
                await Assert.That(host.Status).IsEqualTo(ProcessStatus.Exited);

                await host.StopAsync();
                await Assert.That(host.Status).IsEqualTo(ProcessStatus.Exited);
            }


            void Host_Exited(object? _, ProcessExitedEventArgs __)
            {
                autoResetEvent.Set();
            }
        }

        [Test]
        public async Task Start_InvalidExecutable_ThrowsFileNotFoundException_Async()
        {
            FileInfo executable = new FileInfo("NotExistingFile.exe");
            DirectoryInfo dir = new DirectoryInfo("./");
            await using (ProcessHost host = CreateProcessHost(executable, dir, null))
            {
                Assert.Throws<FileNotFoundException>(() => host.Start());
            }
        }

        [Test]
        public async Task Start_InvalidWorkingDirectory_ThrowsDirectoryNotFoundException_Async()
        {
            DirectoryInfo dir = new DirectoryInfo("NotExistingFolder");
            await using (ProcessHost host = CreateProcessHost(s_dummyConsoleAppFileInfo, dir, null))
            {
                Assert.Throws<DirectoryNotFoundException>(() => host.Start());
            }
        }

        [Test]
        public async Task SendCommandAsync_WhenNotStarted_ThrowsInvalidOperationException_Async()
        {
            await using (ProcessHost host = CreateProcessHost())
            {
                await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                {
                    await host.SendCommandAsync("anything").ConfigureAwait(false);
                });
            }
        }

        [Test]
        public async Task SendCommand_AfterStop_ThrowsInvalidOperationException()
        {
            AutoResetEvent autoResetEvent = null!;

            using (autoResetEvent = new AutoResetEvent(false))
            await using (ProcessHost host = CreateProcessHost())
            {
                host.Exited += Host_Exited;

                host.Start();
                await Task.Delay(c_waitForProcessToStartInMs).ConfigureAwait(false);
                await host.SendCommandAsync("stop").ConfigureAwait(false);
                autoResetEvent.WaitOne(c_waitForProcessExitInMs);

                await Assert.That(host.Status).IsEqualTo(ProcessStatus.Exited);
                await Assert.ThrowsAsync<InvalidOperationException>(async () => await host.SendCommandAsync("never gonna happen").ConfigureAwait(false));
            }


            void Host_Exited(object? _, ProcessExitedEventArgs __)
            {
                autoResetEvent.Set();
            }
        }


        private static ProcessHost CreateProcessHost()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo("./");
            return CreateProcessHost(s_dummyConsoleAppFileInfo, directoryInfo, null);
        }

        private static ProcessHost CreateProcessHost(FileInfo executable, DirectoryInfo workingDir, string? args)
        {
            ILogger<ProcessHost> emptyLogger = NullLoggerFactory.Instance.CreateLogger<ProcessHost>();
            return new ProcessHost(emptyLogger, executable, workingDir, args);
        }
    }
}
