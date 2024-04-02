namespace Vixen.InputSystem;

/// <summary>
///     This interface is used for interacting with game controller devices.
/// </summary>
public interface IGameControllerDevice : IInputDevice {
    /// <summary>
    ///     Product Id of the device
    /// </summary>
    Guid ProductId { get; }

    /// <summary>
    ///     Information about the buttons on this game controller
    /// </summary>
    IReadOnlyList<GameControllerButtonInfo> ButtonInfos { get; }

    /// <summary>
    ///     Information about the axes on this game controller
    /// </summary>
    IReadOnlyList<GameControllerAxisInfo> AxisInfos { get; }

    /// <summary>
    ///     Information about the direction controllers on this game controller
    /// </summary>
    IReadOnlyList<GameControllerDirectionInfo> DirectionInfos { get; }

    /// <summary>
    ///     The buttons that have been pressed since the last frame
    /// </summary>
    IReadOnlySet<int> PressedButtons { get; }

    /// <summary>
    ///     The buttons that have been released since the last frame
    /// </summary>
    IReadOnlySet<int> ReleasedButtons { get; }

    /// <summary>
    ///     The buttons that are down
    /// </summary>
    IReadOnlySet<int> DownButtons { get; }

    /// <summary>
    ///     Retrieves the state of a single axis
    /// </summary>
    /// <param name="index">The axis' index, as exposed in <see cref="AxisInfos" /></param>
    /// <returns>The value read directly from the axis</returns>
    float GetAxis(int index);

    /// <summary>
    ///     Retrieves the state of a single point of direction controller
    /// </summary>
    /// <param name="index">The direction controller's index, as exposed in <see cref="DirectionInfos" /></param>
    /// <returns>The current state of the direction controller</returns>
    Direction GetDirection(int index);
}
