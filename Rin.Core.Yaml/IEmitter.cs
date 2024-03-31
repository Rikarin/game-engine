﻿using Rin.Core.Yaml.Events;

namespace Rin.Core.Yaml;

/// <summary>
///     Represents a YAML stream emitter.
/// </summary>
public interface IEmitter {
    /// <summary>
    ///     Emits an event.
    /// </summary>
    void Emit(ParsingEvent @event);
}
