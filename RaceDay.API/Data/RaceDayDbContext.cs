using Microsoft.EntityFrameworkCore;
using RaceDay.API.Models;

namespace RaceDay.API.Data;

public class RaceDayDbContext(DbContextOptions<RaceDayDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Enrolment> Enrolments => Set<Enrolment>();
    public DbSet<Result> Results => Set<Result>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasIndex(x => x.Email).IsUnique();
        modelBuilder.Entity<User>().ToTable(t => t.HasCheckConstraint("CK_User_Role", "[Role] IN ('Organiser','Participant')"));
        modelBuilder.Entity<Event>().ToTable(t =>
        {
            t.HasCheckConstraint("CK_Event_Status", "[Status] IN ('Draft','Published','Cancelled','Completed')");
            t.HasCheckConstraint("CK_Event_EventType", "[EventType] IN ('Run','Walk','Cycle')");
        });
        modelBuilder.Entity<Enrolment>().ToTable(t => t.HasCheckConstraint("CK_Enrolment_Status", "[Status] IN ('Confirmed','Cancelled')"));
        modelBuilder.Entity<Event>().HasOne(x => x.Organiser).WithMany(x => x.OrganisedEvents)
            .HasForeignKey(x => x.OrganiserID).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Category>().HasOne(x => x.Event).WithMany(x => x.Categories)
            .HasForeignKey(x => x.EventID).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Enrolment>().HasOne(x => x.User).WithMany(x => x.Enrolments)
            .HasForeignKey(x => x.UserID).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Enrolment>().HasOne(x => x.Category).WithMany(x => x.Enrolments)
            .HasForeignKey(x => x.CategoryID).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Result>().HasIndex(x => x.EnrolmentID).IsUnique();
        modelBuilder.Entity<Result>().HasOne(x => x.Enrolment).WithOne(x => x.Result)
            .HasForeignKey<Result>(x => x.EnrolmentID).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Payment>().HasIndex(x => x.EnrolmentID).IsUnique();
        modelBuilder.Entity<Payment>().HasOne(x => x.Enrolment).WithOne(x => x.Payment)
            .HasForeignKey<Payment>(x => x.EnrolmentID).OnDelete(DeleteBehavior.Cascade);
        base.OnModelCreating(modelBuilder);
    }
}
