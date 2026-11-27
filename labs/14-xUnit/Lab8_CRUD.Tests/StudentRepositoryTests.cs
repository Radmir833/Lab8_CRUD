using Xunit;

namespace Lab8_CRUD.Tests
{
    public class StudentRepositoryTests
    {
        [Theory]
        [InlineData("Иванов Иван", true)]
        [InlineData("иванов иван", true)]
        [InlineData("Петрова Анна", false)]
        public void Exists_ReturnsExpected(string fullName, bool expected)
        {
            IStudentRepository repository = new MockStudentRepository(new[] { "Иванов Иван" });
            bool result = repository.Exists(fullName);
            Assert.Equal(expected, result);
        }
    }
}
