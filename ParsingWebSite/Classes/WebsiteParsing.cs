using HtmlAgilityPack;
using log4net;
using System.Collections.Concurrent;
using System.Security.Policy;

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

        public static int GetCountPage(string url)
        {
            int countPage = 0;
            List<string> paginList = new List<string>();
            

            HtmlDocument panigationDocument = GetDocument(url);
            log.Info("Получение количества страниц таблицы");

            var paginationFind = panigationDocument.DocumentNode.SelectNodes("//ul[contains(@class, 'pagination')]//li/a");
            if (paginationFind != null && paginationFind.Count > 0)
            {
                foreach (var pagin in paginationFind)
                {
                    string textContent = pagin.InnerText;
                    if (!string.IsNullOrEmpty(textContent))
                    {
                        paginList.Add(textContent);
                        if (int.TryParse(textContent, out int pageNum))
                        {
                            countPage = Math.Max(countPage, pageNum);
                        }
                    }
                }
            }
            return countPage;
        }

        // Метод получения конкретных данных с сайта
        public static List<string> GetLinks(string url)
        {
            int countPage = GetCountPage(url);
            var dataLinks = new List<string>();

            log.Info($"Успешное получение количества страниц таблицы. Страниц: {countPage}");
            Console.WriteLine("Получение данных с сайта...");
            log.Debug("Получение данных со страницы");

            int countPageReady = 0;
            var pageResults = new List<string>[countPage];

            Parallel.For(0, countPage, new ParallelOptions { MaxDegreeOfParallelism = 30 }, i =>
            {
                Random next = new Random();
                int randomNumber = next.Next(1000, 5000);
                System.Threading.Thread.Sleep(randomNumber);
                log.Debug($"Задержка для следующего парсинга страниц: {randomNumber}");

                try
                {
                    HtmlDocument doc = GetDocument(url + $"?c7928-page={i + 1}");
                    var colth = doc.DocumentNode.SelectNodes(".//th");
                    var col = doc.DocumentNode.SelectNodes(".//td");

                    List<string> pageData = new List<string>();

                    // Заголовок вытягивается только для первой страницы
                    if (i == 0)
                    {
                        log.Info("Получение заголовка таблицы");
                        if (colth != null && colth.Count > 0)
                        {
                            log.Info("Сохранение заголовка");
                            foreach (var column in colth)
                            {
                                string textContent = column.InnerText;
                                if (!string.IsNullOrEmpty(textContent))
                                {
                                    pageData.Add(textContent);
                                }
                            }
                        }
                    }

                    if (col != null && col.Count > 0)
                    {
                        Interlocked.Increment(ref countPageReady);
                        foreach (var column in col)
                        {
                            string textContent = column.InnerText;
                            if (!string.IsNullOrEmpty(textContent))
                            {
                                pageData.Add(textContent);
                            }
                        }
                    }

                    // Сохраняем данные в массив по индексу - это гарантирует порядок
                    pageResults[i] = pageData;
                    log.Debug($"Вытянута {i + 1} страница из {countPage}");
                    Console.WriteLine($"Готово {countPageReady} из {countPage}");
                }
                catch (Exception ex)
                {
                    log.Error($"Ошибка на странице {i + 1}. Подробнее: ", ex);
                    Console.WriteLine($"Ошибка на странице {i + 1}");
                }
            });

            foreach (var pageData in pageResults)
            {
                if (pageData != null && pageData.Count > 0)
                {
                    dataLinks.AddRange(pageData);
                }
            }

            log.Info("Успешное получение данных!");
            Console.WriteLine("Успешное получение данных!");
            return dataLinks.ToList();
        }

    }
}