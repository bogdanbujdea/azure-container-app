using Microsoft.Extensions.Diagnostics.HealthChecks;

public class ReadyHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(HealthCheckResult.Healthy("The app is ready."));
    }
}