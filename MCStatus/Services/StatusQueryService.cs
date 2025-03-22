using MCStatus.Models;
using MCStatus.Utilities;
using Microsoft.Extensions.Caching.Memory;

namespace MCStatus.Services;

public class StatusQueryService(IMemoryCache cache)
{
    public async Task<Status?> RequestStatusAsync(
        ulong userId, Server server, CancellationToken cancellationToken = default)
    {
        if (cache.TryGetValue(server, out Status? status) && status is not null)
            return status;

        status = await Pinger.RequestStatusAsync(server, cancellationToken);

        if (status is not null)
        {
            cache.Set(server, status, TimeSpan.FromMinutes(1));

            if (!cache.TryGetValue(userId, out HashSet<Server>? requests))
                requests = cache.Set(userId, new HashSet<Server>(), TimeSpan.FromDays(7));

            requests?.Add(server);
        }

        return status;
    }

    public IEnumerable<Server> GetPreviousRequests(ulong userId)
    {
        if (cache.TryGetValue(userId, out HashSet<Server>? previousRequests))
            return previousRequests ?? [];

        return [];
    }
}