
namespace InteractiveStand.Application.State
{
    public class PowerDistributionState
    {
        public static bool IsSimulationRunning { get; set; }
        public static bool IsPaused { get; set; }
        public static CancellationTokenSource CancellationTokenSource { get; set; }
        public static int CurrentTick { get; set; } = 0;
        public static List<string> SimulationLogs { get; } = new List<string>();
        public static double SimulationTimeSeconds { get; set; }

    }
}
