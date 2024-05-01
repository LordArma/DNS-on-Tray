using System;
using System.Collections.Generic;
using System.Formats.Tar;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.VisualBasic.Devices;

namespace DNS_on_Try
{
    public class DNS(string DNSName, string DNS1="8.8.8.8", string DNS2="4.2.2.4")
    {
        private string dnsname = DNSName;
        private string dns1 = DNS1;
        private string dns2 = DNS2;
        private string dbName = "dns.db";

        private void MakeDB()
        {
            if (!File.Exists(dbName))
            {
                File.WriteAllBytes(dbName, new byte[0]);

                using (var connection = new SqliteConnection("Data Source=" + dbName))
                {
                    connection.Open();
                    SqliteCommand sqliteCmd = connection.CreateCommand();
                    sqliteCmd.CommandText = "CREATE TABLE IF NOT EXISTS dnsTable (dnsName VARCHAR(32) Primary Key, dns1 VARCHAR(32), dns2 VARCHAR(32))";
                    sqliteCmd.ExecuteNonQuery();
                }
            }
        }

        public void Save()
        {
            using (SqliteConnection db = new SqliteConnection($"Filename={dbName}"))
            {
                db.Open();

                SqliteCommand cmd = new SqliteCommand();
                cmd.Connection = db;

                cmd.CommandText = "INSERT INTO dnsTable VALUES (@dnsName, @dns1, @dns2);";
                cmd.Parameters.AddWithValue("@dnsName", DNSName);
                cmd.Parameters.AddWithValue("@dns1", DNS1);
                cmd.Parameters.AddWithValue("@dns2", DNS2);

                cmd.ExecuteScalar();
            }
        }

        public void Remove()
        {

        }
    }
}
