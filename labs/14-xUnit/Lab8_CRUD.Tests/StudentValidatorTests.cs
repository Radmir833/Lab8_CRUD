using Xunit;

namespace Lab8_CRUD.Tests
{
    public class StudentValidatorTests
    {
        private readonly StudentValidator validator = new StudentValidator();

        [Fact]
        public void ValidateFullName_AcceptsStudentName()
        {
            string fullName = "Иванов Иван";
            bool result = validator.ValidateFullName(fullName);
            Assert.True(result);
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("   ", false)]
        [InlineData("Иванов 1", false)]
        [InlineData("А", true)]
        public void ValidateFullName_ReturnsExpected(string name, bool expected) =>
            Assert.Equal(expected, validator.ValidateFullName(name));

        [Fact]
        public void ValidateAge_AcceptsTwenty() => Assert.True(validator.ValidateAge(20));

        [Theory]
        [InlineData(15, true)]
        [InlineData(100, true)]
        [InlineData(14, false)]
        [InlineData(101, false)]
        public void ValidateAge_ChecksBounds(int age, bool expected) =>
            Assert.Equal(expected, validator.ValidateAge(age));

        [Fact]
        public void ValidateGroup_AcceptsGroupCode() => Assert.True(validator.ValidateGroup("ИС-31"));

        [Theory]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("   ", false)]
        [InlineData("А", true)]
        public void ValidateGroup_ReturnsExpected(string group, bool expected) =>
            Assert.Equal(expected, validator.ValidateGroup(group));

        [Fact]
        public void ValidateGrade_AcceptsFourAndHalf() => Assert.True(validator.ValidateGrade(4.5));

        [Theory]
        [InlineData(0d, true)]
        [InlineData(5d, true)]
        [InlineData(-1d, false)]
        [InlineData(6d, false)]
        [InlineData(double.NaN, false)]
        public void ValidateGrade_ChecksBounds(double grade, bool expected) =>
            Assert.Equal(expected, validator.ValidateGrade(grade));
    }
}
