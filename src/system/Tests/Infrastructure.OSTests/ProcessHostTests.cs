using Infrastructure.OS;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TUnit.Assertions.Extensions;

namespace Infrastructure.OSTests
{
    public class ProcessHostTests
    {
        private const int c_waitForProcessExitInMs = 20000;
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
                autoResetEvent.Set();
                exitHost = (sender as ProcessHost)!;
                processExitedEventArgs = e;
            }
        }

        [Test]
        public async Task Status_Exited_After_Reattach_Async()
        {
            AutoResetEvent autoResetEvent = new AutoResetEvent(false);
            ProcessExitedEventArgs processExitedEventArgs = null!;

            ProcessHost host1 = CreateProcessHost();
            int pid = host1.Start();

            try
            {
                ProcessHost host2 = CreateProcessHost();
                host2.Exited += Host_Exited;
                await host2.ReattachAsync(pid).ConfigureAwait(false);

                await Assert.That(host2.Status).IsEqualTo(ProcessStatus.Running);

                await host2.StopAsync().ConfigureAwait(false);
                autoResetEvent.WaitOne(c_waitForProcessExitInMs);

                await Assert.That(host2.Status).IsEqualTo(ProcessStatus.Exited);
                await Assert.That(processExitedEventArgs.ExitCode).IsNotEqualTo(0);
            }
            finally
            {
                await host1.StopAsync().ConfigureAwait(false);
            }

            void Host_Exited(object? sender, ProcessExitedEventArgs e)
            {
                autoResetEvent.Set();
                processExitedEventArgs = e;
            }
        }

        [Test]
        public async Task Status_Exited_After_StartAndError_Async()
        {
            AutoResetEvent autoResetEvent = new AutoResetEvent(false);
            ProcessExitedEventArgs processExitedEventArgs = null!;

            DirectoryInfo dir = new DirectoryInfo("./");
            ProcessHost host = CreateProcessHost(s_dummyConsoleAppFileInfo, dir, "-explode");
            host.Exited += Host_Exited;

            host.Start();
            autoResetEvent.WaitOne(c_waitForProcessExitInMs);

            await Assert.That(host.Status).IsEqualTo(ProcessStatus.Exited);
            await Assert.That(processExitedEventArgs.ExitCode).IsNotEqualTo(0);


            void Host_Exited(object? sender, ProcessExitedEventArgs e)
            {
                autoResetEvent.Set();
                processExitedEventArgs = e;
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


            void Host_Exited(object? sender, ProcessExitedEventArgs e)
            {
                autoResetEvent.Set();
                processExitedEventArgs = e;
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


            void Host_Exited(object? sender, ProcessExitedEventArgs e)
            {
                autoResetEvent.Set();
                processExitedEventArgs = e;
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

        [Test]
        public async Task Reattach_WrongExecutable_Throws_InvalidOperationException_Async()
        {
            ProcessHost host1 = CreateProcessHost();
            int pid = host1.Start();

            string tempFileName = Guid.NewGuid().ToString();
            FileInfo tempFile = new FileInfo(tempFileName);
            try
            {
                await File.Create(tempFileName).DisposeAsync().ConfigureAwait(false);

                DirectoryInfo dir = new DirectoryInfo("./");
                ProcessHost host2 = CreateProcessHost(tempFile, dir, null);

                await Assert.ThrowsAsync<InvalidOperationException>(() => host2.ReattachAsync(pid)).ConfigureAwait(false);
            }
            finally
            {
                tempFile.Delete();
                await host1.StopAsync().ConfigureAwait(false);
            }
        }

        [Test]
        public async Task Reattach_WrongArgs_Throws_InvalidOperationException_Async()
        {
            DirectoryInfo dir = new DirectoryInfo("./");

            ProcessHost host1 = CreateProcessHost(s_dummyConsoleAppFileInfo, dir, "someArgs");
            int pid = host1.Start();

            try
            {
                ProcessHost host2 = CreateProcessHost(s_dummyConsoleAppFileInfo, dir, "anotherArgs");

                await Assert.ThrowsAsync<InvalidOperationException>(() => host2.ReattachAsync(pid)).ConfigureAwait(false);
            }
            finally
            {
                await host1.StopAsync().ConfigureAwait(false);
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
