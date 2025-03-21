using MCStatus.Models;
using MCStatus.Utilities;
using Microsoft.Extensions.Caching.Memory;

namespace MCStatus.Services;

public class StatusQueryService(IMemoryCache cache)
{
    public async Task<Status?> RequestStatusAsync(Server server, CancellationToken cancellationToken = default)
    {
        if (!cache.TryGetValue(server, out Status? status) || status is null)
        {
            status = await Pinger.RequestStatusAsync(server, cancellationToken);

            if (status is not null)
            {
                var cacheEntry = cache.CreateEntry(server);
                cacheEntry.SetValue(status);
                cacheEntry.SetAbsoluteExpiration(TimeSpan.FromMinutes(1));
            }
        }

        return status;
    }
}