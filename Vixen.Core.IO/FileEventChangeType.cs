namespace Vixen.Core.IO;

/// <summary>
///     Change type of file used by <see cref="FileEvent" /> and <see cref="DirectoryWatcher" />.
/// </summary>
[Flags]
public enum FileEventChangeType {
    // This enum must match exactly the System.IO.WatcherChangeTypes

    Created = 1,
    Deleted = 2,
    Changed = 4,
    Renamed = 8,
    All = Renamed | Changed | Deleted | Created
}
