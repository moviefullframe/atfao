using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace atfao
{
    public partial class MainWindow : Window
    {
        private atf_aoEntities db;

        public MainWindow()
        {
            InitializeComponent();
            db = new atf_aoEntities();

            // Устанавливаем источник данных для DataGrid
            DataGrid.ItemsSource = db.AccessEvents.ToList();
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            // Обработчик события нажатия кнопки "Экспорт".
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Обработчик события изменения выбранной строки в DataGrid.
        }
    }
}
