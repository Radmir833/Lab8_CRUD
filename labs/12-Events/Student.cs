namespace Lab12_EventsDelegates;

public sealed record Student(int Id, string FullName, int Age, string GroupName, double AverageGrade)
{
    public override string ToString() => $"{Id}: {FullName}, {Age} лет, {GroupName}, балл {AverageGrade:F2}";
}
