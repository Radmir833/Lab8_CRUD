namespace Lab12_EventsDelegates;

public sealed class StatisticsCounter
{
    private int _addedCount;
    private int _updatedCount;
    private int _deletedCount;

    public StatisticsCounter(StudentManager manager)
    {
        manager.StudentAdded += OnStudentAdded;
        manager.StudentUpdated += OnStudentUpdated;
        manager.StudentDeleted += OnStudentDeleted;
    }

    private void OnStudentAdded(Student _) => Console.WriteLine($"[STAT] Добавлено: {++_addedCount}");
    private void OnStudentUpdated(Student _) => Console.WriteLine($"[STAT] Обновлено: {++_updatedCount}");
    private void OnStudentDeleted(Student _) => Console.WriteLine($"[STAT] Удалено: {++_deletedCount}");

    public void PrintStatistics()
    {
        Console.WriteLine("--- Статистика ---");
        Console.WriteLine($"Добавлено: {_addedCount}\nОбновлено: {_updatedCount}\nУдалено: {_deletedCount}");
    }
}
