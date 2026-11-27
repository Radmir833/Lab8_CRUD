using System.Globalization;
using System.Text;
using Lab12_EventsDelegates;

Console.OutputEncoding = Encoding.UTF8;
CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");

var manager = new StudentManager();
var logger = new ConsoleLogger(manager);
var statistics = new StatisticsCounter(manager);
_ = new EmailNotifier(manager);
manager.StudentGradeChanged += (student, oldGrade, newGrade) =>
    Console.WriteLine($"[GRADE] {student.FullName}: {oldGrade:F2} -> {newGrade:F2}");

var ivan = new Student(1, "Иванов Иван", 20, "ИС-31", 4.7);
var anna = new Student(2, "Петрова Анна", 19, "ИС-31", 4.2);
manager.AddStudent(ivan);
manager.AddStudent(anna);
manager.AddStudent(new Student(3, "Сидоров Пётр", 21, "ИС-32", 3.8));
manager.UpdateStudent(ivan with { AverageGrade = 4.9 });
manager.DeleteStudent(3);
statistics.PrintStatistics();

Console.WriteLine("--- Оставшиеся студенты ---");
foreach (Student student in manager.GetAllStudents())
    Console.WriteLine(student);

Console.WriteLine("\n--- Проверка отписки и обновления без смены балла ---");
logger.Unsubscribe();
manager.UpdateStudent(anna with { GroupName = "ИС-32" });
