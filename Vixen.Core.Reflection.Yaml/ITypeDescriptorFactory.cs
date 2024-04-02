using Vixen.Core.Reflection.TypeDescriptors;

namespace Vixen.Core.Reflection;

/// <summary>
///     A factory to create an instance of a <see cref="ITypeDescriptor" />
/// </summary>
public interface ITypeDescriptorFactory {
    /// <summary>
    ///     Gets the attribute registry used by this factory.
    /// </summary>
    /// <value>The attribute registry.</value>
    IAttributeRegistry AttributeRegistry { get; }

    /// <summary>
    ///     Tries to create an instance of a <see cref="ITypeDescriptor" /> from the type. Return null if this factory is not
    ///     handling this type.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>ITypeDescriptor.</returns>
    ITypeDescriptor? Find(Type? type);
}
