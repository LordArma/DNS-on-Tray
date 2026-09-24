using Microsoft.Data.Sqlite;
using static DNS_on_Tray.Helper;

namespace DNS_on_Tray
{
    public class DNS
    {
        private string dnsname;
        private string dns1;
        private string dns2;
        private string dns1v6;
        private string dns2v6;
        private string doh;
        private static string dbName = "dnsontry.db";
        private static string dbPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\" + dbName;
        private static string tblName = "dnsTable";
        private static bool dbReady = false;

        // Columns added after the first release; older databases are migrated in MakeDB.
        private static readonly string[] addedColumns = { "dns1v6", "dns2v6", "doh" };

        /// <summary>
        /// A DNS entry. Only <paramref name="DNS1"/> is required; the others may be empty.
        /// <paramref name="DoH"/> is a DNS-over-HTTPS template (e.g. https://cloudflare-dns.com/dns-query).
        /// </summary>
        public DNS(string DNSName, string DNS1, string DNS2, string DNS1v6 = "", string DNS2v6 = "", string DoH = "")
        {
            dnsname = DNSName;
            dns1 = DNS1;
            dns2 = DNS2;
            dns1v6 = DNS1v6;
            dns2v6 = DNS2v6;
            doh = DoH;
        }

        private static SqliteConnection Open()
        {
            MakeDB();

            SqliteConnection connection = new SqliteConnection($"Data Source={dbPath}");
            connection.Open();
            return connection;
        }

        private static DNS Read(SqliteDataReader r)
        {
            return new DNS((string)r["dnsName"], (string)r["dns1"], (string)r["dns2"],
                           (string)r["dns1v6"], (string)r["dns2v6"], (string)r["doh"]);
        }

        /// <summary>
        /// Loads a saved DNS by name, or returns null when there is no such entry.
        /// </summary>
        public static DNS? Find(string name)
        {
            using SqliteConnection connection = Open();
            using SqliteCommand fmd = connection.CreateCommand();

            fmd.CommandText = $"SELECT * FROM {tblName} WHERE dnsName=@dnsName";
            fmd.Parameters.AddWithValue("@dnsName", name);

            using SqliteDataReader r = fmd.ExecuteReader();
            return r.Read() ? Read(r) : null;
        }

        /// <summary>
        /// True when a saved DNS already uses this name, ignoring case.
        /// </summary>
        public static bool NameTaken(string name)
        {
            using SqliteConnection connection = Open();
            using SqliteCommand sqliteCmd = connection.CreateCommand();

            sqliteCmd.CommandText = $"SELECT count(*) FROM {tblName} WHERE dnsName=@dnsName COLLATE NOCASE";
            sqliteCmd.Parameters.AddWithValue("@dnsName", name);
            return Convert.ToInt32(sqliteCmd.ExecuteScalar()) > 0;
        }

        public static List<DNS> All()
        {
            List<DNS> dns = new List<DNS>();

            using SqliteConnection connection = Open();
            using SqliteCommand fmd = connection.CreateCommand();

            fmd.CommandText = $"SELECT * FROM {tblName}";
            using SqliteDataReader r = fmd.ExecuteReader();
            while (r.Read())
                dns.Add(Read(r));

            return dns;
        }

        public string Name()
        {
            return dnsname;
        }

        public string DNS1()
        {
            return dns1;
        }

        public string DNS2()
        {
            return dns2;
        }

        public string DNS1v6()
        {
            return dns1v6;
        }

        public string DNS2v6()
        {
            return dns2v6;
        }

        public string DoH()
        {
            return doh;
        }

        /// <summary>
        /// The IPv4 servers in order; DNS2 is optional and left out when empty.
        /// </summary>
        public List<string> IPv4Servers()
        {
            return new[] { dns1, dns2 }.Where(s => s != "").ToList();
        }

        /// <summary>
        /// The IPv6 servers in order (possibly none).
        /// </summary>
        public List<string> IPv6Servers()
        {
            return new[] { dns1v6, dns2v6 }.Where(s => s != "").ToList();
        }

        /// <summary>
        /// Every configured server, IPv4 first.
        /// </summary>
        public List<string> Servers()
        {
            return IPv4Servers().Concat(IPv6Servers()).ToList();
        }

        /// <summary>
        /// Servers as display text, e.g. "1.1.1.1, 1.0.0.1".
        /// </summary>
        public string ServersText()
        {
            return string.Join(", ", IPv4Servers());
        }

        /// <summary>
        /// Makes sure the table exists with every column, seeding the default servers when it is
        /// created. A file that is not a valid database is kept as a .bak copy and replaced.
        /// </summary>
        private static void MakeDB()
        {
            if (dbReady)
                return;

            bool created;
            try
            {
                created = CreateTable();
            }
            catch (SqliteException)
            {
                SqliteConnection.ClearAllPools();
                File.Move(dbPath, $"{dbPath}.{DateTime.Now:yyyyMMddHHmmss}.bak");
                created = CreateTable();
            }

            dbReady = true;

            if (created)
                AddPopularDNS();
        }

        private static bool CreateTable()
        {
            using SqliteConnection connection = new SqliteConnection($"Data Source={dbPath}");
            connection.Open();
            using SqliteCommand sqliteCmd = connection.CreateCommand();

            sqliteCmd.CommandText = $"SELECT count(*) FROM sqlite_master WHERE type='table' AND name='{tblName}'";
            bool exists = Convert.ToInt32(sqliteCmd.ExecuteScalar()) > 0;

            if (!exists)
            {
                sqliteCmd.CommandText = $"CREATE TABLE IF NOT EXISTS {tblName} (dnsName VARCHAR(32) Primary Key, dns1 VARCHAR(32), dns2 VARCHAR(32))";
                sqliteCmd.ExecuteNonQuery();
            }

            sqliteCmd.CommandText = $"SELECT name FROM pragma_table_info('{tblName}')";
            HashSet<string> columns = new HashSet<string>();
            using (SqliteDataReader r = sqliteCmd.ExecuteReader())
            {
                while (r.Read())
                    columns.Add(r.GetString(0));
            }

            bool migrated = false;
            foreach (string column in addedColumns.Where(c => !columns.Contains(c)))
            {
                sqliteCmd.CommandText = $"ALTER TABLE {tblName} ADD COLUMN {column} VARCHAR(256) NOT NULL DEFAULT ''";
                sqliteCmd.ExecuteNonQuery();
                migrated = true;
            }

            // Fill the new IPv6/DoH fields for default servers saved by older versions.
            if (exists && migrated)
            {
                foreach (DNS known in PopularDNS().Where(d => d.IPv6Servers().Count > 0 || d.DoH() != ""))
                {
                    sqliteCmd.CommandText = $"UPDATE {tblName} SET dns1v6=@dns1v6, dns2v6=@dns2v6, doh=@doh WHERE dns1=@dns1 AND dns2=@dns2";
                    sqliteCmd.Parameters.Clear();
                    sqliteCmd.Parameters.AddWithValue("@dns1v6", known.dns1v6);
                    sqliteCmd.Parameters.AddWithValue("@dns2v6", known.dns2v6);
                    sqliteCmd.Parameters.AddWithValue("@doh", known.doh);
                    sqliteCmd.Parameters.AddWithValue("@dns1", known.dns1);
                    sqliteCmd.Parameters.AddWithValue("@dns2", known.dns2);
                    sqliteCmd.ExecuteNonQuery();
                }
            }

            return !exists;
        }

        public bool Exist()
        {
            using SqliteConnection connection = Open();
            using SqliteCommand sqliteCmd = connection.CreateCommand();

            sqliteCmd.CommandText = $"SELECT count(*) FROM {tblName} WHERE dnsName=@dnsName";
            sqliteCmd.Parameters.AddWithValue("@dnsName", dnsname);
            return Convert.ToInt32(sqliteCmd.ExecuteScalar()) > 0;
        }

        private void AddParameters(SqliteCommand sqliteCmd)
        {
            sqliteCmd.Parameters.AddWithValue("@dnsName", dnsname);
            sqliteCmd.Parameters.AddWithValue("@dns1", dns1);
            sqliteCmd.Parameters.AddWithValue("@dns2", dns2);
            sqliteCmd.Parameters.AddWithValue("@dns1v6", dns1v6);
            sqliteCmd.Parameters.AddWithValue("@dns2v6", dns2v6);
            sqliteCmd.Parameters.AddWithValue("@doh", doh);
        }

        public void Save()
        {
            if (Exist())
                return;

            using SqliteConnection connection = Open();
            using SqliteCommand sqliteCmd = connection.CreateCommand();

            sqliteCmd.CommandText = $"INSERT INTO {tblName} (dnsName, dns1, dns2, dns1v6, dns2v6, doh) VALUES (@dnsName, @dns1, @dns2, @dns1v6, @dns2v6, @doh);";
            AddParameters(sqliteCmd);
            sqliteCmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Replaces the saved entry called <paramref name="oldName"/> with this one (the name may change).
        /// </summary>
        public void Update(string oldName)
        {
            using SqliteConnection connection = Open();
            using SqliteCommand sqliteCmd = connection.CreateCommand();

            sqliteCmd.CommandText = $"UPDATE {tblName} SET dnsName=@dnsName, dns1=@dns1, dns2=@dns2, dns1v6=@dns1v6, dns2v6=@dns2v6, doh=@doh WHERE dnsName=@oldName";
            AddParameters(sqliteCmd);
            sqliteCmd.Parameters.AddWithValue("@oldName", oldName);
            sqliteCmd.ExecuteNonQuery();
        }

        public void Remove()
        {
            using SqliteConnection connection = Open();
            using SqliteCommand sqliteCmd = connection.CreateCommand();

            sqliteCmd.CommandText = $"DELETE FROM {tblName} WHERE dnsName=@dnsName";
            sqliteCmd.Parameters.AddWithValue("@dnsName", dnsname);
            sqliteCmd.ExecuteNonQuery();
        }
    }
}
