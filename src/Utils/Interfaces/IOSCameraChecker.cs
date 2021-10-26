using System.Threading.Tasks;

public interface IOSCameraChecker {
    Task<StatusEnum> PollCamerasAsync();
}