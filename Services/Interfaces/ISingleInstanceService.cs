namespace Quera.Services;

public interface ISingleInstanceService
{
    bool IsFirstInstance { get; }
    bool TryAcquire();
    void StartSignalListener(Action onShowRequested);
    void SignalExistingInstance();
}
