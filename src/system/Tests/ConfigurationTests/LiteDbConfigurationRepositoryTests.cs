using Contracts.Configuration;
using LiteDB;
using Moq;
using Services.Configuration.Defaults;
using Services.Configuration.Repository;
using System;
using System.Threading.Tasks;

namespace ConfigurationTests
{
    public class LiteDbConfigurationRepositoryTests
    {
        private GlobalSettings? m_stored;

        private Mock<ILiteDatabase> m_liteDatabaseMoq;
        private Mock<ILiteCollection<GlobalSettings>> m_globalSettingsCollection;
        private LiteDbConfigurationRepository m_liteDbConfigurationRepository;


        [Before(HookType.Test)]
        public void InitializeTest()
        {
            m_globalSettingsCollection = new Mock<ILiteCollection<GlobalSettings>>(MockBehavior.Strict);
            m_globalSettingsCollection
                .Setup(x => x.FindById(1))
                .Returns(() => m_stored);
            m_globalSettingsCollection
                .Setup(c => c.Upsert(1, It.IsAny<GlobalSettings>()))
                .Callback<BsonValue, GlobalSettings>((_, gs) => m_stored = gs)
                .Returns(true);

            m_liteDatabaseMoq = new Mock<ILiteDatabase>(MockBehavior.Strict);
            m_liteDatabaseMoq.Setup(x => x.GetCollection<GlobalSettings>(It.IsAny<string>(), It.IsAny<BsonAutoId>())).Returns(m_globalSettingsCollection.Object);
            m_liteDatabaseMoq.Setup(d => d.Checkpoint());

            m_liteDbConfigurationRepository = new LiteDbConfigurationRepository(m_liteDatabaseMoq.Object);
        }


        [Test]
        public async Task GetAsync_Before_Save_Returns_Defaults()
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
        public async Task GetAsync_Returns_Most_Recent_Settings()
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
