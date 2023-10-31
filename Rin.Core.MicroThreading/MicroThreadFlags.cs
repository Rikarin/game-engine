namespace Rin.Core.MicroThreading;

[Flags]
public enum MicroThreadFlags {
    None = 0,

    /// <summary>
    ///     If a faulted <see cref="MicroThread" /> is not being waited on, do not propagate exception outside of
    ///     <see cref="Scheduler.Run" />.
    /// </summary>
    /// <remarks>
    ///     If an exception happens in a <see cref="MicroThread" />, two things can happen.
    ///     Either something was waiting on it (i.e. with <see cref="Scheduler.WhenAll" />), in that case exception will be
    ///     propagated to waiting code.
    ///     Otherwise, exception will be rethrow outside of <see cref="Scheduler.Run" />.
    ///     This flags allows exception to be ignored even if nothing was waiting on it.
    /// </remarks>
    IgnoreExceptions = 1
}
