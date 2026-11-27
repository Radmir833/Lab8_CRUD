# Лабораторная работа №11

Нужны .NET 8, пакет SDK .NET и SQL Server. В новом каталоге проекта выполните:

    dotnet tool restore
    dotnet ef database update
    dotnet run

По умолчанию используется локальный SQL Server и отдельная база StudentDB_EF_Self.
Для другой машины задайте строку подключения через LAB11_CONNECTION_STRING.
Для воспроизведения первой миграции на новой базе выполните сначала dotnet ef database update InitialCreate.
Полный вывод всех операций в отчёте получен на новой базе при первом запуске.
