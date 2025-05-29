using System;
using System.Collections.Generic;
using System.Linq;

namespace Contracts.Backup
{
    public sealed record ManualBackup(
        string? Folder,
        BackupMode? Mode,
        Compression? Compression,
        IReadOnlySet<string>? Scope,
        string? NamePostfix) : IEquatable<ManualBackup>
    {
        public bool Equals(ManualBackup? other) =>
            other is not null &&
            Folder == other.Folder &&
            Mode == other.Mode &&
            Compression == other.Compression &&
            NamePostfix == other.NamePostfix &&
            SetEquals(Scope, other.Scope);

        static bool SetEquals(IReadOnlySet<string>? a, IReadOnlySet<string>? b) =>
            ReferenceEquals(a, b) ||
            a is null || b is null ? false : a.SetEquals(b);


        public override int GetHashCode()
        {
            HashCode HashCode = new HashCode();
            HashCode.Add(Folder);
            HashCode.Add(Mode);
            HashCode.Add(Compression);
            HashCode.Add(NamePostfix);

            if (Scope is not null)
            {
                foreach (var item in Scope.OrderBy(s => s, StringComparer.Ordinal))
                {
                    HashCode.Add(item);
                }
            }

            return HashCode.ToHashCode();
        }
    }
}
