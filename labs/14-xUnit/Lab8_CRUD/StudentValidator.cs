using System.Linq;

namespace Lab8_CRUD
{
    /// <summary>Проверяет данные студента независимо от формы и базы.</summary>
    public sealed class StudentValidator
    {
        /// <summary>Проверяет, что в непустом ФИО нет цифр.</summary>
        /// <param name="fullName">ФИО для проверки.</param>
        /// <returns>Результат проверки ФИО.</returns>
        public bool ValidateFullName(string fullName) =>
            !string.IsNullOrWhiteSpace(fullName) && !fullName.Any(char.IsDigit);

        /// <summary>Проверяет допустимый возраст.</summary>
        /// <param name="age">Возраст в годах.</param>
        /// <returns>Результат проверки возраста.</returns>
        public bool ValidateAge(int age) => age >= 15 && age <= 100;

        /// <summary>Проверяет, что группа указана.</summary>
        /// <param name="groupName">Название группы.</param>
        /// <returns>Результат проверки группы.</returns>
        public bool ValidateGroup(string groupName) => !string.IsNullOrWhiteSpace(groupName);

        /// <summary>Проверяет границы среднего балла.</summary>
        /// <param name="grade">Средний балл.</param>
        /// <returns>Результат проверки балла.</returns>
        public bool ValidateGrade(double grade) => grade >= 0 && grade <= 5;
    }
}
