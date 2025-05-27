using System;
using System.Collections.Generic;

namespace Contracts.Backup
{
    public sealed record BackupRule(
        BackupRuleId Id,
        string Name,
        string Folder,
        BackupMode Mode,
        string Cron,
        Compression Compression,
        IReadOnlyList<string> Scope,
        int? RetainCount,
        TimeSpan? RetainAge);
}
