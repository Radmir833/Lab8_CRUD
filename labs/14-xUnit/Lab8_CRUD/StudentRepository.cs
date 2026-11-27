using System.Data;
using System.Data.SqlClient;

namespace Lab8_CRUD
{
    /// <summary>Читает данные о студентах из SQL Server.</summary>
    public sealed class StudentRepository : IStudentRepository
    {
        private readonly string connectionString;

        /// <summary>Инициализирует репозиторий для заданной базы.</summary>
        /// <param name="connectionString">Строка подключения SQL Server.</param>
        public StudentRepository(string connectionString)
        {
            this.connectionString = connectionString;
        }

        /// <inheritdoc />
        public bool Exists(string fullName)
        {
            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(
                "SELECT COUNT(*) FROM dbo.Students WHERE FullName = @name", connection))
            {
                command.Parameters.Add("@name", SqlDbType.NVarChar, 100).Value = fullName;
                connection.Open();
                return (int)command.ExecuteScalar() > 0;
            }
        }
    }
}
