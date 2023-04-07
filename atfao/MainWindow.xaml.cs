using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace atfao
{
    public partial class MainWindow : Window
    {
        private string connectionString = new atf_aoEntities().Database.Connection.ConnectionString;
        private DataTable dataTable = new DataTable();

        public MainWindow()
        {
            InitializeComponent();

            // Загружаем данные по умолчанию
            LoadData(DateTime.Today);
        }

        private void LoadData(DateTime date)
        {
            // Создаем SQL-запрос для получения данных за выбранную дату
            string sql = "SELECT [ID], [EventType], [ObjectName], [EventDateTime], [DirectionCode], [AccessPointPosition], [PassW26], [AccessPointID] FROM [AccessEvents] WHERE [EventDateTime] BETWEEN @startDate AND @endDate";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Создаем объект команды для выполнения запроса
                SqlCommand command = new SqlCommand(sql, connection);

                // Добавляем параметры для запроса
                command.Parameters.AddWithValue("@startDate", date.Date);
                command.Parameters.AddWithValue("@endDate", date.AddDays(1).Date);

                try
                {
                    // Открываем соединение и выполняем запрос
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    // Заполняем DataTable данными из результата запроса
                    dataTable.Clear();
                    dataTable.Load(reader);

                    // Устанавливаем источник данных для DataGrid
                    DataGrid.ItemsSource = dataTable.DefaultView;

                    // Закрываем ридер и соединение
                    reader.Close();
                    connection.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void Calendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedDate = (sender as Calendar).SelectedDate;
            if (selectedDate.HasValue)
            {
                var selectedDateString = selectedDate.Value.ToString("yyyy-MM-dd");
                var query = $"SELECT * FROM [atf_ao].[dbo].[AccessEvents] WHERE CONVERT(DATE, [EventDateTime]) = '{selectedDateString}'";
                var dataTable = new DataTable();

                using (var connection = new SqlConnection(connectionString))
                {
                    var adapter = new SqlDataAdapter(query, connection);


                    adapter.Fill(dataTable);
                }

                DataGrid.ItemsSource = dataTable.DefaultView;
            }
        }

    }
}
