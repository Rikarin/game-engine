using System.Globalization;

namespace Rin.Core.Yaml.Events;

/// <summary>
///     Represents an alias event.
/// </summary>
public class AnchorAlias : ParsingEvent {
    /// <summary>
    ///     Gets a value indicating the variation of depth caused by this event.
    ///     The value can be either -1, 0 or 1. For start events, it will be 1,
    ///     for end events, it will be -1, and for the remaining events, it will be 0.
    /// </summary>
    public override int NestingIncrease => 0;

    /// <summary>
    ///     Gets the value of the alias.
    /// </summary>
    public string Value { get; }

    /// <summary>
    ///     Gets the event type, which allows for simpler type comparisons.
    /// </summary>
    internal override EventType Type => EventType.YamlAliasEvent;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AnchorAlias" /> class.
    /// </summary>
    /// <param name="value">The value of the alias.</param>
    /// <param name="start">The start position of the event.</param>
    /// <param name="end">The end position of the event.</param>
    public AnchorAlias(string value, Mark start, Mark end) : base(start, end) {
        if (string.IsNullOrEmpty(value)) {
            throw new YamlException(start, end, "Anchor value must not be empty.");
        }

        if (!NodeEvent.anchorValidator.IsMatch(value)) {
            throw new YamlException(start, end, "Anchor value must contain alphanumerical characters only.");
        }

        Value = value;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="AnchorAlias" /> class.
    /// </summary>
    /// <param name="value">The value of the alias.</param>
    public AnchorAlias(string value) : this(value, Mark.Empty, Mark.Empty) { }

    /// <summary>
    ///     Returns a <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />.
    /// </summary>
    /// <returns>
    ///     A <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />.
    /// </returns>
    public override string ToString() => string.Format(CultureInfo.InvariantCulture, "Alias [value = {0}]", Value);
}
