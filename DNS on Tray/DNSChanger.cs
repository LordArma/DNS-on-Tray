using Microsoft.Data.Sqlite;
using static DNS_on_Tray.Helper;

namespace DNS_on_Tray
{
    public class DNS
    {
        private string dnsname;
        private string dns1;
        private string dns2;
        private static string dbName = "dnsontry.db";
        private static string dbPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\" + dbName;
        private static string tblName = "dnsTable";
        private static bool dbReady = false;

        public DNS(string DNSName, string DNS1, string DNS2)
        {
            dnsname = DNSName;
            dns1 = DNS1;
            dns2 = DNS2;
        }

        /// <summary>
        /// Loads a saved DNS by name, or returns null when there is no such entry.
        /// </summary>
        public static DNS? Find(string name)
        {
            MakeDB();

            using (SqliteConnection connection = new SqliteConnection($"Data Source={dbPath}"))
            {
                connection.Open();
                using (SqliteCommand fmd = connection.CreateCommand())
                {
                    fmd.CommandText = $"SELECT dns1, dns2 FROM {tblName} WHERE dnsName=@dnsName";
                    fmd.Parameters.AddWithValue("@dnsName", name);
                    using (SqliteDataReader r = fmd.ExecuteReader())
                    {
                        if (r.Read())
                            return new DNS(name, (string)r["dns1"], (string)r["dns2"]);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// True when a saved DNS already uses this name, ignoring case.
        /// </summary>
        public static bool NameTaken(string name)
        {
            MakeDB();

            using (SqliteConnection connection = new SqliteConnection($"Data Source={dbPath}"))
            {
                connection.Open();
                using (SqliteCommand sqliteCmd = connection.CreateCommand())
                {
                    sqliteCmd.CommandText = $"SELECT count(*) FROM {tblName} WHERE dnsName=@dnsName COLLATE NOCASE";
                    sqliteCmd.Parameters.AddWithValue("@dnsName", name);
                    return Convert.ToInt32(sqliteCmd.ExecuteScalar()) > 0;
                }
            }
        }

        public static List<DNS> All()
        {
            MakeDB();
            List<DNS> dns = new List<DNS>();

            using (SqliteConnection connection = new SqliteConnection($"Data Source={dbPath}"))
            {
                connection.Open();
                using (SqliteCommand fmd = connection.CreateCommand())
                {
                    fmd.CommandText = $"SELECT * FROM {tblName}";
                    using (SqliteDataReader r = fmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            string dnsName = (string)r["dnsName"];
                            string dns1 = (string)r["dns1"];
                            string dns2 = (string)r["dns2"];

                            dns.Add(new DNS(dnsName, dns1, dns2));
                        }
                    }
                }
            }

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

        /// <summary>
        /// Makes sure the table exists, seeding the default servers when it is created.
        /// A file that is not a valid database is kept as a .bak copy and replaced.
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
            using (SqliteConnection connection = new SqliteConnection($"Data Source={dbPath}"))
            {
                connection.Open();
                using (SqliteCommand sqliteCmd = connection.CreateCommand())
                {
                    sqliteCmd.CommandText = $"SELECT count(*) FROM sqlite_master WHERE type='table' AND name='{tblName}'";
                    bool exists = Convert.ToInt32(sqliteCmd.ExecuteScalar()) > 0;
                    if (exists)
                        return false;

                    sqliteCmd.CommandText = $"CREATE TABLE IF NOT EXISTS {tblName} (dnsName VARCHAR(32) Primary Key, dns1 VARCHAR(32), dns2 VARCHAR(32))";
                    sqliteCmd.ExecuteNonQuery();
                    return true;
                }
            }
        }

        public bool Exist()
        {
            MakeDB();

            using (SqliteConnection connection = new SqliteConnection($"Data Source={dbPath}"))
            {
                connection.Open();
                using (SqliteCommand sqliteCmd = connection.CreateCommand())
                {
                    sqliteCmd.CommandText = $"SELECT count(*) FROM {tblName} WHERE dnsName=@dnsName";
                    sqliteCmd.Parameters.AddWithValue("@dnsName", dnsname);
                    return Convert.ToInt32(sqliteCmd.ExecuteScalar()) > 0;
                }
            }
        }

        public void Save()
        {
            if (!Exist())
                using (SqliteConnection connection = new SqliteConnection($"Data Source={dbPath}"))
                {
                    connection.Open();
                    using (SqliteCommand sqliteCmd = connection.CreateCommand())
                    {
                        sqliteCmd.CommandText = $"INSERT INTO {tblName} VALUES (@dnsName, @dns1, @dns2);";
                        sqliteCmd.Parameters.AddWithValue("@dnsName", dnsname);
                        sqliteCmd.Parameters.AddWithValue("@dns1", dns1);
                        sqliteCmd.Parameters.AddWithValue("@dns2", dns2);

                        sqliteCmd.ExecuteNonQuery();
                    }
                }
        }

        public void Remove()
        {
            if (Exist())
                using (SqliteConnection connection = new SqliteConnection($"Data Source={dbPath}"))
                {
                    connection.Open();
                    using (SqliteCommand sqliteCmd = connection.CreateCommand())
                    {
                        sqliteCmd.CommandText = $"DELETE FROM {tblName} WHERE dnsName=@dnsName";
                        sqliteCmd.Parameters.AddWithValue("@dnsName", dnsname);

                        sqliteCmd.ExecuteNonQuery();
                    }
                }
        }
    }
}
