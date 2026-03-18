using Flourish.Models;
using Microsoft.EntityFrameworkCore;

namespace Flourish.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<ReviewPeriod> ReviewPeriods => Set<ReviewPeriod>();
    public DbSet<Goal> Goals => Set<Goal>();
    public DbSet<Actionable> Actionables => Set<Actionable>();
    public DbSet<ReviewEvent> ReviewEvents => Set<ReviewEvent>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(u => u.GoogleId).IsUnique();
            e.HasIndex(u => u.Email).IsUnique();
            e.HasOne(u => u.Manager)
                .WithMany(u => u.Reports)
                .HasForeignKey(u => u.ManagerId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Goal>(e =>
        {
            e.HasOne(g => g.User)
                .WithMany(u => u.Goals)
                .HasForeignKey(g => g.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(g => g.ReviewPeriod)
                .WithMany(rp => rp.Goals)
                .HasForeignKey(g => g.ReviewPeriodId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ReviewEvent>(e =>
        {
            e.HasOne(re => re.Reviewee)
                .WithMany(u => u.ReviewsAsReviewee)
                .HasForeignKey(re => re.RevieweeId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(re => re.Reviewer)
                .WithMany(u => u.ReviewsAsReviewer)
                .HasForeignKey(re => re.ReviewerId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
