﻿using Contracts.Configuration;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Configuration.Repository
{
    public interface IConfigurationRepository
    {
        Task<GlobalSettings?> GetAsync(CancellationToken ct = default);
        Task CreateOrUpdateAsync(GlobalSettings settings, CancellationToken ct = default);
    }
}
