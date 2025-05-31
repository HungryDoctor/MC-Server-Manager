using Contracts.Configuration;
using Services.Configuration.Repository;
using System;
using System.Threading.Tasks;
using TestsBase;

namespace ConfigurationTests
{
    public class RavenDbConfigurationRepositoryTests : RavenDbTestBase
    {
        private RavenDbConfigurationRepository m_ravenDbConfigurationRepository = null!;


        [Before(HookType.Test)]
        public void InitializeTest()
        {
            m_ravenDbConfigurationRepository = new RavenDbConfigurationRepository(m_documentStore);
        }


        [Test]
        public async Task GetAsync_Before_Save_Returns_Null_Async()
        {
            GlobalSettings? firstRead = await m_ravenDbConfigurationRepository.GetAsync();

            await Assert.That(firstRead).IsEqualTo(null);
        }

        [Test]
        public async Task GetAsync_Returns_Saved_Async()
        {
            GlobalSettings settings = GetTestSettings();
            await m_ravenDbConfigurationRepository.CreateOrUpdateAsync(settings);
            GlobalSettings? roundTrip = await m_ravenDbConfigurationRepository.GetAsync();

            await Assert.That(roundTrip).IsEqualTo(settings);
        }

        [Test]
        public async Task GetAsync_Returns_MostRecentSettings_Async()
        {
            GlobalSettings settings = GetTestSettings();
            GlobalSettings anotherSettings = GetAnotherTestSettings();

            await m_ravenDbConfigurationRepository.CreateOrUpdateAsync(settings);
            await m_ravenDbConfigurationRepository.CreateOrUpdateAsync(anotherSettings);
            GlobalSettings? roundTrip = await m_ravenDbConfigurationRepository.GetAsync();

            await Assert.That(roundTrip).IsEqualTo(anotherSettings);
        }


        private static GlobalSettings GetTestSettings() =>
            new GlobalSettings(
                nameof(GlobalSettings.DefaultBackupRoot),
                nameof(GlobalSettings.DefaultServersRoot),
                nameof(GlobalSettings.DefaultJdksRoot),
                1024,
                [new TurnOffMessage(TimeSpan.FromSeconds(20), "Bye-bye")],
                5,
                TimeSpan.FromMinutes(12),
                "0 0 7 7 * ?",
                "0 0 1 1 * ?");

        private static GlobalSettings GetAnotherTestSettings() =>
            new GlobalSettings(
                "Another" + nameof(GlobalSettings.DefaultBackupRoot),
                "Another" + nameof(GlobalSettings.DefaultServersRoot),
                "Another" + nameof(GlobalSettings.DefaultJdksRoot),
                4096,
                [new TurnOffMessage(TimeSpan.FromSeconds(20), "Bye-bye"), new TurnOffMessage(TimeSpan.FromSeconds(10), "Still here?")],
                3,
                TimeSpan.FromMinutes(5),
                "0 0 1 1 * ?",
                "0 0 1 1 * ?");
    }
}
