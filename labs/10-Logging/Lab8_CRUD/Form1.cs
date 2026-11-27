using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Lab8_CRUD
{
    public partial class Form1 : Form
    {
        // 1. Строка для создания базы данных (подключаемся к системной master)
        private readonly string masterConnectionString =
            @"Data Source=.;Initial Catalog=master;Integrated Security=True;TrustServerCertificate=True";

        // 2. Рабочая строка (подключаемся к StudentDB_Lab10)
        private readonly string connectionString =
            Environment.GetEnvironmentVariable("LAB8_CONNECTION_STRING") ??
            @"Data Source=.;Initial Catalog=StudentDB_Lab10;Integrated Security=True;TrustServerCertificate=True";

        private DataTable studentsTable;
        private int selectedId = 0;
        private DataTable lastSnapshot = null;

        public Form1()
        {
            InitializeComponent();
            Logger.ArchiveIfLarge();

            // Привязка обработчиков
            dataGridViewStudents.SelectionChanged += dataGridViewStudents_SelectionChanged;
            buttonAdd.Click += buttonAdd_Click;
            buttonUpdate.Click += buttonUpdate_Click;
            buttonDelete.Click += buttonDelete_Click;
            buttonClear.Click += buttonClear_Click;

            textBoxSearch.TextChanged += (sender, args) => ApplyStudentSearch();

            // Если вы добавили эти кнопки на форму, раскомментируйте:
            buttonExportCsv.Click += buttonExportCsv_Click;
            buttonShowLog.Click += buttonShowLog_Click;
            // buttonUndo.Click += buttonUndo_Click;
        }

        // ---------- ЗАГРУЗКА ФОРМЫ ----------
        private void Form1_Load(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("LAB8_CONNECTION_STRING")))
                EnsureDatabaseCreated();
            LoadStudents();
            UpdateStatus();
        }

        // ---------- АВТОСОЗДАНИЕ БАЗЫ И ТАБЛИЦЫ ----------
        private void EnsureDatabaseCreated()
        {
            try
            {
                // 1. Создаем базу данных, если её нет
                using (var masterConn = new SqlConnection(masterConnectionString))
                {
                    masterConn.Open();
                    string createDbSql = "IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'StudentDB_Lab10') CREATE DATABASE StudentDB_Lab10;";
                    using (var cmd = new SqlCommand(createDbSql, masterConn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }

                // 2. Создаем таблицу внутри StudentDB_Lab10
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string createTableSql = @"
                        IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Students]') AND type in (N'U'))
                        BEGIN
                            CREATE TABLE [dbo].[Students](
                                [Id] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
                                [FullName] [nvarchar](100) NOT NULL,
                                [Age] [int] NOT NULL,
                                [GroupName] [nvarchar](50) NOT NULL,
                                [AverageGrade] [decimal](3, 2) NOT NULL,
                                CONSTRAINT UQ_Students_FullName UNIQUE (FullName),
                                CONSTRAINT CK_Students_Age CHECK (Age BETWEEN 15 AND 100),
                                CONSTRAINT CK_Students_Grade CHECK (AverageGrade BETWEEN 0 AND 5)
                            );
                        END";
                    using (var cmd = new SqlCommand(createTableSql, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при инициализации базы данных:\n" + ex.Message +
                    "\n\nПроверьте подключение к локальному экземпляру SQL Server.",
                    "Критическая ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ---------- ЗАГРУЗКА ДАННЫХ (READ) ----------
        private void LoadStudents()
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand(
                    "SELECT Id, FullName, Age, GroupName, AverageGrade FROM Students ORDER BY Id", conn))
                using (var adapter = new SqlDataAdapter(cmd))
                {
                    var table = new DataTable();
                    adapter.Fill(table);
                    table.CaseSensitive = false;
                    studentsTable = table;
                    dataGridViewStudents.DataSource = table.DefaultView;
                    ApplyStudentSearch();

                    if (dataGridViewStudents.Columns.Count > 0)
                    {
                        dataGridViewStudents.Columns["Id"].Visible = false;
                        dataGridViewStudents.Columns["FullName"].HeaderText = "ФИО";
                        dataGridViewStudents.Columns["Age"].HeaderText = "Возраст";
                        dataGridViewStudents.Columns["GroupName"].HeaderText = "Группа";
                        dataGridViewStudents.Columns["AverageGrade"].HeaderText = "Средний балл";
                    }
                }
                selectedId = 0;
                UpdateStatus();
            }
            catch (SqlException ex) { ReportDatabaseError("Загрузка студентов", ex); }
            catch (TimeoutException ex) { ReportDatabaseError("Загрузка студентов", ex); }
            catch (Exception ex) { ReportUnexpectedError("Загрузка студентов", ex); }
        }

        private void ApplyStudentSearch()
        {
            if (studentsTable == null) return;
            var filter = new StringBuilder();
            foreach (char character in textBoxSearch.Text.Trim())
            {
                switch (character)
                {
                    case '\'': filter.Append("''"); break;
                    case '[': filter.Append("[[]"); break;
                    case ']': filter.Append("[]]"); break;
                    case '*': filter.Append("[*]"); break;
                    case '%': filter.Append("[%]"); break;
                    default: filter.Append(character); break;
                }
            }
            studentsTable.DefaultView.RowFilter =
                filter.Length == 0 ? "" : $"FullName LIKE '%{filter}%'";
        }

        // ---------- ВЫБОР СТРОКИ ----------
        private void dataGridViewStudents_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridViewStudents.CurrentRow == null) return;

            var row = dataGridViewStudents.CurrentRow;
            if (row.Cells["Id"].Value == null || row.Cells["Id"].Value == DBNull.Value) return;

            selectedId = Convert.ToInt32(row.Cells["Id"].Value);
            textBoxName.Text = row.Cells["FullName"].Value?.ToString() ?? "";
            numericAge.Value = Convert.ToDecimal(row.Cells["Age"].Value ?? 15);
            textBoxGroup.Text = row.Cells["GroupName"].Value?.ToString() ?? "";
            numericGrade.Value = Convert.ToDecimal(row.Cells["AverageGrade"].Value ?? 0);
        }

        // ---------- CREATE ----------
        private void buttonAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateInput()) return;

            try
            {
                string fullName = textBoxName.Text.Trim();
                if (IsStudentExists(fullName))
                {
                    const string message = "Студент с таким ФИО уже существует!";
                    Logger.LogError("Дубликат ФИО: " + fullName);
                    MessageBox.Show(message, "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SaveSnapshot();

                    using (var cmd = new SqlCommand(
                        @"INSERT INTO Students (FullName, Age, GroupName, AverageGrade)
                          VALUES (@name, @age, @group, @grade)", conn))
                    {
                        cmd.Parameters.AddWithValue("@name", textBoxName.Text.Trim());
                        cmd.Parameters.AddWithValue("@age", (int)numericAge.Value);
                        cmd.Parameters.AddWithValue("@group", textBoxGroup.Text.Trim());
                        cmd.Parameters.AddWithValue("@grade", numericGrade.Value);

                        cmd.ExecuteNonQuery();
                    }
                }

                Logger.LogInfo("Добавлен студент: " + fullName);
                ClearInputs();
                LoadStudents();
                labelStatus.Text = "Студент добавлен";
            }
            catch (SqlException ex) { ReportDatabaseError("Добавление студента", ex); }
            catch (TimeoutException ex) { ReportDatabaseError("Добавление студента", ex); }
            catch (Exception ex) { ReportUnexpectedError("Добавление студента", ex); }
        }

        // ---------- UPDATE ----------
        private void buttonUpdate_Click(object sender, EventArgs e)
        {
            if (selectedId == 0)
            {
                MessageBox.Show("Выберите студента в таблице для редактирования.",
                    "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (!ValidateInput()) return;

            try
            {
                SaveSnapshot();

                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand(
                    @"UPDATE Students
                      SET FullName = @name, Age = @age, GroupName = @group, AverageGrade = @grade
                      WHERE Id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@name", textBoxName.Text.Trim());
                    cmd.Parameters.AddWithValue("@age", (int)numericAge.Value);
                    cmd.Parameters.AddWithValue("@group", textBoxGroup.Text.Trim());
                    cmd.Parameters.AddWithValue("@grade", numericGrade.Value);
                    cmd.Parameters.AddWithValue("@id", selectedId);

                    conn.Open();
                    int rows = cmd.ExecuteNonQuery();
                    if (rows == 0)
                        MessageBox.Show("Запись не найдена (возможно, уже удалена).",
                            "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                LoadStudents();
                labelStatus.Text = "Данные обновлены";
            }
            catch (SqlException ex) { ReportDatabaseError("Обновление студента", ex); }
            catch (TimeoutException ex) { ReportDatabaseError("Обновление студента", ex); }
            catch (Exception ex) { ReportUnexpectedError("Обновление студента", ex); }
        }

        // ---------- DELETE ----------
        private void buttonDelete_Click(object sender, EventArgs e)
        {
            if (selectedId == 0)
            {
                MessageBox.Show("Выберите студента для удаления.",
                    "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var confirm = MessageBox.Show(
                $"Удалить студента «{textBoxName.Text}»?",
                "Подтверждение удаления",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            try
            {
                string fullName = textBoxName.Text;
                SaveSnapshot();

                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("DELETE FROM Students WHERE Id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", selectedId);
                    conn.Open();
                    if (cmd.ExecuteNonQuery() == 0)
                    {
                        MessageBox.Show("Запись уже удалена.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                Logger.LogInfo("Удалён студент: " + fullName);
                ClearInputs();
                LoadStudents();
                labelStatus.Text = "Студент удалён";
            }
            catch (SqlException ex) { ReportDatabaseError("Удаление студента", ex); }
            catch (TimeoutException ex) { ReportDatabaseError("Удаление студента", ex); }
            catch (Exception ex) { ReportUnexpectedError("Удаление студента", ex); }
        }

        private bool IsStudentExists(string fullName)
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

        private static string GetSqlErrorMessage(SqlException ex)
        {
            switch (ex.Number)
            {
                case 4060: return "Не удалось открыть базу данных. Проверьте её имя и права доступа.";
                case 18456: return "Ошибка входа. Проверьте учётные данные и права доступа.";
                case 208: return "Таблица Students не найдена. Проверьте схему базы.";
                case 2601:
                case 2627: return "Такая запись уже существует. Проверьте ФИО студента.";
                case 547: return "Нарушено ограничение базы. Проверьте возраст и средний балл.";
                case -2:
                case 258: return "Истекло время ожидания ответа SQL Server.";
                case 2:
                case 26:
                case 40:
                case 53: return "Не удалось подключиться. Проверьте имя SQL Server и сеть.";
                default: return $"Ошибка SQL Server (код {ex.Number}). Подробности в журнале.";
            }
        }

        private static void ReportDatabaseError(string operation, SqlException exception)
        {
            Logger.LogError($"{operation} (SQL {exception.Number})", exception);
            MessageBox.Show(GetSqlErrorMessage(exception), "Ошибка базы данных",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private static void ReportDatabaseError(string operation, TimeoutException exception)
        {
            Logger.LogError(operation + ": тайм-аут", exception);
            MessageBox.Show("Сервер базы данных не ответил вовремя.", "Тайм-аут",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private static void ReportUnexpectedError(string operation, Exception exception)
        {
            Logger.LogError(operation, exception);
            MessageBox.Show("Операцию не удалось выполнить. Подробности в журнале.",
                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void buttonShowLog_Click(object sender, EventArgs e)
        {
            if (!File.Exists(Logger.LogPath))
            {
                MessageBox.Show("Журнал пока пуст.", "Журнал", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                System.Diagnostics.Process.Start("notepad.exe", "\"" + Logger.LogPath + "\"");
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                ReportUnexpectedError("Открытие журнала", ex);
            }
        }

        // ---------- ОЧИСТКА ----------
        private void buttonClear_Click(object sender, EventArgs e)
        {
            ClearInputs();
            labelStatus.Text = "Поля очищены";
        }

        private void ClearInputs()
        {
            textBoxName.Text = "";
            numericAge.Value = 15;
            textBoxGroup.Text = "";
            numericGrade.Value = 0;
            selectedId = 0;
            dataGridViewStudents.ClearSelection();
        }

        // ---------- ВАЛИДАЦИЯ ----------
        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(textBoxName.Text))
            {
                MessageBox.Show("Поле «ФИО» не может быть пустым.",
                    "Валидация", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxName.Focus();
                return false;
            }
            if (textBoxName.Text.Trim().Length < 3)
            {
                MessageBox.Show("ФИО слишком короткое (минимум 3 символа).",
                    "Валидация", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (string.IsNullOrWhiteSpace(textBoxGroup.Text))
            {
                MessageBox.Show("Поле «Группа» не может быть пустым.",
                    "Валидация", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxGroup.Focus();
                return false;
            }
            return true;
        }

        // ---------- СТАТУС + СЧЁТЧИК ----------
        private void UpdateStatus()
        {
            int count = 0;
            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Students", conn))
                {
                    conn.Open();
                    count = (int)cmd.ExecuteScalar();
                }
            }
            catch { }

            string baseText = labelStatus.Text;
            int pipeIndex = baseText.IndexOf('|');
            if (pipeIndex > 0) baseText = baseText.Substring(0, pipeIndex).Trim();

            labelStatus.Text = $"{baseText} | Всего студентов: {count}";
        }

        // ---------- ЭКСПОРТ В CSV ----------
        private void buttonExportCsv_Click(object sender, EventArgs e)
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "CSV файл (*.csv)|*.csv";
                sfd.FileName = "students.csv";
                if (sfd.ShowDialog() != DialogResult.OK) return;

                try
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("Id;ФИО;Возраст;Группа;СреднийБалл");

                    foreach (DataGridViewRow row in dataGridViewStudents.Rows)
                    {
                        if (row.IsNewRow) continue;
                        sb.AppendLine(string.Join(";",
                            CsvField(row.Cells["Id"].Value),
                            CsvField(row.Cells["FullName"].Value),
                            CsvField(row.Cells["Age"].Value),
                            CsvField(row.Cells["GroupName"].Value),
                            CsvField(row.Cells["AverageGrade"].Value)));
                    }

                    File.WriteAllText(sfd.FileName, sb.ToString(), new UTF8Encoding(true));
                    MessageBox.Show("Экспорт завершён: " + sfd.FileName,
                        "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка экспорта:\n" + ex.Message,
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private static string CsvField(object value)
        {
            string text = Convert.ToString(value) ?? "";
            const char quote = '"';
            return quote + text.Replace(quote.ToString(), new string(quote, 2)) + quote;
        }

        // ---------- ОТМЕНА ----------
        private void buttonUndo_Click(object sender, EventArgs e)
        {
            if (lastSnapshot == null)
            {
                MessageBox.Show("Нет изменений для отмены.",
                    "Отмена", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (var tran = conn.BeginTransaction())
                    {
                        // Удаляем всё и восстанавливаем снимок
                        using (var del = new SqlCommand("DELETE FROM Students", conn, tran))
                            del.ExecuteNonQuery();

                        foreach (DataRow r in lastSnapshot.Rows)
                        {
                            using (var ins = new SqlCommand(
                                @"SET IDENTITY_INSERT Students ON;
                                  INSERT INTO Students (Id, FullName, Age, GroupName, AverageGrade)
                                  VALUES (@id, @name, @age, @group, @grade);
                                  SET IDENTITY_INSERT Students OFF;", conn, tran))
                            {
                                ins.Parameters.AddWithValue("@id", r["Id"]);
                                ins.Parameters.AddWithValue("@name", r["FullName"]);
                                ins.Parameters.AddWithValue("@age", r["Age"]);
                                ins.Parameters.AddWithValue("@group", r["GroupName"]);
                                ins.Parameters.AddWithValue("@grade", r["AverageGrade"]);
                                ins.ExecuteNonQuery();
                            }
                        }
                        tran.Commit();
                    }
                }

                lastSnapshot = null;
                LoadStudents();
                labelStatus.Text = "Изменение отменено";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка отмены:\n" + ex.Message,
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Снимок таблицы "до" операции
        private void SaveSnapshot()
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand(
                    "SELECT Id, FullName, Age, GroupName, AverageGrade FROM Students ORDER BY Id", conn))
                using (var adapter = new SqlDataAdapter(cmd))
                {
                    var t = new DataTable();
                    adapter.Fill(t);
                    lastSnapshot = t;
                }
            }
            catch { }
        }
    }
}
