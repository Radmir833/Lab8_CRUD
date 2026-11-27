namespace Lab12_EventsDelegates;

public sealed class ConsoleLogger
{
    private readonly StudentManager _manager;

    public ConsoleLogger(StudentManager manager)
    {
        _manager = manager;
        _manager.StudentAdded += OnStudentAdded;
        _manager.StudentUpdated += OnStudentUpdated;
        _manager.StudentDeleted += OnStudentDeleted;
    }

    public void Unsubscribe()
    {
        _manager.StudentAdded -= OnStudentAdded;
        _manager.StudentUpdated -= OnStudentUpdated;
        _manager.StudentDeleted -= OnStudentDeleted;
    }

    private static void OnStudentAdded(Student student) => Console.WriteLine($"[LOG] Добавлен: {student}");
    private static void OnStudentUpdated(Student student) => Console.WriteLine($"[LOG] Обновлён: {student}");
    private static void OnStudentDeleted(Student student) => Console.WriteLine($"[LOG] Удалён: {student}");
}
