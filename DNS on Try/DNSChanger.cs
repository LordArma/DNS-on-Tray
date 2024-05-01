using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Formats.Tar;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Data.Sqlite;

namespace DNS_on_Try
{
    public class DNS
    {
        private string dnsname;
        private string dns1;
        private string dns2;
        private string strLocalApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        private string dbName = "dnsontry.db";
        private string dbPath;
        private string tblName = "dnsTable";

        public DNS(string DNSName, string DNS1 = "8.8.8.8", string DNS2 = "4.2.2.4")
        {
            dnsname = DNSName;
            dns1 = DNS1;
            dns2 = DNS2;
            dbPath = strLocalApplicationData + "\\" + dbName;
            MakeDB();
        }

        private void MakeDB()
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
