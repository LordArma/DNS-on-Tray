using System;
using System.Collections.Generic;
using System.Formats.Tar;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Data.Sqlite;

namespace DNS_on_Try
{
    public class DNS(string DNSName, string DNS1="8.8.8.8", string DNS2="4.2.2.4")
    {
        private string dnsname = DNSName;
        private string dns1 = DNS1;
        private string dns2 = DNS2;
        string dbName = "dns.db";

        private void MakeDB()
        {
            if (!File.Exists(dbName))
            {
                File.WriteAllBytes(dbName, new byte[0]);

                using (SqliteConnection connection = new SqliteConnection("Data Source=" + dbName))
                {
                    connection.Open();
                    SqliteCommand sqliteCmd = new SqliteCommand();
                    sqliteCmd.Connection = connection;

                    sqliteCmd.CommandText = "CREATE TABLE IF NOT EXISTS dnsTable (dnsName VARCHAR(32) Primary Key, dns1 VARCHAR(32), dns2 VARCHAR(32))";
                    sqliteCmd.ExecuteNonQuery();
                }
            }
        }

        public bool Exist()
        {
            using (SqliteConnection connection = new SqliteConnection("Data Source=" + dbName))
            {
                connection.Open();
                SqliteCommand sqliteCmd = new SqliteCommand();
                sqliteCmd.Connection = connection;

                sqliteCmd.CommandText = $"SELECT count(*) FROM dnsTable WHERE dnsName='{dnsname}'";
                int count = Convert.ToInt32(sqliteCmd.ExecuteScalar());
                if (count > 0)
                    return true;

                return false;
            }
        }

        public void Save()
        {
            MakeDB();
            if (!Exist())
                using (SqliteConnection connection = new SqliteConnection("Data Source=" + dbName))
                {
                    connection.Open();
                    SqliteCommand sqliteCmd = new SqliteCommand();
                    sqliteCmd.Connection = connection;

                    sqliteCmd.CommandText = "INSERT INTO dnsTable VALUES (@dnsName, @dns1, @dns2);";
                    sqliteCmd.Parameters.AddWithValue("@dnsName", DNSName);
                    sqliteCmd.Parameters.AddWithValue("@dns1", DNS1);
                    sqliteCmd.Parameters.AddWithValue("@dns2", DNS2);

                    sqliteCmd.ExecuteNonQuery();
                }
        }

        public void Remove()
        {

        }
    }
}
