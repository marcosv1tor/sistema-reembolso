using System;
using System.Collections.Generic;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Reembolso.Shared.DTOs
{
    /// <summary>
    /// DTO para resultado completo de health check
    /// </summary>
    public class HealthCheckResultDto
    {
        public string ServiceName { get; set; }
        public HealthStatus Status { get; set; }
        public DateTime Timestamp { get; set; }
        public TimeSpan Duration { get; set; }
        public List<IndividualHealthCheckDto> Checks { get; set; } = new List<IndividualHealthCheckDto>();

        public override string ToString()
        {
            var healthyCount = 0;
            var degradedCount = 0;
            var unhealthyCount = 0;

            foreach (var check in Checks)
            {
                switch (check.Status)
                {
                    case HealthStatus.Healthy:
                        healthyCount++;
                        break;
                    case HealthStatus.Degraded:
                        degradedCount++;
                        break;
                    case HealthStatus.Unhealthy:
                        unhealthyCount++;
                        break;
                }
            }

            return $"Service: {ServiceName}, Status: {Status}, Duration: {Duration.TotalMilliseconds}ms, " +
                   $"Checks: {Checks.Count} (Healthy: {healthyCount}, Degraded: {degradedCount}, Unhealthy: {unhealthyCount})";
        }
    }

    /// <summary>
    /// DTO para verificação individual de health check
    /// </summary>
    public class IndividualHealthCheckDto
    {
        public string Name { get; set; }
        public HealthStatus Status { get; set; }
        public string Description { get; set; }
        public DateTime Timestamp { get; set; }
        public TimeSpan Duration { get; set; }
        public string Exception { get; set; }
        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();

        public override string ToString()
        {
            var result = $"{Name}: {Status}";
            if (!string.IsNullOrEmpty(Description))
                result += $" - {Description}";
            if (Duration != TimeSpan.Zero)
                result += $" ({Duration.TotalMilliseconds}ms)";
            return result;
        }
    }

    /// <summary>
    /// DTO para configuração de health check
    /// </summary>
    public class HealthCheckConfigDto
    {
        public bool EnableDatabaseCheck { get; set; } = true;
        public bool EnableExternalDependenciesCheck { get; set; } = true;
        public bool EnableSystemResourcesCheck { get; set; } = true;
        public bool EnableInternalServicesCheck { get; set; } = true;
        public int TimeoutSeconds { get; set; } = 30;
        public int MaxMemoryUsageMB { get; set; } = 1024;
        public double MaxCpuUsagePercentage { get; set; } = 80;
        public double MaxDiskUsagePercentage { get; set; } = 90;
        public List<string> ExternalEndpoints { get; set; } = new List<string>();
    }

    /// <summary>
    /// DTO para resumo de health check
    /// </summary>
    public class HealthCheckSummaryDto
    {
        public string ServiceName { get; set; }
        public HealthStatus OverallStatus { get; set; }
        public DateTime LastCheck { get; set; }
        public int TotalChecks { get; set; }
        public int HealthyChecks { get; set; }
        public int DegradedChecks { get; set; }
        public int UnhealthyChecks { get; set; }
        public double AverageResponseTimeMs { get; set; }
        public List<string> CriticalIssues { get; set; } = new List<string>();
    }
}