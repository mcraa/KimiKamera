using System.Collections.Generic;
using System.Threading.Tasks;

public class ServicesCollection : Dictionary<string, IStatusNotifier>
{
    public async Task SendStatuses(StatusEnum status)       
    {
        foreach (var srv in this)
        {
            switch (status)
            {
                case StatusEnum.Busy:
                await srv.Value.SetBusyAsync();
                break;
                case StatusEnum.Transition:
                await srv.Value.SetInTransitionAsync();
                break;
                case StatusEnum.Available:
                await srv.Value.SetAvailableAsync();
                break;
            }
        }
    } 
}