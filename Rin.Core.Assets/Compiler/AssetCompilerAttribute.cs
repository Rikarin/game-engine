namespace Rin.Core.Assets.Compiler;

/// <summary>
///     Attribute to define an asset compiler for a <see cref="Asset" />.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
[BaseTypeRequired(typeof(IAssetCompiler))]
public class AssetCompilerAttribute : DynamicTypeAttributeBase {
    public Type CompilationContext { get; private set; }

    public AssetCompilerAttribute(Type type, Type compilationContextType) : base(type) {
        CompilationContext = compilationContextType;
    }

    public AssetCompilerAttribute(string typeName, Type compilationContextType) : base(typeName) {
        CompilationContext = compilationContextType;
    }
}
