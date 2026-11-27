using System.ComponentModel.DataAnnotations;

namespace Lab11_EFCore_Self;

public class Student
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string FullName { get; set; } = "";

    [Range(15, 100)]
    public int Age { get; set; }

    [Required, MaxLength(20)]
    public string GroupName { get; set; } = "";

    [Range(0d, 5d)]
    public double AverageGrade { get; set; }

    [MaxLength(100)]
    public string? Email { get; set; }

    public DateTime EnrollmentDate { get; set; }
}
