using ChatApp.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Data;

public sealed class ChatDbContext(DbContextOptions<ChatDbContext> options) : DbContext(options)
{
    public DbSet<AppRole> Roles => Set<AppRole>();

    public DbSet<AppUser> Users => Set<AppUser>();

    public DbSet<ChatMessage> Messages => Set<ChatMessage>();

    public DbSet<ChatAttachment> Attachments => Set<ChatAttachment>();

    public DbSet<ReadReceipt> ReadReceipts => Set<ReadReceipt>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppRole>(entity =>
        {
            entity.HasKey(role => role.Id);
            entity.HasIndex(role => role.Name).IsUnique();
            entity.Property(role => role.Name).HasMaxLength(32).IsRequired();
        });

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasKey(user => user.Id);
            entity.HasIndex(user => user.Email).IsUnique();
            entity.Property(user => user.Email).HasMaxLength(256).IsRequired();
            entity.Property(user => user.DisplayName).HasMaxLength(80).IsRequired();
            entity.Property(user => user.PasswordHash).IsRequired();
            entity.HasOne(user => user.Role)
                .WithMany(role => role.Users)
                .HasForeignKey(user => user.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.HasKey(message => message.Id);
            entity.HasIndex(message => new { message.RoomId, message.CreatedAt });
            entity.Property(message => message.RoomId).HasMaxLength(64).IsRequired();
            entity.Property(message => message.Text).HasMaxLength(4000);
            entity.Property(message => message.State).HasConversion<string>().HasMaxLength(32);
            entity.HasOne(message => message.Sender)
                .WithMany(user => user.Messages)
                .HasForeignKey(message => message.SenderId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ChatAttachment>(entity =>
        {
            entity.HasKey(attachment => attachment.Id);
            entity.Property(attachment => attachment.OriginalFileName).HasMaxLength(255).IsRequired();
            entity.Property(attachment => attachment.StoredFileName).HasMaxLength(255).IsRequired();
            entity.Property(attachment => attachment.PublicUrl).HasMaxLength(512).IsRequired();
            entity.Property(attachment => attachment.ContentType).HasMaxLength(128).IsRequired();
            entity.Property(attachment => attachment.Kind).HasConversion<string>().HasMaxLength(32);
            entity.HasOne(attachment => attachment.Message)
                .WithOne(message => message.Attachment)
                .HasForeignKey<ChatAttachment>(attachment => attachment.MessageId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ReadReceipt>(entity =>
        {
            entity.HasKey(receipt => new { receipt.MessageId, receipt.UserId });
            entity.HasOne(receipt => receipt.Message)
                .WithMany(message => message.ReadReceipts)
                .HasForeignKey(receipt => receipt.MessageId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(receipt => receipt.User)
                .WithMany(user => user.ReadReceipts)
                .HasForeignKey(receipt => receipt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
