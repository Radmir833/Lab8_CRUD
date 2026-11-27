using System.Globalization;
using System.Text;
using Lab11_EFCore_Self;

Console.OutputEncoding = Encoding.UTF8;
CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");
var enrollment = new DateTime(2025, 9, 1);

using (var context = new StudentContext())
{
    if (!context.Students.Any())
    {
        context.Students.AddRange(
            NewStudent("Иванов Иван", 20, "ИС-31", 4.7, "ivanov@example.com"),
            NewStudent("Петрова Анна", 19, "ИС-31", 4.2),
            NewStudent("Сидоров Пётр", 21, "ИС-32", 3.3),
            NewStudent("Козлова Мария", 20, "ИС-32", 4.8, "kozlova@example.com"),
            NewStudent("Смирнов Алексей", 22, "ИС-33", 3.8));
        Console.WriteLine($"CREATE: добавлено {context.SaveChanges()} студентов.");
    }
    else
    {
        Console.WriteLine("CREATE: демоданные уже загружены.");
    }
}

using (var context = new StudentContext())
    Print("READ: все студенты", context.Students.OrderBy(student => student.Id).ToList());

using (var context = new StudentContext())
    Print("READ: балл выше 4,0", context.Students
        .Where(student => student.AverageGrade > 4.0)
        .OrderByDescending(student => student.AverageGrade).ThenBy(student => student.Id).ToList());

using (var context = new StudentContext())
    Print("READ: группа ИС-31", context.Students
        .Where(student => student.GroupName == "ИС-31").OrderBy(student => student.Id).ToList());

using (var context = new StudentContext())
{
    var student = context.Students.FirstOrDefault(item => item.FullName == "Иванов Иван");
    if (student != null)
    {
        student.AverageGrade = 4.9;
        context.SaveChanges();
        Console.WriteLine($"UPDATE: {student.FullName}, балл {student.AverageGrade:F2}.");
    }
}

using (var context = new StudentContext())
{
    var student = context.Students.FirstOrDefault(item => item.Id == 2 && item.FullName == "Петрова Анна");
    if (student != null)
    {
        student.Email = "petrova@example.com";
        context.SaveChanges();
        Console.WriteLine($"UPDATE: Id 2, электронная почта {student.Email}.");
    }
}

using (var context = new StudentContext())
{
    var student = context.Students.Where(item => item.AverageGrade < 3.5 && item.FullName == "Сидоров Пётр")
        .OrderBy(item => item.Id).FirstOrDefault();
    if (student != null)
    {
        context.Students.Remove(student);
        context.SaveChanges();
        Console.WriteLine($"DELETE: {student.FullName}, балл ниже 3,5.");
    }
}

using (var context = new StudentContext())
{
    var student = context.Students.FirstOrDefault(item => item.Id == 5 && item.FullName == "Смирнов Алексей");
    if (student != null)
    {
        context.Students.Remove(student);
        context.SaveChanges();
        Console.WriteLine($"DELETE: Id 5, {student.FullName}.");
    }
}

using (var context = new StudentContext())
    Print("Итоговый список с датами поступления", context.Students.OrderBy(student => student.Id).ToList());

Student NewStudent(string name, int age, string group, double grade, string? email = null) => new()
{
    FullName = name, Age = age, GroupName = group, AverageGrade = grade,
    Email = email, EnrollmentDate = enrollment
};

static void Print(string title, IReadOnlyList<Student> students)
{
    Console.WriteLine($"\n{title} ({students.Count})");
    foreach (var student in students)
        Console.WriteLine($"{student.Id} | {student.FullName} | {student.Age} | {student.GroupName} | " +
                          $"{student.AverageGrade:F2} | {student.Email ?? "—"} | {student.EnrollmentDate:dd.MM.yyyy}");
    Console.WriteLine();
}
