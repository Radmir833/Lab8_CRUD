using System;
using System.Collections.Generic;

namespace Lab8_CRUD.Tests
{
    public sealed class MockStudentRepository : IStudentRepository
    {
        private readonly HashSet<string> students;

        public MockStudentRepository(IEnumerable<string> names)
        {
            students = new HashSet<string>(names, StringComparer.OrdinalIgnoreCase);
        }

        public bool Exists(string fullName) => students.Contains(fullName);
    }
}
