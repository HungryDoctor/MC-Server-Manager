using Contracts.Lifecycle;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Contracts.Backup
{
    public interface IBackup
    {
        Task<BackupId> RunNowAsync(ServerInstanceId serverInstanceId, BackupRuleId backupRuleId, CancellationToken ct = default);
        Task<BackupId> RunNowAsync(ServerInstanceId serverInstanceId, ManualBackup manualBackup, CancellationToken ct = default);
        Task PruneAsync(ServerInstanceId serverInstanceId, CancellationToken ct = default);
        Task<IReadOnlyList<BackupRule>> GetRulesAsync(ServerInstanceId serverInstanceId, CancellationToken ct = default);
        Task<BackupRuleId> AddRuleAsync(ServerInstanceId serverInstanceId, BackupRule backupRule, CancellationToken ct = default);
        Task DeleteRuleAsync(ServerInstanceId serverInstanceId, BackupRuleId id, CancellationToken ct = default);
    }
}
