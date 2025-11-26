using HtmlAgilityPack;
using log4net;

namespace ParsingWebSite.Classes
{
    internal static class WebsiteParsing
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Program));

        // Метод для загрузки сайта
        public static HtmlDocument GetDocument(string url)
        {
            try
            {

                log.Info("=============");
                log.Info("Парсинг сайта");
                log.Info("=============");
                HtmlWeb web = new HtmlWeb();
                HtmlDocument doc = web.Load(url);
                log.Info($"Страница загружена: {url}");

                return doc;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Проверьте подключение к интернету или правильность ссылки на сайт!");
                log.Error("Проверьте подключение к интернету или правильность ссылки на сайт! Подробнее: ", ex);
                throw new Exception("Проверьте подключение к интернету или правильность ссылки на сайт!");

            }
        }

        // Метод получения конкретных данных с сайта
        public static List<string> GetLinks(string url)
        {
            int countPage = 0;

            List<string> paginList = new List<string>();
            var dataLinks = new List<string>();
            HtmlDocument panigationDocument = GetDocument(url);
            log.Info("Получение количества страниц таблцы");

            var paginationFind = panigationDocument.DocumentNode.SelectNodes("//ul[contains(@class, 'pagination')]//li/a");
            if (paginationFind != null && paginationFind.Count() > 0)
            {
                foreach (var pagin in paginationFind)
                {
                    string textContent = pagin.InnerText;
                    if (!string.IsNullOrEmpty(textContent))
                    {

                        paginList.Add(textContent);
                        countPage = int.Parse(paginList.Max());
                    }
                }
            }
            log.Info($"Успешное получение количества страниц таблицы. Страниц: {countPage}");
            Console.WriteLine("Получение данных с сайта...");
            log.Debug("Получение данных со страницы");
            for (int i = 0; i < 10; i++) // На данный момент с сайта получается 10 страниц таблицы, чтобы получить все страницы нужно поменять на countPage
            {

                HtmlDocument doc = GetDocument(url + $"?c7928-page={i + 1}");
                var colth = doc.DocumentNode.SelectNodes(".//th");
                var col = doc.DocumentNode.SelectNodes(".//td");
                try
                {
                    // Заголовок вытягивается только тогда, когда происходит парсинг первой старницы таблицы

                    if (i + 1 == 1)
                    {
                        log.Info("Получение заголовка таблицы");
                        if (colth != null && colth.Count() > 0)
                        {
                            log.Info("Сохранение заголовка");
                            foreach (var column in colth)
                            {
                                string textContent = column.InnerText;
                                if (!string.IsNullOrEmpty(textContent))
                                {

                                    dataLinks.Add(textContent);
                                }
                            }
                        }
                    }


                    if (col != null && col.Count() > 0)
                    {
                        foreach (var column in col)
                        {
                            string textContent = column.InnerText;
                            if (!string.IsNullOrEmpty(textContent))
                            {

                                dataLinks.Add(textContent);
                            }
                        }
                    }
                    log.Debug($"Вытянуто {i + 1} страница из {countPage}");
                    Console.WriteLine($"Готово {i + 1} из {countPage}");

                }
                catch (Exception ex)
                {
                    log.Error("Данных нет. Подробнее: ", ex);
                    throw new Exception("Данные отсувствуют");
                }
            }
            log.Info("Успешное получение данных!");
            Console.WriteLine("Успешное получение данных!");
            return dataLinks;


        }
    }
}
