using Contracts.Lifecycle;
using LiteDB;
using Services.Lifecycle.Repository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace LifecycleTests
{
    public class LiteDbServerProcessRepositoryTests
    {
        private MemoryStream m_dbMemoryStream;
        private LiteDatabase m_database;
        private LiteDbServerProcessRepository m_liteDbServerProcessRepository;


        [Before(HookType.Test)]
        public void InitializeTest()
        {
            m_dbMemoryStream = new MemoryStream();
            m_database = new LiteDatabase(m_dbMemoryStream);
            m_liteDbServerProcessRepository = new LiteDbServerProcessRepository(m_database);
        }

        [After(HookType.Test)]
        public void CleanupTest()
        {
            m_database.Dispose();
            m_dbMemoryStream.Dispose();
        }


        [Test]
        public async Task CreateAsync_InsertsAndReturns_Id_Async()
        {
            ServerState serverState = GetServerState();
            ServerState anotherServerState = GetAnotherServerState();

            ServerInstanceId instanceId = await m_liteDbServerProcessRepository.CreateAsync(serverState);
            ServerInstanceId anotherInstanceId = await m_liteDbServerProcessRepository.CreateAsync(anotherServerState);

            await Assert.That(instanceId).IsEqualTo(serverState.ServerInstanceId);
            await Assert.That(anotherInstanceId).IsEqualTo(anotherServerState.ServerInstanceId);
        }

        [Test]
        public async Task GetAsync_Returns_WhatSaved_Async()
        {
            ServerState serverState = GetServerState();
            ServerState anotherServerState = GetAnotherServerState();

            ServerInstanceId instanceId = await m_liteDbServerProcessRepository.CreateAsync(serverState);
            ServerInstanceId anotherInstanceId = await m_liteDbServerProcessRepository.CreateAsync(anotherServerState);

            ServerState? roundTrip = await m_liteDbServerProcessRepository.GetAsync(instanceId);
            ServerState? anotherRoundTrip = await m_liteDbServerProcessRepository.GetAsync(anotherInstanceId);

            await Assert.That(roundTrip).IsEqualTo(serverState);
            await Assert.That(anotherRoundTrip).IsEqualTo(anotherServerState);
        }

        [Test]
        public async Task GetAsync_Returns_AllSavedStates_Async()
        {
            ServerState serverState = GetServerState();
            ServerState anotherServerState = GetAnotherServerState();

            await m_liteDbServerProcessRepository.UpdateAsync(serverState);
            await m_liteDbServerProcessRepository.UpdateAsync(anotherServerState);

            IReadOnlySet<ServerState> allStates = await m_liteDbServerProcessRepository.GetAsync();

            await Assert.That(allStates.Count).IsEqualTo(2);
            await Assert.That(allStates.Contains(serverState)).IsTrue();
            await Assert.That(allStates.Contains(anotherServerState)).IsTrue();
        }

        [Test]
        public async Task UpdateAsync_Overwrites_WhenKeyExists_Async()
        {
            ServerState serverState = GetServerState();

            await m_liteDbServerProcessRepository.UpdateAsync(serverState);
            ServerState updated = GetServerState() with { CrashCountLastWindow = 99 };

            await m_liteDbServerProcessRepository.UpdateAsync(updated);
            ServerState? roundTrip = await m_liteDbServerProcessRepository.GetAsync(serverState.ServerInstanceId);

            await Assert.That(roundTrip).IsEqualTo(updated);
        }

        [Test]
        public async Task GetAsync_Returns_NullWhenNotFound_Async()
        {
            ServerState? missing = await m_liteDbServerProcessRepository.GetAsync(new ServerInstanceId(Guid.NewGuid()));

            await Assert.That(missing).IsNull();
        }


        private static ServerState GetServerState() => new ServerState(
            new ServerInstanceId(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")),
            ServerStatus.Running,
            123,
            new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.FromHours(12)),
            null,
            0);

        private static ServerState GetAnotherServerState() => new ServerState(
            new ServerInstanceId(Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb")),
            ServerStatus.Stopped,
            456,
            new DateTimeOffset(2022, 1, 1, 10, 0, 0, TimeSpan.FromHours(3)).AddMinutes(-10),
            new DateTimeOffset(2022, 1, 1, 10, 0, 0, TimeSpan.FromHours(3)),
            2);
    }
}
