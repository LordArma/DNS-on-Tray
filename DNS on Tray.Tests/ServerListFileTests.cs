namespace DNS_on_Tray.Tests
{
    [Collection("Database")]
    public class ServerListFileTests : DatabaseTest
    {
        [Fact]
        public void ExportThenImport_RestoresRemovedEntries()
        {
            string file = PathFor("list.json");
            int count = ServerListFile.Export(file);
            Assert.Equal(Helper.PopularDNS().Count, count);

            new DNS("Cloudflare", "", "").Remove();
            var (added, skipped) = ServerListFile.Import(file);

            Assert.Equal(1, added);
            Assert.Equal(count - 1, skipped);
            Assert.Equal("https://cloudflare-dns.com/dns-query", DNS.Find("Cloudflare")!.DoH());
        }

        [Fact]
        public void Import_SkipsInvalidAndReservedEntries()
        {
            string file = PathFor("list.json");
            File.WriteAllText(file,
                "{\"servers\":[{\"name\":\"Bad\",\"dns1\":\"1.1.1\"},{\"name\":\"Good\",\"dns1\":\"9.9.9.9\"},{\"name\":\"clear\",\"dns1\":\"9.9.9.9\"}]}");

            var (added, skipped) = ServerListFile.Import(file);

            Assert.Equal(1, added);
            Assert.Equal(2, skipped);
            Assert.NotNull(DNS.Find("Good"));
        }

        [Fact]
        public void Import_InvalidJson_Throws()
        {
            string file = PathFor("list.json");
            File.WriteAllText(file, "not json");

            Assert.ThrowsAny<System.Text.Json.JsonException>(() => ServerListFile.Import(file));
        }
    }
}
