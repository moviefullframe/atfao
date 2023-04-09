using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Data;


namespace atfao
{
    public class DatabaseManager
    {

        private readonly string connectionString;

        public DatabaseManager(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public DataTable LoadData(string date)
        {
            string sql = "SELECT [ID], [EventType], [ObjectName], [EventDateTime], [DirectionCode], [AccessPointPosition], [PassW26], [AccessPointID], [Time] FROM [AccessEvents] WHERE [EventDateTime] = @date";
            DataTable dataTable = new DataTable();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@date", DateTime.Parse(date).Date);

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    dataTable.Load(reader);

                    reader.Close();
                    connection.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

            return dataTable;
        }
    }
}

