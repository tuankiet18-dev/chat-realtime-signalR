using ChatApp.Contracts;

namespace ChatApp.Services;

public sealed class PresenceService
{
    private readonly Lock sync = new();
    private readonly Dictionary<Guid, PresenceUser> users = [];
    private readonly Dictionary<string, Guid> connections = [];

    public IReadOnlyList<PresenceUser> Connect(string connectionId, PresenceUser user)
    {
        lock (sync)
        {
            connections[connectionId] = user.UserId;
            users[user.UserId] = user;
            return users.Values.OrderBy(existing => existing.DisplayName).ToList();
        }
    }

    public IReadOnlyList<PresenceUser> Disconnect(string connectionId)
    {
        lock (sync)
        {
            if (connections.Remove(connectionId, out var userId) &&
                !connections.ContainsValue(userId))
            {
                users.Remove(userId);
            }

            return users.Values.OrderBy(existing => existing.DisplayName).ToList();
        }
    }

    public IReadOnlyList<PresenceUser> Snapshot()
    {
        lock (sync)
        {
            return users.Values.OrderBy(existing => existing.DisplayName).ToList();
        }
    }
}
