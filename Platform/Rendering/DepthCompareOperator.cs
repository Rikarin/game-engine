namespace Rin.Platform.Rendering;

public enum DepthCompareOperator {
    None = 0, // TODO: remove none?
    Never,
    NotEqual,
    Less,
    LessOrEqual,
    Greater,
    GreaterOrEqual,
    Equal,
    Always
}
