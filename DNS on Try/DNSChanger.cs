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

        public DNS(string DNSName, string DNS1 = "8.8.8.8", string DNS2 = "4.2.2.4")
        {
            dnsname = DNSName;
            dns1 = DNS1;
            dns2 = DNS2;
            MakeDB();

            if (Exist())
            {
                using (SqliteConnection connection = new SqliteConnection($"Data Source={dbPath}"))
                {
                    connection.Open();
                    using (SqliteCommand fmd = connection.CreateCommand())
                    {
                        fmd.CommandText = $"SELECT * FROM {tblName} Where dnsName='{dnsname}'";
                        SqliteDataReader r = fmd.ExecuteReader();
                        while (r.Read())
                        {
                            dns1 = (string)r["dns1"];
                            dns2 = (string)r["dns2"];
                        }

                        connection.Close();
                    }
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
                    SqliteDataReader r = fmd.ExecuteReader();
                    while (r.Read())
                    {
                        string dnsName = (string)r["dnsName"];
                        string dns1 = (string)r["dns1"];
                        string dns2 = (string)r["dns2"];

                        dns.Add(new DNS(dnsName, dns1, dns2));
                    }

                    connection.Close();
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

        private static void MakeDB()
        {
            if (!File.Exists(dbPath))
            {
                File.WriteAllBytes(dbPath, new byte[0]);

                using (SqliteConnection connection = new SqliteConnection($"Data Source={dbPath}"))
                {
                    connection.Open();
                    SqliteCommand sqliteCmd = new SqliteCommand();
                    sqliteCmd.Connection = connection;

                    sqliteCmd.CommandText = $"CREATE TABLE IF NOT EXISTS {tblName} (dnsName VARCHAR(32) Primary Key, dns1 VARCHAR(32), dns2 VARCHAR(32))";
                    sqliteCmd.ExecuteNonQuery();
                }

                AddPopularDNS();
            }
        }

        public bool Exist()
        {
            using (SqliteConnection connection = new SqliteConnection($"Data Source={dbPath}"))
            {
                connection.Open();
                SqliteCommand sqliteCmd = new SqliteCommand();
                sqliteCmd.Connection = connection;

                sqliteCmd.CommandText = $"SELECT count(*) FROM {tblName} WHERE dnsName='{dnsname}'";
                int count = Convert.ToInt32(sqliteCmd.ExecuteScalar());
                if (count > 0)
                    return true;

                return false;
            }
        }

        public void Save()
        {
            if (!Exist())
                using (SqliteConnection connection = new SqliteConnection($"Data Source={dbPath}"))
                {
                    connection.Open();
                    SqliteCommand sqliteCmd = new SqliteCommand();
                    sqliteCmd.Connection = connection;

                    sqliteCmd.CommandText = $"INSERT INTO {tblName} VALUES (@dnsName, @dns1, @dns2);";
                    sqliteCmd.Parameters.AddWithValue("@dnsName", dnsname);
                    sqliteCmd.Parameters.AddWithValue("@dns1", dns1);
                    sqliteCmd.Parameters.AddWithValue("@dns2", dns2);

                    sqliteCmd.ExecuteNonQuery();
                }
        }

        public void Remove()
        {
            if (Exist())
                using (SqliteConnection connection = new SqliteConnection($"Data Source={dbPath}"))
                {
                    connection.Open();
                    SqliteCommand sqliteCmd = new SqliteCommand();
                    sqliteCmd.Connection = connection;

                    sqliteCmd.CommandText = $"DELETE FROM {tblName} WHERE dnsName='{dnsname}'";

                    sqliteCmd.ExecuteNonQuery();
                }
        }
    }
}
