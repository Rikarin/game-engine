namespace Vixen.Core.Yaml.Events;

/// <summary>
///     Represents a stream end event.
/// </summary>
public class StreamEnd : ParsingEvent {
    /// <summary>
    ///     Gets a value indicating the variation of depth caused by this event.
    ///     The value can be either -1, 0 or 1. For start events, it will be 1,
    ///     for end events, it will be -1, and for the remaining events, it will be 0.
    /// </summary>
    public override int NestingIncrease => -1;

    /// <summary>
    ///     Gets the event type, which allows for simpler type comparisons.
    /// </summary>
    internal override EventType Type => EventType.YamlStreamEndEvent;

    /// <summary>
    ///     Initializes a new instance of the <see cref="StreamEnd" /> class.
    /// </summary>
    /// <param name="start">The start position of the event.</param>
    /// <param name="end">The end position of the event.</param>
    public StreamEnd(Mark start, Mark end) : base(start, end) { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="StreamEnd" /> class.
    /// </summary>
    public StreamEnd() : this(Mark.Empty, Mark.Empty) { }

    /// <summary>
    ///     Returns a <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />.
    /// </summary>
    /// <returns>
    ///     A <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />.
    /// </returns>
    public override string ToString() => "Stream end";
}
