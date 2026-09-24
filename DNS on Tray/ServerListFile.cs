using System.Text.Json;
using System.Text.Json.Serialization;
using static DNS_on_Tray.Helper;

namespace DNS_on_Tray
{
    /// <summary>
    /// Imports and exports the saved server list as JSON:
    /// { "version": 1, "servers": [ { "name": ..., "dns1": ..., "dns2": ..., "dns1v6": ..., "dns2v6": ..., "doh": ... } ] }
    /// </summary>
    public static class ServerListFile
    {
        private sealed class Entry
        {
            public string Name { get; set; } = "";
            public string Dns1 { get; set; } = "";
            public string Dns2 { get; set; } = "";
            public string Dns1v6 { get; set; } = "";
            public string Dns2v6 { get; set; } = "";
            public string Doh { get; set; } = "";
        }

        private sealed class ListFile
        {
            public int Version { get; set; } = 1;
            public List<Entry> Servers { get; set; } = new();
        }

        private static readonly JsonSerializerOptions options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never
        };

        public static int Export(string path)
        {
            List<DNS> all = DNS.All();
            ListFile file = new ListFile
            {
                Servers = all.Select(d => new Entry
                {
                    Name = d.Name(),
                    Dns1 = d.DNS1(),
                    Dns2 = d.DNS2(),
                    Dns1v6 = d.DNS1v6(),
                    Dns2v6 = d.DNS2v6(),
                    Doh = d.DoH()
                }).ToList()
            };

            File.WriteAllText(path, JsonSerializer.Serialize(file, options));
            return all.Count;
        }

        /// <summary>
        /// Adds the servers from the file. Entries whose name is already used (or reserved) or
        /// whose addresses are not valid are skipped.
        /// </summary>
        public static (int Added, int Skipped) Import(string path)
        {
            ListFile? file = JsonSerializer.Deserialize<ListFile>(File.ReadAllText(path), options);
            if (file == null)
                return (0, 0);

            int added = 0, skipped = 0;
            foreach (Entry e in file.Servers)
            {
                DNS dns = new DNS((e.Name ?? "").Trim(), (e.Dns1 ?? "").Trim(), (e.Dns2 ?? "").Trim(),
                                  (e.Dns1v6 ?? "").Trim(), (e.Dns2v6 ?? "").Trim(), (e.Doh ?? "").Trim());

                bool nameOk = dns.Name() != ""
                    && !string.Equals(dns.Name(), "Clear", StringComparison.OrdinalIgnoreCase)
                    && !DNS.NameTaken(dns.Name());

                if (nameOk && IsValidEntry(dns))
                {
                    dns.Save();
                    added++;
                }
                else
                {
                    skipped++;
                }
            }

            return (added, skipped);
        }
    }
}
