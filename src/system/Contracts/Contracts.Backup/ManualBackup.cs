using System.Collections.Generic;

namespace Contracts.Backup
{
    public sealed record ManualBackup(
        string? Folder,
        BackupMode? Mode,
        Compression? Compression,
        IReadOnlySet<string>? Scope,
        string? NamePostfix);
}
