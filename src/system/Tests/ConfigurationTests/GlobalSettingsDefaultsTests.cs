using Quartz;
using Services.Configuration.Defaults;
using System;
using System.Buffers;
using System.IO;
using System.Threading.Tasks;

namespace ConfigurationTests
{
    public class GlobalSettingsDefaultsTests
    {
        private static readonly SearchValues<char> s_windowsForbiddenFodlerChars = SearchValues.Create('<', '>', ':', '"', '\\', '|', '?', '*');
        private static readonly SearchValues<char> s_shellForbiddenFolerChars = SearchValues.Create(';', '|', '&', '>', '<', '\'', '"');


        [Test]
        public void JdkScanCron_Valid()
        {
            CronExpression.ValidateExpression(GlobalSettingsDefaults.JdkScanCron);
        }

        [Test]
        public void ModLoaderScanCron_Valid()
        {
            CronExpression.ValidateExpression(GlobalSettingsDefaults.ModLoaderScanCron);
        }

        [Test]
        public async Task CrashLoopWindow_GreaterThan_30sec_Async()
        {
            await Assert.That(GlobalSettingsDefaults.CrashLoopWindow).IsGreaterThan(TimeSpan.FromSeconds(30));
        }

        [Test]
        public async Task CrashLoopCount_GreaterThan_1_Async()
        {
            await Assert.That(GlobalSettingsDefaults.CrashLoopCount).IsGreaterThan(1u);
        }

        [Test]
        public async Task LogTailKBDefault_GreaterThan_512kb_Async()
        {
            await Assert.That(GlobalSettingsDefaults.LogTailKBDefault).IsGreaterThan(512u);
        }

        [Test]
        public Task DefaultBackupRoot_Contains_ValidSymbols_Async()
        {
            return PathContainsValidSymbolsAsync(GlobalSettingsDefaults.DefaultBackupRoot);
        }

        [Test]
        public Task DefaultJdksRoot_Contains_ValidSymbols_Async()
        {
            return PathContainsValidSymbolsAsync(GlobalSettingsDefaults.DefaultJdksRoot);
        }

        [Test]
        public Task DefaultServersRoot_Contains_ValidSymbols_Async()
        {
            return PathContainsValidSymbolsAsync(GlobalSettingsDefaults.DefaultServersRoot);
        }


        private static async Task PathContainsValidSymbolsAsync(string path)
        {
            await Assert.That(Path.IsPathRooted(path)).IsTrue();
            await Assert.That(path.Contains("..")).IsFalse();

            await Assert.That(path.AsSpan().IndexOfAny(s_windowsForbiddenFodlerChars)).IsEqualTo(-1);
            await Assert.That(path.AsSpan().IndexOfAny(s_shellForbiddenFolerChars)).IsEqualTo(-1);
            await Assert.That(path.IndexOfAny(Path.GetInvalidPathChars())).IsEqualTo(-1);
        }
    }
}
