using System;

namespace GoogleDriveClient
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("=== Демонстрація Google Drive Client (C# .NET) ===\n");

            // 1. Ініціалізація сховища облікових даних (2-га ієрархія наслідування)
            CredentialStore credentialStore = new MemoryCredentialStore();
            var driveService = new GoogleDriveService(credentialStore);

            // 2. Демонстрація обробки винятків: спроба доступу без авторизації
            Console.WriteLine("--- 1. Тест безпеки та обробки винятків ---");
            try
            {
                driveService.CheckAuthorization();
            }
            catch (DriveAuthenticationException ex)
            {
                Console.WriteLine($"[Очікуване перехоплення винятку]: {ex.Message} (Код: {ex.StatusCode})");
            }

            // 3. Авторизація користувача
            Console.WriteLine("\n--- 2. Автентифікація користувача ---");
            var userToken = new UserCredential("client_app_id_123", "access_tok_xyz", "refresh_tok_abc", 3600);
            driveService.Authenticate("user_primary", userToken);
            Console.WriteLine($"Користувач успішно увійшов. Сховище: {credentialStore.StoreName}");

            // 4. Побудова файлової структури (1-ша ієрархія наслідування, Composite Pattern)
            Console.WriteLine("\n--- 3. Створення файлової ієрархії (Composite Pattern) ---");
            var rootFolder = driveService.RootFolder;

            var docsFolder = new DriveFolder("f_docs", "My Documents");
            var doc1 = new Document("d_report", "AnnualReport.docx", "Annual sales report for 2026.");
            doc1.AppendText(" All quarterly targets achieved.");

            var sheet1 = new Spreadsheet("s_budget", "Budget2026.xlsx");
            sheet1.SetCell("A1", "Revenue");
            sheet1.SetCell("B1", "500000");

            docsFolder.AddChild(doc1);
            docsFolder.AddChild(sheet1);
            rootFolder.AddChild(docsFolder);

            var rootFile = new DriveFile("f_notes", "QuickNotes.txt", "text/plain");
            rootFolder.AddChild(rootFile);

            // 5. Демонстрація динамічного поліморфізму (CalculateTotalSize та Display)
            Console.WriteLine("\nСтруктура диска:");
            rootFolder.Display(0);

            Console.WriteLine($"\nСумарний розмір папки '{docsFolder.Name}': {docsFolder.CalculateTotalSize()} байт");
            Console.WriteLine($"Сумарний розмір всього диску: {rootFolder.CalculateTotalSize()} байт");

            // 6. Демонстрація узагальненого типу (Generics) та статичного поліморфізму
            Console.WriteLine("\n--- 4. Тестування StoragePool<T> (Generics та перевантаження операторів) ---");
            var pool = new StoragePool<DriveItem>(capacity: 5);

            // Використання оператора + (статичний поліморфізм)
            pool = pool + doc1;
            pool = pool + sheet1;

            Console.WriteLine($"Кількість елементів у кеші пулу: {pool.Count}");
            Console.WriteLine($"Сумарний розмір об'єктів у пулі: {pool.CalculatePoolSize()} байт");

            // Використання оператора - (статичний поліморфізм)
            pool = pool - doc1.Id;
            Console.WriteLine($"Після видалення '{doc1.Name}' залишилось у пулі: {pool.Count}");

            Console.WriteLine("\n=== Демонстрацію завершено успішно ===");
        }
    }
}