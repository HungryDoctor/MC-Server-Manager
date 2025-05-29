using Contracts.Configuration;
using LiteDB;
using Services.Configuration.Defaults;
using Services.Configuration.Repository;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ConfigurationTests
{
    public class LiteDbConfigurationRepositoryTests
    {
        private MemoryStream m_dbMemoryStream;
        private LiteDatabase m_database;
        private LiteDbConfigurationRepository m_liteDbConfigurationRepository;


        [Before(HookType.Test)]
        public void InitializeTest()
        {
            m_dbMemoryStream = new MemoryStream();
            m_database = new LiteDatabase(m_dbMemoryStream);
            m_liteDbConfigurationRepository = new LiteDbConfigurationRepository(m_database);
        }

        [After(HookType.Test)]
        public void CleanupTest()
        {
            m_database.Dispose();
            m_dbMemoryStream.Dispose();
        }


        [Test]
        public async Task GetAsync_Before_Save_Returns_Defaults_Async()
        {
            GlobalSettings firstRead = await m_liteDbConfigurationRepository.GetAsync();

            await Assert.That(firstRead).IsEqualTo(GlobalSettingsDefaults.Instance);
        }

        [Test]
        public async Task LiteDbConfigurationRepository_GetWhatSaved_Async()
        {
            GlobalSettings settings = GetTestSettings();
            await m_liteDbConfigurationRepository.SaveAsync(settings);
            GlobalSettings roundTrip = await m_liteDbConfigurationRepository.GetAsync();

            await Assert.That(roundTrip).IsEqualTo(settings);
        }

        [Test]
        public async Task GetAsync_Returns_Most_Recent_Settings_Async()
        {
            GlobalSettings settings = GetTestSettings();
            GlobalSettings anotherSettings = GetAnotherTestSettings();

            await m_liteDbConfigurationRepository.SaveAsync(settings);
            await m_liteDbConfigurationRepository.SaveAsync(anotherSettings);
            GlobalSettings roundTrip = await m_liteDbConfigurationRepository.GetAsync();

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
