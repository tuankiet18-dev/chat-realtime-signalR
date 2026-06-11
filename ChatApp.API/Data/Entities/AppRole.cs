namespace ChatApp.Data.Entities;

public sealed class AppRole
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public List<AppUser> Users { get; set; } = [];
}
