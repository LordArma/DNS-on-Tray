using Microsoft.Data.Sqlite;

namespace DNS_on_Tray.Tests
{
    [Collection("Database")]
    public class StorageTests : DatabaseTest
    {
        [Fact]
        public void NewDatabase_IsSeededWithDefaults()
        {
            List<DNS> all = DNS.All();

            Assert.Equal(Helper.PopularDNS().Count, all.Count);
            Assert.Equal(2, DNS.Find("Google (Public DNS)")!.IPv6Servers().Count);
        }

        [Fact]
        public void NamesWithQuotes_AreStoredAndRemovedSafely()
        {
            new DNS("Bob's DNS", "9.9.9.9", "149.112.112.112").Save();

            Assert.Equal("149.112.112.112", DNS.Find("Bob's DNS")?.DNS2());
            Assert.True(DNS.NameTaken("bob's dns"));
            Assert.Null(DNS.Find("' OR '1'='1"));

            new DNS("Bob's DNS", "", "").Remove();
            Assert.Null(DNS.Find("Bob's DNS"));
        }

        [Fact]
        public void Update_CanRenameAndChangeServers()
        {
            new DNS("Quad9", "9.9.9.9", "").Save();
            new DNS("Quad 9", "9.9.9.9", "149.112.112.112", "2620:fe::fe").Update("Quad9");

            Assert.Null(DNS.Find("Quad9"));
            DNS updated = DNS.Find("Quad 9")!;
            Assert.Equal("9.9.9.9, 149.112.112.112", updated.ServersText());
            Assert.Equal(new[] { "9.9.9.9", "149.112.112.112", "2620:fe::fe" }, updated.Servers());
        }

        [Fact]
        public void Save_DoesNotDuplicateExistingName()
        {
            int before = DNS.All().Count;
            new DNS("Cloudflare", "9.9.9.9", "").Save();

            Assert.Equal(before, DNS.All().Count);
            Assert.Equal("1.1.1.1", DNS.Find("Cloudflare")!.DNS1());
        }

        [Fact]
        public void OldDatabase_IsMigratedWithoutReseeding()
        {
            string path = UseDatabase("old.db");
            using (var connection = new SqliteConnection($"Data Source={path}"))
            {
                connection.Open();
                var cmd = connection.CreateCommand();
                cmd.CommandText =
                    "CREATE TABLE dnsTable (dnsName VARCHAR(32) Primary Key, dns1 VARCHAR(32), dns2 VARCHAR(32));" +
                    "INSERT INTO dnsTable VALUES ('Cloudflare','1.1.1.1','1.0.0.1'),('My Home','192.168.1.1','192.168.1.2');";
                cmd.ExecuteNonQuery();
            }
            UseDatabase("old.db");

            Assert.Equal(2, DNS.All().Count);
            Assert.Equal("https://cloudflare-dns.com/dns-query", DNS.Find("Cloudflare")!.DoH());
            Assert.Equal("", DNS.Find("My Home")!.DNS1v6());
        }

        [Fact]
        public void EmptyFile_GetsATable()
        {
            File.WriteAllBytes(PathFor("empty.db"), Array.Empty<byte>());
            UseDatabase("empty.db");

            Assert.Equal(Helper.PopularDNS().Count, DNS.All().Count);
        }

        [Fact]
        public void CorruptFile_IsBackedUpAndReplaced()
        {
            File.WriteAllText(PathFor("corrupt.db"), new string('x', 200));
            UseDatabase("corrupt.db");

            Assert.Equal(Helper.PopularDNS().Count, DNS.All().Count);
            Assert.Single(Directory.GetFiles(Path.GetDirectoryName(PathFor("corrupt.db"))!, "corrupt.db.*.bak"));
        }
    }
}
