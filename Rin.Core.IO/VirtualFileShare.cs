namespace Rin.Core.IO;

/// <summary>
///     File share capabilities, equivalent of <see cref="System.IO.FileShare" />.
/// </summary>
[Flags]
public enum VirtualFileShare : uint {
    None = 0,
    Read = 1,
    Write = 2,
    ReadWrite = 3,
    Delete = 4
}
