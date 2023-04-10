using ClosedXML.Excel;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Windows;
using Microsoft.Win32;
using System.Linq;
using System.Windows.Controls;

namespace atfao
{
    public partial class MainWindow : Window
    {
        private bool isLunchTime = false;

        private string connectionString = new atf_aoEntities1().Database.Connection.ConnectionString;
        private DataTable dataTable = new DataTable();

        public MainWindow()
        {
            InitializeComponent();

        }

        private void LoadData(string date)
        {
            DatabaseManager dbManager = new DatabaseManager(connectionString);
            dataTable = dbManager.LoadData(date);
            Console.WriteLine($"Loaded {dataTable.Rows.Count} rows from the database");
            DataGrid.ItemsSource = dataTable.DefaultView;
        }

        private void LoadDataButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedDate = DatePicker.SelectedDate;
            if (selectedDate.HasValue)
            {
                string formattedDate = selectedDate.Value.ToString("dd.MM.yyyy");
                LoadData(formattedDate);
            }
        }


        private void Report_Click(object sender, RoutedEventArgs e)
        {
            TimeSpan workStartTime = new TimeSpan(9, 0, 0);
            var reportData = new DataTable();
            reportData.Columns.AddRange(new[] { new DataColumn("ID"), new DataColumn("Cтатус"), new DataColumn("Потери", typeof(TimeSpan)) });

            TimeSpan totalLunchTime = TimeSpan.Zero;

            foreach (DataRow row in dataTable.Rows)
            {
                TimeSpan lunchTime = TimeSpan.Zero;
                if (!DateTime.TryParseExact(row["EventDateTime"].ToString(), "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime eventDate))
                {
                    MessageBox.Show($"Некорректный формат даты для строки с ID {row["ID"]}. Пропускаем эту строку.");
                    continue;
                }

                if (row["Time"] == DBNull.Value)
                {
                    MessageBox.Show($"Время не указано для строки с ID {row["ID"]}. Пропускаем эту строку.");
                    continue;
                }

                if (!TimeSpan.TryParse(row["Time"].ToString(), out TimeSpan arrivalTime))
                {
                    MessageBox.Show($"Некорректный формат времени для строки с ID {row["ID"]}. Пропускаем эту строку.");

                    continue;
                }

                if (arrivalTime >= new TimeSpan(13, 0, 0) && arrivalTime < new TimeSpan(14, 0, 0))
                {
                    continue;
                }

                TimeSpan lateTime = arrivalTime > workStartTime ? arrivalTime - workStartTime : TimeSpan.Zero;
                if (isLunchTime && arrivalTime >= new TimeSpan(14, 0, 0))
                {
                    lateTime -= new TimeSpan(1, 0, 0);
                }


                reportData.Rows.Add(row["ID"], lateTime > TimeSpan.Zero ? "Опоздание" : "Нет опоздания", lateTime);
            }


            // Генерация Excel-отчета с заголовком, графиками и системой опозданий на основе DataTable reportData
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Report");

                // Задаем стиль для заголовка "Отчеты о проходах"
                var titleStyle = worksheet.Style;
                titleStyle.Font.FontName = "Helvetica";
                titleStyle.Font.FontSize = 20;
                titleStyle.Font.Bold = true;
                titleStyle.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                titleStyle.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                // Выводим заголовок "Отчеты о проходах" на ячейку A1 и объединяем ячейки с A1 по G1
                worksheet.Cell("A1").Value = "Отчеты о проходах";
                worksheet.Range("A1:G1").Merge();
                worksheet.Cell("A1").Style = titleStyle;

                // Задаем стиль для названия организации "АО Архангельский траловый флот"
                var orgNameStyle = worksheet.Style;
                orgNameStyle.Font.FontName = "Helvetica";
                orgNameStyle.Font.FontSize = 16;
                orgNameStyle.Font.Bold = true;
                orgNameStyle.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Выводим название организации на ячейку A2 и объединяем ячейки с A2 по G2
                worksheet.Cell("A2").Value = "АО Архангельский Траловый Флот";
                worksheet.Range("A2:G2").Merge();
                worksheet.Cell("A2").Style = orgNameStyle;

                // Выводим названия столбцов DataTable reportData на строку 4
                var columnHeaders = reportData.Columns.Cast<DataColumn>().ToList();
                for (int i = 0; i < columnHeaders.Count; i++)
                {
                    worksheet.Cell(4, i + 1).Value = columnHeaders[i].ColumnName;
                    worksheet.Cell(4, i + 1).Style.Font.FontName = "Helvetica";
                    worksheet.Cell(4, i + 1).Style.Font.Bold = true;
                    worksheet.Cell(4, i + 1).Style.Fill.BackgroundColor = XLColor.LightSkyBlue;
                    worksheet.Cell(4, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(4, i + 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                }

                // Задаем стиль для ячеек таблицы
                worksheet.Cell("A4").InsertTable(reportData).Theme = XLTableTheme.TableStyleMedium2;
                
                var tableStyle = worksheet.Table("Table1").Style;
                tableStyle.Font.FontName = "Helvetica";
                tableStyle.Font.FontSize = 10;
                tableStyle.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                tableStyle.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                       
                            // Выводим данные DataTable reportData на ячейки таблицы
                            for (int i = 0; i < reportData.Rows.Count; i++)
                            {
                                var currentRow = reportData.Rows[i];

                                // Добавляем ячейки для каждого столбца DataTable reportData
                                for (int j = 0; j < reportData.Columns.Count; j++)
                                {
                                    var currentCell = worksheet.Cell(i + 5, j + 1);

                                    if (currentRow[j] is TimeSpan timeSpanValue)
                                    {
                                        // Если значение типа TimeSpan, выводим его в формате чч:мм:сс
                                        currentCell.Value = timeSpanValue.ToString(@"hh\:mm\:ss");
                                    }
                                    else
                                    {
                                        // Иначе выводим значение строки
                                        currentCell.Value = Convert.ToString(currentRow[j]);
                                    }

                                    // Задаем стиль ячеек
                                    currentCell.Style.Font.FontName = "Helvetica";
                                    currentCell.Style.Font.FontSize = 10;
                                    currentCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                    currentCell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                                    // Выделяем время опоздания светло-голубым цветом
                                    if (reportData.Columns[j].ColumnName == "Потери")
                                    {
                                        currentCell.Style.Fill.BackgroundColor = XLColor.LightSkyBlue;
                                    }
                                }
                                // Автоматически подбираем ширину столбцов
                                worksheet.Columns().AdjustToContents();

                                // Сохраняем Excel-отчет
                                
                            }


                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Excel files (.xlsx)|.xlsx";
                saveFileDialog.DefaultExt = "xlsx";
                saveFileDialog.FileName = "TardinessReport.xlsx";
                bool? result = saveFileDialog.ShowDialog();
                if (result == true)
                {
                    string fileName = saveFileDialog.FileName;
                    workbook.SaveAs(fileName);
                    MessageBox.Show($"Excel-отчет сохранен в {fileName}");
                }

                MessageBox.Show("Excel-отчет успешно создан.");



            }
        }
        
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            isLunchTime = ((CheckBox)sender).IsChecked ?? false;
            LoadData(DatePicker.SelectedDate?.ToString("dd.MM.yyyy"));
        }
    }
 }
       