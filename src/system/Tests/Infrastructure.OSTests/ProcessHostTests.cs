using Infrastructure.OS.Processes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.OSTests
{
    public class ProcessHostTests
    {
        private const int c_waitForProcessExitInMs = 20000;
        private const int c_waitForProcessOutputInMs = 5000;
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
            ProcessHost host = CreateProcessHost();

            await Assert.That(host.Status).IsEqualTo(ProcessStatus.NotStarted);
        }

        [Test]
        public async Task Status_Running_After_Start_Async()
        {
            ProcessHost host = CreateProcessHost();
            host.Start();

            await Assert.That(host.Status).IsEqualTo(ProcessStatus.Running);
        }

        [Test]
        public async Task Status_Exited_After_Stop_Async()
        {
            AutoResetEvent autoResetEvent = new AutoResetEvent(false);
            ProcessExitedEventArgs processExitedEventArgs = null!;
            ProcessHost exitHost = null!;

            ProcessHost host = CreateProcessHost();
            host.Exited += Host_Exited;

            host.Start();
            await host.StopAsync().ConfigureAwait(false);
            autoResetEvent.WaitOne(c_waitForProcessExitInMs);

            await Assert.That(host.Status).IsEqualTo(ProcessStatus.Exited);
            await Assert.That(processExitedEventArgs.ExitCode).IsNotEqualTo(0);
            await Assert.That(exitHost).IsEqualTo(host);


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
            AutoResetEvent autoResetEvent = new AutoResetEvent(false);
            ProcessExitedEventArgs processExitedEventArgs = null!;

            ProcessHost host = CreateProcessHost(s_dummyConsoleAppFileInfo, new DirectoryInfo("./"), "-explode");
            host.Exited += Host_Exited;

            host.Start();
            autoResetEvent.WaitOne(c_waitForProcessExitInMs);

            await Assert.That(host.Status).IsEqualTo(ProcessStatus.Exited);
            await Assert.That(processExitedEventArgs.ExitCode).IsNotEqualTo(0);


            void Host_Exited(object? _, ProcessExitedEventArgs e)
            {
                processExitedEventArgs = e;
                autoResetEvent.Set();
            }
        }

        [Test]
        public async Task Dispose_StopsProcess_Async()
        {
            AutoResetEvent autoResetEvent = new AutoResetEvent(false);
            ProcessExitedEventArgs processExitedEventArgs = null!;

            ProcessHost host = CreateProcessHost();
            host.Exited += Host_Exited;

            host.Start();
            await host.DisposeAsync().ConfigureAwait(false);

            await Assert.That(host.Status).IsEqualTo(ProcessStatus.Exited);
            await Assert.That(processExitedEventArgs.ExitCode).IsNotEqualTo(0);


            void Host_Exited(object? _, ProcessExitedEventArgs e)
            {
                processExitedEventArgs = e;
                autoResetEvent.Set();
            }
        }

        [Test]
        public async Task Status_Running_After_StartStop_Async()
        {
            AutoResetEvent autoResetEvent = new AutoResetEvent(false);
            ProcessExitedEventArgs processExitedEventArgs = null!;

            ProcessHost host = CreateProcessHost();
            host.Exited += Host_Exited;

            host.Start();
            await Assert.That(host.Status).IsEqualTo(ProcessStatus.Running);

            await host.StopAsync().ConfigureAwait(false);
            autoResetEvent.WaitOne(c_waitForProcessExitInMs);

            await Assert.That(host.Status).IsEqualTo(ProcessStatus.Exited);
            await Assert.That(processExitedEventArgs.ExitCode).IsNotEqualTo(0);

            host.Start();
            await Assert.That(host.Status).IsEqualTo(ProcessStatus.Running);


            void Host_Exited(object? _, ProcessExitedEventArgs e)
            {
                processExitedEventArgs = e;
                autoResetEvent.Set();
            }
        }

        [Test]
        public async Task OutputStreams_CanBeRead_Async()
        {
            AutoResetEvent errorAutoResetEvent = new AutoResetEvent(false);
            AutoResetEvent dataAutoResetEvent = new AutoResetEvent(false);
            ProcessDataReceivedEventArgs errorEventArgs = null!;
            ProcessDataReceivedEventArgs dataEventArgs = null!;

            ProcessHost host = CreateProcessHost(s_dummyConsoleAppFileInfo, new DirectoryInfo("./"), "-explode");

            try
            {
                host.ErrorReceived += Host_ErrorReceived;
                host.OutputReceived += Host_OutputReceived;

                host.Start();
                errorAutoResetEvent.WaitOne(c_waitForProcessOutputInMs);
                dataAutoResetEvent.WaitOne(c_waitForProcessOutputInMs);
            }
            finally
            {
                await host.DisposeAsync().ConfigureAwait(false);
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
        public async Task Command_CanBeSent_Async()
        {
            AutoResetEvent dataAutoResetEvent = new AutoResetEvent(false);
            ProcessDataReceivedEventArgs dataEventArgs = null!;

            ProcessHost host = CreateProcessHost(s_dummyConsoleAppFileInfo, new DirectoryInfo("./"), "SomeArgs");

            string command = null!;
            try
            {
                host.OutputReceived += Host_OutputReceived;

                host.Start();

                await Task.Delay(1000).ConfigureAwait(false);

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
            finally
            {
                await host.DisposeAsync().ConfigureAwait(false);
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
        public void Start_InvalidExecutable_ThrowsFileNotFoundException()
        {
            FileInfo executable = new FileInfo("NotExistingFile.exe");
            DirectoryInfo dir = new DirectoryInfo("./");
            ProcessHost host = CreateProcessHost(executable, dir, null);

            Assert.Throws<FileNotFoundException>(() => host.Start());
        }

        [Test]
        public void Start_InvalidWorkingDirectory_ThrowsDirectoryNotFoundException()
        {
            DirectoryInfo dir = new DirectoryInfo("NotExistingFolder");
            ProcessHost host = CreateProcessHost(s_dummyConsoleAppFileInfo, dir, null);

            Assert.Throws<DirectoryNotFoundException>(() => host.Start());
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
