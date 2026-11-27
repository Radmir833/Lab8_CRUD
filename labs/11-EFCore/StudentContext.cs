using Microsoft.EntityFrameworkCore;

namespace Lab11_EFCore_Self;

public class StudentContext : DbContext
{
    public DbSet<Student> Students => Set<Student>();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        string connection = Environment.GetEnvironmentVariable("LAB11_CONNECTION_STRING")
            ?? "Server=.;Database=StudentDB_EF_Self;Integrated Security=True;TrustServerCertificate=True;";
        options.UseSqlServer(connection);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Student>().Property(student => student.EnrollmentDate).HasColumnType("date");
        modelBuilder.Entity<Student>().ToTable("Students", table =>
        {
            table.HasCheckConstraint("CK_Students_Age", "[Age] BETWEEN 15 AND 100");
            table.HasCheckConstraint("CK_Students_Grade", "[AverageGrade] BETWEEN 0 AND 5");
        });
    }
}
