namespace Quera.Services;

public class SingleInstanceService : ISingleInstanceService, IDisposable
{
    private const string MutexName = "Quera-Launcher-{E8CF9D05-C7BC-4EE8-AD90-572864EB65B2}";
    private const string SignalName = "Quera-Show-{E8CF9D05-C7BC-4EE8-AD90-572864EB65B2}";

    private Mutex? _mutex;
    private EventWaitHandle? _showSignal;
    private CancellationTokenSource? _signalCts;

    public bool IsFirstInstance { get; private set; }

    public bool TryAcquire()
    {
        _mutex = new Mutex(true, MutexName, out bool createdNew);
        IsFirstInstance = createdNew;
        return createdNew;
    }

    public void StartSignalListener(Action onShowRequested)
    {
        _signalCts = new CancellationTokenSource();
        _showSignal = new EventWaitHandle(false, EventResetMode.AutoReset, SignalName);

        Task.Run(() =>
        {
            while (!_signalCts.Token.IsCancellationRequested)
            {
                try
                {
                    var signaled = WaitHandle.WaitAny(new WaitHandle[] { _showSignal, _signalCts.Token.WaitHandle });
                    if (_signalCts.Token.IsCancellationRequested) break;

                    Application.Current.Dispatcher.Invoke(onShowRequested);
                }
                catch (ObjectDisposedException) { break; }
            }
        }, _signalCts.Token);
    }

    public void SignalExistingInstance()
    {
        try
        {
            using var evt = EventWaitHandle.OpenExisting(SignalName);
            evt.Set();
        }
        catch (WaitHandleCannotBeOpenedException)
        {
            // First instance hasn't created the signal yet
        }
    }

    public void Dispose()
    {
        _signalCts?.Cancel();
        _signalCts?.Dispose();
        _showSignal?.Dispose();
        _mutex?.Dispose();
    }
}
