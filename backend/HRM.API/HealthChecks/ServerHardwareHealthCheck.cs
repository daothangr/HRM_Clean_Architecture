using System.Diagnostics;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HRM.API.HealthChecks;

public class ServerHardwareHealthCheck : IHealthCheck
{
    private const double MaxCpuUsagePercent = 90;
    private const double MaxMemoryUsagePercent = 90;
    private const double MinDiskFreePercent = 10;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var data = new Dictionary<string, object>();
        var issues = new List<string>();

        var cpuUsage = await GetCpuUsagePercentAsync(cancellationToken);
        data["cpuUsagePercent"] = Math.Round(cpuUsage, 2);
        if (cpuUsage > MaxCpuUsagePercent)
        {
            issues.Add($"CPU usage is too high ({cpuUsage:F2}%).");
        }

        var memoryInfo = GC.GetGCMemoryInfo();
        var availableMemory = memoryInfo.TotalAvailableMemoryBytes;
        var allocatedMemory = GC.GetTotalMemory(forceFullCollection: false);
        if (availableMemory > 0)
        {
            var memoryUsage = allocatedMemory * 100d / availableMemory;
            data["memoryUsagePercent"] = Math.Round(memoryUsage, 2);
            data["allocatedMemoryBytes"] = allocatedMemory;
            data["availableMemoryBytes"] = availableMemory;

            if (memoryUsage > MaxMemoryUsagePercent)
            {
                issues.Add($"Memory usage is too high ({memoryUsage:F2}%).");
            }
        }
        else
        {
            data["memoryUsagePercent"] = "unknown";
        }

        var driveRoot = Path.GetPathRoot(AppContext.BaseDirectory);
        if (!string.IsNullOrWhiteSpace(driveRoot))
        {
            var drive = new DriveInfo(driveRoot);
            if (drive.IsReady && drive.TotalSize > 0)
            {
                var diskFreePercent = drive.AvailableFreeSpace * 100d / drive.TotalSize;
                data["diskFreePercent"] = Math.Round(diskFreePercent, 2);
                data["diskName"] = drive.Name;

                if (diskFreePercent < MinDiskFreePercent)
                {
                    issues.Add($"Disk free space is too low ({diskFreePercent:F2}%).");
                }
            }
            else
            {
                data["diskFreePercent"] = "unknown";
            }
        }

        if (issues.Count > 0)
        {
            return HealthCheckResult.Unhealthy(
                description: string.Join(" ", issues),
                data: data);
        }

        return HealthCheckResult.Healthy("Server hardware is healthy.", data);
    }

    private static async Task<double> GetCpuUsagePercentAsync(CancellationToken cancellationToken)
    {
        using var process = Process.GetCurrentProcess();
        var startCpu = process.TotalProcessorTime;
        var startTime = Stopwatch.GetTimestamp();

        await Task.Delay(200, cancellationToken);

        process.Refresh();
        var endCpu = process.TotalProcessorTime;
        var endTime = Stopwatch.GetTimestamp();

        var cpuUsedMs = (endCpu - startCpu).TotalMilliseconds;
        var elapsedMs = (endTime - startTime) * 1000d / Stopwatch.Frequency;
        if (elapsedMs <= 0 || Environment.ProcessorCount <= 0)
        {
            return 0;
        }

        return cpuUsedMs / (elapsedMs * Environment.ProcessorCount) * 100d;
    }
}
