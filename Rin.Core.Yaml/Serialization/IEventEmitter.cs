using Rin.Core.Yaml.Events;

namespace Rin.Core.Yaml.Serialization;

/// <summary>
///     Interface used to write YAML events.
/// </summary>
public interface IEventEmitter {
    void StreamStart();
    void DocumentStart();
    void Emit(AliasEventInfo eventInfo);
    void Emit(ScalarEventInfo eventInfo);
    void Emit(MappingStartEventInfo eventInfo);
    void Emit(MappingEndEventInfo eventInfo);
    void Emit(SequenceStartEventInfo eventInfo);
    void Emit(SequenceEndEventInfo eventInfo);
    void Emit(ParsingEvent parsingEvent);
    void DocumentEnd();
    void StreamEnd();
}