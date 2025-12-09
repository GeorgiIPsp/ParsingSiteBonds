using log4net;
using log4net.Appender;
using log4net.Layout;
using log4net.Repository.Hierarchy;

namespace ParsingWebSite.Classes
{
    internal static class ProgramLogging
    {
        /// <summary>
        /// Метод настройки создания логов
        /// </summary>
        public static void ConfigureFileLogging()
        {
            var hierarchy = (Hierarchy)LogManager.GetRepository();
            var fileAppender = new FileAppender();
            fileAppender.Name = "FileAppender";
            fileAppender.File = $"../../../Logs/logs-{DateTime.Now.ToShortDateString()}.log";

            fileAppender.AppendToFile = true;
            var patternLayout = new PatternLayout();
            patternLayout.ConversionPattern = "%date [%thread] %-5level - %message%newline";
            patternLayout.ActivateOptions();

            fileAppender.Layout = patternLayout;
            fileAppender.ActivateOptions();

            hierarchy.Root.AddAppender(fileAppender);
            hierarchy.Root.Level = log4net.Core.Level.All;
            hierarchy.Configured = true;
        }
    }
}
