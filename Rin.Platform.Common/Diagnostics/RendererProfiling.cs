using Rin.Diagnostics;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Rin.Platform.Abstractions.Diagnostics;

public static class RendererProfiling {
    public static readonly ActivitySource ApplicationSource = new("Rin.Renderer");
    public static readonly Meter ApplicationMeter = new("Rin.Renderer");
    
    public static readonly Histogram<double> WorkTime = ApplicationMeter.CreateHistogram<double>("WorkTime");
    public static readonly Histogram<double> WaitTime = ApplicationMeter.CreateHistogram<double>("WaitTime");

    public static readonly Histogram<int> SubmitCount = ApplicationMeter.CreateHistogram<int>("SubmitCount");
    // public static readonly UpDownCounter<int> SubmitCount = ApplicationMeter.CreateUpDownCounter<int>("SubmitCount");
    public static readonly Histogram<int> SubmitDisposalCount = ApplicationMeter.CreateHistogram<int>("SubmitDisposalCount");

    public static ProfileScope StartWorkTime() => new(WorkTime, ApplicationSource.StartActivity("Work"));
    public static ProfileScope StartWaitTime() => new(WaitTime, ApplicationSource.StartActivity("Wait"));
}
