using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Lab8_CRUD
{
    /// <summary>Записывает события программы и архивирует большой журнал.</summary>
    public static class Logger
    {
        private static readonly object Gate = new object();
        private const long MaxLogBytes = 1024 * 1024;
        /// <summary>Возвращает путь к рабочему журналу.</summary>
        public static string LogPath { get; } =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "errors.log");

        /// <summary>Записывает диагностическое сообщение об ошибке.</summary>
        /// <param name="message">Контекст операции.</param>
        /// <param name="error">Исходное исключение, если оно есть.</param>
        public static void LogError(string message, Exception error = null)
        {
            Write("ERROR", message, error);
        }

        /// <summary>Записывает сведения об успешной операции.</summary>
        /// <param name="message">Описание операции.</param>
        public static void LogInfo(string message)
        {
            Write("INFO", message, null);
        }

        /// <summary>Архивирует текущий журнал, если его размер превышает 1 МБ.</summary>
        public static void ArchiveIfLarge()
        {
            TryFileOperation(() =>
            {
                var info = new FileInfo(LogPath);
                if (!info.Exists || info.Length <= MaxLogBytes) return;

                string archive = Path.Combine(info.DirectoryName, "errors_old.log");
                if (File.Exists(archive)) File.Replace(LogPath, archive, null);
                else File.Move(LogPath, archive);
            });
        }

        private static void Write(string level, string message, Exception error)
        {
            string details = error == null ? "" : $" | {error.GetType().Name}: {error.Message}";
            string line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {level} | {message}{details}";
            line = line.Replace("\r", " ").Replace("\n", " ");
            TryFileOperation(() => File.AppendAllText(LogPath, line + Environment.NewLine, Encoding.UTF8));
        }

        private static void TryFileOperation(Action action)
        {
            lock (Gate)
            {
                try { action(); }
                catch (IOException ex) { Trace.TraceWarning("Журнал недоступен: {0}", ex.Message); }
                catch (UnauthorizedAccessException ex) { Trace.TraceWarning("Журнал недоступен: {0}", ex.Message); }
            }
        }
    }
}
