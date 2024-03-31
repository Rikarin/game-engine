using Rin.Core.Reflection;
using Rin.Core.Reflection.TypeDescriptors;

namespace Rin.Core.Yaml.Serialization;

/// <summary>
///     A factory of <see cref="IYamlSerializable" />
/// </summary>
[AssemblyScan]
public interface IYamlSerializableFactory {
    /// <summary>
    ///     Try to create a <see cref="IYamlSerializable" /> or return null if not supported for a particular .NET
    ///     typeDescriptor.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="typeDescriptor">The typeDescriptor.</param>
    /// <returns>If supported, return an instance of <see cref="IYamlSerializable" /> else return <c>null</c>.</returns>
    IYamlSerializable TryCreate(SerializerContext context, ITypeDescriptor typeDescriptor);
}
