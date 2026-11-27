namespace Lab12_EventsDelegates;

public delegate void StudentEventHandler(Student student);
public delegate void StudentGradeChangedHandler(Student student, double oldGrade, double newGrade);

public sealed class StudentManager
{
    private readonly List<Student> _students = new();
    public event StudentEventHandler? StudentAdded;
    public event StudentEventHandler? StudentUpdated;
    public event StudentEventHandler? StudentDeleted;
    public event StudentGradeChangedHandler? StudentGradeChanged;

    public void AddStudent(Student student)
    {
        if (_students.Exists(current => current.Id == student.Id))
            throw new ArgumentException("Студент с таким Id уже существует.", nameof(student));

        _students.Add(student);
        StudentAdded?.Invoke(student);
    }

    public bool UpdateStudent(Student student)
    {
        int index = _students.FindIndex(current => current.Id == student.Id);
        if (index < 0) return false;

        double oldGrade = _students[index].AverageGrade;
        _students[index] = student;
        StudentUpdated?.Invoke(student);
        if (oldGrade != student.AverageGrade)
            StudentGradeChanged?.Invoke(student, oldGrade, student.AverageGrade);
        return true;
    }

    public bool DeleteStudent(int id)
    {
        int index = _students.FindIndex(current => current.Id == id);
        if (index < 0) return false;

        Student removed = _students[index];
        _students.RemoveAt(index);
        StudentDeleted?.Invoke(removed);
        return true;
    }

    public IReadOnlyList<Student> GetAllStudents() => _students.AsReadOnly();
}
