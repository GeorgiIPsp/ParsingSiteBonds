using log4net;
using ParsingWebSite.Classes;


ILog log = LogManager.GetLogger(typeof(Program));
ProgramLogging.ConfigureFileLogging();

log.Debug("Приложение запущено");
log.Info("Начало работы");

try
{
    var dataTable = WebsiteParsing.GetLinks("https://www.wienerborse.at/en/bonds/");
    WorkingWithCSVFile.ExportToCSV(dataTable);

    log.Info("Данные успешно получены и экспортированы");
}
catch (Exception ex)
{
    log.Error($"Ошибка: {ex.Message}");
    log.Debug("Детали ошибки:", ex);
}

log.Info("Приложение завершено");
Console.WriteLine("Проверьте файл логов в папке 'Logs'");
Console.ReadKey();
