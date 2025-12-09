using CsvHelper;
using log4net;
using System.Globalization;

namespace ParsingWebSite.Classes
{
    internal static class WorkingWithCSVFile
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Program));
        /// <summary>
        /// Метод для преобразование в csv файл
        /// </summary>
        /// <param name="data"></param>
        public static void ExportToCSV(List<string> data)
        {
            log.Info("Создание файла");
            string nameFiles = $"{DateTime.Now.ToShortDateString()}.csv";
            int countColumn = 0;
            Console.WriteLine($"Создание файла {nameFiles}");
            log.Debug($"Создание файла {nameFiles}");
            try
            {
                // Изменен разделитель на табуляцию вместт запятой
                var config = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = "\t"
                };

                log.Debug($"Путь к файлу: ../../../Documents/{nameFiles}");
                log.Info("Запись в файл");
                using (var writer = new StreamWriter($"../../../Documents/{nameFiles}"))
                using (var csv = new CsvWriter(writer, config))
                {

                    foreach (var Data in data)
                    {

                        csv.WriteField(Data);
                        countColumn++;

                        if (countColumn == 9)
                        {
                            csv.NextRecord();
                            countColumn = 0;
                        }
                    }
                }
                Console.WriteLine("Файл успешно создан!");
                log.Info("Успешное создание файла");
            }
            catch (Exception ex)
            {
                log.Error("Ошибка:", ex);
                Console.WriteLine(ex);
            }
        }
    }
}
