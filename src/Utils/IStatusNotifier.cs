using System;
using System.Threading.Tasks;

public interface IStatusNotifier : IDisposable {
    Task SetBusyAsync();
    Task SetInTransitionAsync();
    Task SetAvailableAsync();
    Task<Boolean> IsConnected();

}