using NexFit.Models;

namespace NexFit.Models.ViewModels
{
    public class ProgressViewModel
    {
        // =========================
        // CURRENT USER DATA
        // =========================

        public ApplicationUser? User { get; set; }

        // =========================
        // DAILY HISTORY
        // =========================

        public List<DailyVitals> VitalsHistory
        { get; set; } = new();
    }
}
