using Microsoft.Data.Sqlite;

namespace DNS_on_Tray.Tests
{
    /// <summary>
    /// The DNS class keeps its database path in static state, so every test that uses it runs
    /// in this collection (not in parallel) and gets a fresh temporary file.
    /// </summary>
    [CollectionDefinition("Database", DisableParallelization = true)]
    public class DatabaseCollection
    {
    }

    public abstract class DatabaseTest : IDisposable
    {
        private readonly string directory = Path.Combine(Path.GetTempPath(), "dnsontray-tests-" + Guid.NewGuid());

        protected DatabaseTest()
        {
            Directory.CreateDirectory(directory);
            UseDatabase("test.db");
        }

        protected string PathFor(string name) => Path.Combine(directory, name);

        protected string UseDatabase(string name)
        {
            SqliteConnection.ClearAllPools();
            DNS.DbPath = PathFor(name);
            return DNS.DbPath;
        }

        public void Dispose()
        {
            SqliteConnection.ClearAllPools();
            try { Directory.Delete(directory, true); } catch (IOException) { }
        }
    }
}
