using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Exercises.API;

public class StartupHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(HealthCheckResult.Healthy("The startup task has completed."));
    }
}