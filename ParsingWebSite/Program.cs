using log4net;
using ParsingWebSite.Classes;


ILog log = LogManager.GetLogger(typeof(Program));
ProgramLogging.ConfigureFileLogging();


try
{
    var dataTable = WebsiteParsing.GetLinks("https://www.wienerborse.at/en/bonds/");
    

    log.Info("Данные успешно получены и экспортированы");
}
catch (Exception ex)
{
    log.Error($"Ошибка: {ex.Message}");
    log.Debug("Детали ошибки:", ex);
}


Console.WriteLine("Проверьте файл логов в папке 'Logs'");
Console.ReadKey();
