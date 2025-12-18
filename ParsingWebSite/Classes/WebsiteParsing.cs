using HtmlAgilityPack;
using log4net;
using System.Collections.Concurrent;
using System.Data;
using System.Net;
using System.Security.Policy;

namespace ParsingWebSite.Classes
{
    internal static class WebsiteParsing
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Program));

        /// <summary>
        /// Метод для загрузки сайта
        /// </summary> 
        /// <param name="url"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static HtmlDocument GetDocument(string url)
        {
            log.Info($"Попытка загрузки страницы. URL: {url}");

            try
            {
                HtmlWeb web = new HtmlWeb();
                log.Debug($"Параметры запроса: UserAgent={web.UserAgent}, Timeout={web.Timeout}");

                log.Info($"Начало загрузки страницы: {url}");
                HtmlDocument doc = web.Load(url);
                log.Info($"Страница успешно загружена. URL: {url}");

                return doc;
            }
            catch (WebException webEx)
            {
                log.Error($"Ошибка WebException при загрузке страницы. URL: {url}, Status: {webEx.Status}");
                Console.WriteLine($"Ошибка при загрузке страницы {url}. Проверьте подключение к интернету!");
                throw new Exception($"Не удалось загрузить страницу {url}. Проверьте подключение к интернету.");
            }
            catch (UriFormatException uriEx)
            {
                log.Error($"Некорректный URL. URL: {url}, Ошибка: {uriEx.Message}", uriEx);
                Console.WriteLine($"Некорректный URL: {url}");
                throw new Exception($"Некорректный URL: {url}", uriEx);
            }
            catch (Exception ex)
            {
                log.Error($"Неизвестная ошибка при загрузке страницы. URL: {url}", ex);
                Console.WriteLine($"Ошибка при обработке страницы {url}.");
                throw new Exception($"Ошибка при обработке страницы {url}. Подробности в логе.", ex);
            }
        }
        /// <summary>
        /// Метод для получения количества страниц таблицы
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static int GetCountPage(string url)
        {
            log.Info($"Начинаем получение количества страниц таблицы. URL: {url}");

            int countPage = 0;
            List<string> paginList = new List<string>();

            try
            {
                log.Info($"Загрузка документа...");
                HtmlDocument panigationDocument = GetDocument(url);

                var paginationFind = panigationDocument.DocumentNode.SelectNodes("//ul[contains(@class, 'pagination')]//li/a");
                log.Info("Документ загружен");
                if (paginationFind != null && paginationFind.Count > 0)
                {
                    log.Debug($"Обработка найденных элементов пагинации");
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
                else
                {
                    log.Warn($"Элементы пагинации не найдены на странице: {url}");
                }

                log.Info($"Получение количества страниц завершено. URL: {url}, Страниц: {countPage}");
                return countPage;
            }
            catch (Exception ex)
            {
                log.Error($"Ошибка при получении количества страниц. URL: {url}", ex);
                throw;
            }
        }

        /// <summary>
        /// Метод получения конкретных данных с сайта
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static List<string> GetLinks(string url)
        {
            log.Info($"Начинаем получение данных с сайта. URL: {url}");

            var dataLinks = new List<string>();

            try
            {
                log.Info($"Определение количества страниц для парсинга: {url}");
                int countPage = GetCountPage(url);

                log.Info($"Количество страниц для обработки: {countPage}, URL: {url}");
                Console.WriteLine($"Получение данных с сайта {url}...");

                int countPageReady = 0;
                var pageResults = new List<string>[countPage];
                bool dataTrue = false;
                int countCol = 0;

                log.Info($"Старт параллельного парсинга {countPage} страниц с сайта: {url}");
                log.Debug($"Параллельно парсинг производится 30 страниц");

                Parallel.For(0, countPage, new ParallelOptions { MaxDegreeOfParallelism = 30 }, i =>
                {
                    int pageNumber = i + 1;
                    log.Info($"Старт обработки страницы {pageNumber} из {countPage}");
                    try
                    {
                        Random next = new Random();
                        int randomNumber = next.Next(1000, 5000);
                        log.Debug($"Страница {pageNumber}: установка задержки {randomNumber} мс");
                        System.Threading.Thread.Sleep(randomNumber);

                        string pageUrl = url + $"?c7928-page={pageNumber}";
                        log.Info($"Страница {pageNumber}: загрузка документа. URL: {pageUrl}");

                        HtmlDocument doc = GetDocument(pageUrl);
                        var colth = doc.DocumentNode.SelectNodes(".//th");
                        var col = doc.DocumentNode.SelectNodes(".//td");

                        List<string> pageData = new List<string>();

                        if (i == 0)
                        {
                            log.Info($"Страница {pageNumber}: получение заголовка таблицы");
                            if (colth != null && colth.Count > 0)
                            {
                                countCol = colth.Count;
                                log.Info($"Страница {pageNumber}: сохранение заголовка, колонок: {countCol}");
                                foreach (var column in colth)
                                {
                                    string textContent = column.InnerText;
                                    if (!string.IsNullOrEmpty(textContent))
                                    {
                                        pageData.Add(textContent);
                                    }
                                }
                            }
                            else
                            {
                                log.Warn($"Страница {pageNumber}: заголовки таблицы не найдены");
                            }
                        }

                        if (col != null && col.Count > 0)
                        {
                            dataTrue = true;
                            countPageReady++;
                            log.Info($"Страница {pageNumber}: найдено {col.Count} элементов данных");

                            foreach (var column in col)
                            {
                                string textContent = column.InnerText;
                                if (!string.IsNullOrEmpty(textContent))
                                {
                                    pageData.Add(textContent);
                                }
                            }
                        }
                        else
                        {
                            log.Warn($"Страница {pageNumber}: данные не найдены");
                        }

                        pageResults[i] = pageData;
                        log.Info($"Страница {pageNumber} из {countPage} успешно обработана. Элементов: {pageData.Count}");
                        Console.WriteLine($"Готово {countPageReady} из {countPage}");
                    }
                    catch (Exception ex)
                    {
                        log.Error($"Ошибка при обработке страницы {pageNumber}. URL: {url + $"?c7928-page={pageNumber}"}", ex);
                        Console.WriteLine($"Ошибка при обработке страницы {pageNumber}. URL: {url + $"?c7928-page={pageNumber}"}");
                    }
                });

                log.Info($"Сбор данных из обработанных страниц. Всего страниц: {pageResults.Length}");

                int totalElements = 0;
                foreach (var pageData in pageResults)
                {
                    if (pageData != null && pageData.Count > 0)
                    {
                        dataLinks.AddRange(pageData);
                        totalElements += pageData.Count;
                    }
                }

                log.Info($"Сбор данных завершен. Всего собрано элементов: {totalElements}");

                if (dataTrue)
                {
                    log.Info($"Экспорт данных в CSV. Элементов: {dataLinks.Count}, Колонок: {countCol}, URL: {url}");
                    WorkingWithCSVFile.ExportToCSV(dataLinks, countCol);

                    log.Info($"Успешное получение и сохранение данных с сайта: {url}");
                    Console.WriteLine($"Успешное получение данных с {url}!");
                }
                else
                {
                    log.Warn($"Не удалось сохранить данные с сайта: {url}. dataTrue = false");
                    Console.WriteLine($"Не удалось сохранить данные с {url}!");
                }

                log.Info($"Завершение получения данных с сайта. URL: {url}, Получено элементов: {dataLinks.Count}");
                return dataLinks.ToList();
            }
            catch (Exception ex)
            {
                log.Error($"Ошибка при получении данных с сайта. URL: {url}", ex);
                throw;
            }
        }

    }
}