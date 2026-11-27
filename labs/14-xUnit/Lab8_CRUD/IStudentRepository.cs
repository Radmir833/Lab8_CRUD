namespace Lab8_CRUD
{
    /// <summary>Предоставляет проверку существования студента.</summary>
    public interface IStudentRepository
    {
        /// <summary>Ищет запись по полному имени.</summary>
        /// <param name="fullName">Точное ФИО.</param>
        /// <returns>Признак существования записи.</returns>
        bool Exists(string fullName);
    }
}
