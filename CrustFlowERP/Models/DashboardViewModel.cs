namespace CrustFlowERP.Models
{
    public class DashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalRoles { get; set; }
        public int LockedUsers { get; set; }
        public int NewUsersToday { get; set; }
        public List<SystemActivity> RecentActivities { get; set; } = new List<SystemActivity>();
    }

    public class SystemActivity
    {
        public string User { get; set; } = "";
        public string Action { get; set; } = "";
        public DateTime Timestamp { get; set; }
        public string Status { get; set; } = "";
        public string BadgeClass { get; set; } = "";
    }
}
