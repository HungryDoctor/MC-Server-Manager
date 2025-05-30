using Contracts.Lifecycle;
using Services.Lifecycle.Repository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TestsBase;

namespace LifecycleTests
{
    public class RavenDbServerStateRepositoryTests : RavenDbTestBase
    {
        private RavenDbServerStateRepository m_ravenDbServerStateRepository;


        [Before(HookType.Test)]
        public void InitializeTest()
        {
            m_ravenDbServerStateRepository = new RavenDbServerStateRepository(m_documentStore);
        }


        [Test]
        public async Task CreateAsync_InsertsAndReturns_Id_Async()
        {
            ServerState serverState = GetServerState();
            ServerState anotherServerState = GetAnotherServerState();

            ServerInstanceId instanceId = await m_ravenDbServerStateRepository.CreateAsync(serverState);
            ServerInstanceId anotherInstanceId = await m_ravenDbServerStateRepository.CreateAsync(anotherServerState);

            await Assert.That(instanceId).IsEqualTo(serverState.ServerInstanceId);
            await Assert.That(anotherInstanceId).IsEqualTo(anotherServerState.ServerInstanceId);
        }

        [Test]
        public async Task GetAsync_Returns_Saved_Async()
        {
            ServerState serverState = GetServerState();
            ServerState anotherServerState = GetAnotherServerState();

            ServerInstanceId instanceId = await m_ravenDbServerStateRepository.CreateAsync(serverState);
            ServerInstanceId anotherInstanceId = await m_ravenDbServerStateRepository.CreateAsync(anotherServerState);

            ServerState? roundTrip = await m_ravenDbServerStateRepository.GetAsync(instanceId);
            ServerState? anotherRoundTrip = await m_ravenDbServerStateRepository.GetAsync(anotherInstanceId);

            await Assert.That(roundTrip).IsEqualTo(serverState);
            await Assert.That(anotherRoundTrip).IsEqualTo(anotherServerState);
        }

        [Test]
        public async Task GetAsync_Returns_Null_When_NotFound_Async()
        {
            ServerState? missing = await m_ravenDbServerStateRepository.GetAsync(new ServerInstanceId(Guid.NewGuid()));

            await Assert.That(missing).IsNull();
        }

        [Test]
        public async Task GetAsync_Returns_AllSavedStates_Async()
        {
            ServerState serverState = GetServerState();
            ServerState anotherServerState = GetAnotherServerState();

            await m_ravenDbServerStateRepository.UpdateAsync(serverState);
            await m_ravenDbServerStateRepository.UpdateAsync(anotherServerState);

            IReadOnlySet<ServerState> allStates = await m_ravenDbServerStateRepository.GetAsync();

            await Assert.That(allStates).HasCount(2);
            await Assert.That(allStates).Contains(serverState);
            await Assert.That(allStates).Contains(anotherServerState);
        }

        [Test]
        public async Task GetAsync_Returns_EmptyCollection_When_NotSaved_Async()
        {
            IReadOnlySet<ServerState> collection = await m_ravenDbServerStateRepository.GetAsync();

            await Assert.That(collection).HasCount(0);
        }

        [Test]
        public async Task UpdateAsync_Overwrites_WhenKeyExists_Async()
        {
            ServerState serverState = GetServerState();

            await m_ravenDbServerStateRepository.UpdateAsync(serverState);
            ServerState updated = GetServerState() with { CrashCountLastWindow = 99 };

            await m_ravenDbServerStateRepository.UpdateAsync(updated);
            ServerState? roundTrip = await m_ravenDbServerStateRepository.GetAsync(serverState.ServerInstanceId);

            await Assert.That(roundTrip).IsEqualTo(updated);
        }

        [Test]
        public async Task DeleteAsync_Deletes_Async()
        {
            ServerState serverState = GetServerState();
            ServerInstanceId instanceId = await m_ravenDbServerStateRepository.CreateAsync(serverState);

            await m_ravenDbServerStateRepository.DeleteAsync(instanceId);
            ServerState? roundTrip = await m_ravenDbServerStateRepository.GetAsync(instanceId);

            await Assert.That(roundTrip).IsNull();
        }


        private static ServerState GetServerState() => new ServerState(
            new ServerInstanceId(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")),
            ServerStatus.Running,
            123,
            new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            null,
            0);

        private static ServerState GetAnotherServerState() => new ServerState(
            new ServerInstanceId(Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb")),
            ServerStatus.Stopped,
            456,
            new DateTime(2022, 1, 1, 10, 0, 0, DateTimeKind.Utc).AddMinutes(-10),
            new DateTime(2022, 1, 1, 10, 0, 0, DateTimeKind.Utc),
            2);
    }
}
