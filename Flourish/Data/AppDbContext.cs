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
    public DbSet<GoalLink> GoalLinks => Set<GoalLink>();
    public DbSet<SharedGoal> SharedGoals => Set<SharedGoal>();
    public DbSet<SharedGoalMember> SharedGoalMembers => Set<SharedGoalMember>();

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

        modelBuilder.Entity<SharedGoal>(e =>
        {
            e.HasOne(sg => sg.ReviewPeriod).WithMany().HasForeignKey(sg => sg.ReviewPeriodId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(sg => sg.CreatedBy).WithMany().HasForeignKey(sg => sg.CreatedById).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SharedGoalMember>(e =>
        {
            e.HasOne(m => m.SharedGoal).WithMany(sg => sg.Members).HasForeignKey(m => m.SharedGoalId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(m => m.User).WithMany().HasForeignKey(m => m.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(m => new { m.SharedGoalId, m.UserId }).IsUnique();
        });

        modelBuilder.Entity<GoalLink>(e =>
        {
            e.HasOne(gl => gl.Goal1).WithMany().HasForeignKey(gl => gl.GoalId1).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(gl => gl.Goal2).WithMany().HasForeignKey(gl => gl.GoalId2).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(gl => gl.CreatedBy).WithMany().HasForeignKey(gl => gl.CreatedById).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(gl => new { gl.GoalId1, gl.GoalId2 }).IsUnique();
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
