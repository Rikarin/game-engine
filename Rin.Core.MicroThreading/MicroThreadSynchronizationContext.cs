namespace Rin.Core.MicroThreading;

public class MicroThreadSynchronizationContext : SynchronizationContext, IMicroThreadSynchronizationContext {
    readonly MicroThread microThread;

    MicroThread IMicroThreadSynchronizationContext.MicroThread => microThread;

    public MicroThreadSynchronizationContext(MicroThread microThread) {
        this.microThread = microThread;
    }

    public override SynchronizationContext CreateCopy() => this;

    public override void Post(SendOrPostCallback d, object state) {
        // There is two case:
        // 1/ We are either in normal MicroThread inside Scheduler.Step() (CurrentThread test),
        // in which case we will directly execute the callback to avoid further processing from scheduler.
        // Also, note that Wait() sends us event that are supposed to come back into scheduler.
        // Note: As it will end up on the callstack, it might be better to Schedule it instead (to avoid overflow)?
        // 2/ Otherwise, we just received an external task continuation (i.e. Task.Sleep()), or a microthread triggering another,
        // so schedule it so that it comes back in our regular scheduler.
        if (microThread.Scheduler.RunningMicroThread == microThread) {
            d(state);
        } else if (microThread.State == MicroThreadState.Completed) {
            throw new InvalidOperationException("MicroThread is already completed but still posting continuations.");
        } else {
            microThread.ScheduleContinuation(microThread.ScheduleMode, d, state);
        }
    }
}
