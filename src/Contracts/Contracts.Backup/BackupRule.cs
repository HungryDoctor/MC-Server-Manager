using System;
using System.Collections.Generic;
using System.Linq;

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
        TimeSpan? RetainAge) : IEquatable<BackupRule>
    {
        public bool Equals(BackupRule? other) =>
            other is not null &&
            Id == other.Id &&
            Name == other.Name &&
            Folder == other.Folder &&
            Mode == other.Mode &&
            Cron == other.Cron &&
            Compression == other.Compression &&
            RetainCount == other.RetainCount &&
            RetainAge == other.RetainAge &&
            Scope.SequenceEqual(other.Scope);

        public override int GetHashCode()
        {
            HashCode hashCode = new HashCode();
            hashCode.Add(Id);
            hashCode.Add(Name);
            hashCode.Add(Folder);
            hashCode.Add(Mode);
            hashCode.Add(Cron);
            hashCode.Add(Compression);
            hashCode.Add(RetainCount);
            hashCode.Add(RetainAge);

            foreach (string s in Scope)
            {
                hashCode.Add(s);
            }

            return hashCode.ToHashCode();
        }
    }
}
