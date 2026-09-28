using System;
using GoogleDriveClient;
using Xunit;

namespace GoogleDriveClient.Tests
{
    public class DriveTests
    {
        [Fact]
        public void Document_AppendText_CalculatesCorrectSize()
        {
            var doc = new Document("doc_1", "notes.txt", "Hello");
            doc.AppendText(" World");

            Assert.Equal(11, doc.CalculateTotalSize());
            Assert.Equal("notes.txt", doc.Name);
        }

        [Fact]
        public void Spreadsheet_SetCell_CalculatesCorrectSize()
        {
            var sheet = new Spreadsheet("sheet_1", "table.xlsx");
            sheet.SetCell("A1", "100");
            sheet.SetCell("B1", "200");

            Assert.Equal(10, sheet.CalculateTotalSize());
        }

        [Fact]
        public void DriveFolder_Composite_CalculatesRecursiveSize()
        {
            var root = new DriveFolder("root", "RootFolder");
            var subFolder = new DriveFolder("sub", "SubFolder");
            var doc = new Document("d1", "test.txt", "12345");

            subFolder.AddChild(doc);
            root.AddChild(subFolder);

            Assert.Equal(5, root.CalculateTotalSize());
        }

        [Fact]
        public void DriveFolder_AddDuplicateName_ThrowsInvalidOperationException()
        {
            var folder = new DriveFolder("root", "RootFolder");
            var doc1 = new Document("d1", "report.txt", "A");
            var doc2 = new Document("d2", "report.txt", "B");

            folder.AddChild(doc1);

            Assert.Throws<InvalidOperationException>(() => folder.AddChild(doc2));
        }

        [Fact]
        public void StoragePool_OperatorAddAndRemove_WorksCorrectly()
        {
            var pool = new StoragePool<DriveItem>(capacity: 2);
            var doc1 = new Document("d1", "file1.txt", "Data");
            var doc2 = new Document("d2", "file2.txt", "More Data");

            pool += doc1;
            pool += doc2;

            Assert.Equal(2, pool.Count);
            Assert.True(pool.IsFull);

            pool -= "d1";
            Assert.Equal(1, pool.Count);
            Assert.False(pool.Contains("d1"));
        }

        [Fact]
        public void StoragePool_ExceedCapacity_ThrowsInvalidOperationException()
        {
            var pool = new StoragePool<DriveItem>(capacity: 1);
            var doc1 = new Document("d1", "file1.txt", "A");
            var doc2 = new Document("d2", "file2.txt", "B");

            pool.Add(doc1);

            Assert.Throws<InvalidOperationException>(() => pool.Add(doc2));
        }

        [Fact]
        public void MemoryCredentialStore_SaveAndLoad_ReturnsExpectedCredential()
        {
            var store = new MemoryCredentialStore();
            var cred = new UserCredential("client_id", "access_token", "refresh_token", 3600);

            store.SaveCredentials("user_1", cred);
            var loaded = store.LoadCredentials("user_1");

            Assert.NotNull(loaded);
            Assert.Equal("client_id", loaded.ClientId);
            Assert.Equal("access_token", loaded.AccessToken);
        }

        [Fact]
        public void Service_CheckAuthorization_WithoutAuth_ThrowsDriveAuthenticationException()
        {
            var store = new MemoryCredentialStore();
            var service = new GoogleDriveService(store);

            var ex = Assert.Throws<DriveAuthenticationException>(() => service.CheckAuthorization());
            Assert.Equal(401, ex.StatusCode);
        }

        [Fact]
        public void Service_GetItem_NonExistent_ThrowsDriveItemNotFoundException()
        {
            var store = new MemoryCredentialStore();
            var service = new GoogleDriveService(store);
            var cred = new UserCredential("client_id", "access_token", "refresh_token", 3600);
            service.Authenticate("user_1", cred);

            var ex = Assert.Throws<DriveItemNotFoundException>(() => service.GetItem("non_existing_id"));
            Assert.Equal(404, ex.StatusCode);
        }
    }
}