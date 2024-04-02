using Vixen.Core.Common.Collections;

namespace Vixen.InputSystem;

/// <summary>
///     An abstraction for a platform specific mechanism that provides input in the form of one of multiple
///     <see cref="IInputDevice" />(s).
///     An input source is responsible for cleaning up it's own devices at cleanup
/// </summary>
public interface IInputSource : IDisposable {
    /// <summary>
    ///     All the input devices currently provided by this source
    /// </summary>
    TrackingDictionary<Guid, IInputDevice> Devices { get; }

    /// <summary>
    ///     Initializes the input source
    /// </summary>
    /// <param name="inputManager">The <see cref="InputManager" /> initializing this source</param>
    void Initialize(InputManager inputManager);

    /// <summary>
    ///     Allows the source to take it's time to search for new devices
    /// </summary>
    void Scan();

    /// <summary>
    ///     Update the input source and possibly add/remove input devices
    /// </summary>
    void Update();

    /// <summary>
    ///     Called when input should be paused, for example when the application leaves the foreground
    /// </summary>
    void Pause();

    /// <summary>
    ///     Called when input should be resumed, when an application enters the forground
    /// </summary>
    void Resume();
}
