using Contracts.Configuration;
using Moq;
using Services.Configuration.Defaults;
using Services.Configuration.Repository;
using Services.Configuration.Service;
using System.Threading;
using System.Threading.Tasks;

namespace ConfigurationTests
{
    public class ConfigurationServiceTests
    {
        private GlobalSettings? m_stored;

        private Mock<IConfigurationRepository> m_testRepository;
        private ConfigurationService m_configurationService;


        [Before(HookType.Test)]
        public void InitializeTest()
        {
            m_testRepository = new Mock<IConfigurationRepository>(MockBehavior.Strict);
            m_testRepository
                .Setup(x => x.SaveAsync(It.IsAny<GlobalSettings>(), It.IsAny<CancellationToken>()))
                .Callback<GlobalSettings, CancellationToken>((settings, _) => m_stored = settings)
                .Returns(Task.CompletedTask);
            m_testRepository
                .Setup(x => x.GetAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => m_stored ?? GlobalSettingsDefaults.Instance);

            m_configurationService = new ConfigurationService(m_testRepository.Object);
        }


        [Test]
        public async Task GetAsync_ReturnsDefault_If_NothingSaved_Async()
        {
            GlobalSettings result = await m_configurationService.GetAsync();
            await Assert.That(result).IsEqualTo(GlobalSettingsDefaults.Instance);
        }

        [Test]
        public async Task GetAsync_Delegates_To_Repository_Async()
        {
            GlobalSettings result = await m_configurationService.GetAsync();
            m_testRepository.Verify(x => x.GetAsync(It.IsAny<CancellationToken>()), Times.Exactly(1));
        }

        [Test]
        public async Task SaveAsync_Delegates_To_Repository_Async()
        {
            GlobalSettings changedSettings = GlobalSettingsDefaults.Instance with
            {
                DefaultBackupRoot = "TEST"
            };

            await m_configurationService.SaveAsync(changedSettings);
            await Assert.That(changedSettings).IsEqualTo(m_stored);
            m_testRepository.Verify(x => x.SaveAsync(changedSettings, It.IsAny<CancellationToken>()), Times.Exactly(1));
        }
    }
}
