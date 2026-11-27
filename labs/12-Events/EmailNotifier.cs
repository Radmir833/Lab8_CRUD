namespace Lab12_EventsDelegates;

public sealed class EmailNotifier
{
    public EmailNotifier(StudentManager manager)
    {
        manager.StudentAdded += Notify;
        manager.StudentUpdated += Notify;
    }

    private static void Notify(Student student) =>
        Console.WriteLine($"[EMAIL] Отправлено письмо студенту {student.FullName} (имитация)");
}
