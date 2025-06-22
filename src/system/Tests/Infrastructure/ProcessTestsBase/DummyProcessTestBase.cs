using System;
using System.IO;

namespace ProcessTestsBase
{
    public abstract class DummyProcessTestBase
    {
        protected const int c_waitForProcessExitInMs = 20_000;
        protected const int c_waitForProcessOutputInMs = 5_000;
        protected const int c_waitForProcessToStartInMs = 1_000;
        protected static FileInfo s_dummyConsoleAppFileInfo = null!;


        [Before(HookType.TestSession)]
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
    }
}
