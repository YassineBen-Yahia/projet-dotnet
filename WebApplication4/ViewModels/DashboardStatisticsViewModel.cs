using System;
using System.Collections.Generic;

namespace WebApplication4.ViewModels
{
    public class DashboardStatisticsViewModel
    {
        // 1. Property Status Distribution
        public List<string> PropertyStatusLabels { get; set; } = new();
        public List<int> PropertyStatusData { get; set; } = new();

        // 2. Monthly Request Trend
        public List<string> RequestTrendLabels { get; set; } = new();
        public List<int> RequestTrendData { get; set; } = new();

        // 3. User Distribution by Role
        public List<string> RoleLabels { get; set; } = new();
        public List<int> RoleData { get; set; } = new();

        // 4. Top 5 Most Requested Properties
        public List<string> TopPropertyLabels { get; set; } = new();
        public List<int> TopPropertyData { get; set; } = new();

        // 5. Monthly Message Trend
        public List<string> MessageTrendLabels { get; set; } = new();
        public List<int> MessageTrendData { get; set; } = new();
    }
}
