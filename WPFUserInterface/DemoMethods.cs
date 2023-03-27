using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace WPFUserInterface
{
    public static class DemoMethods
    {
        public static List<string> PrepData()
        {
            List<string> output = new List<string>();

            output.Add("https://www.yahoo.com");
            output.Add("https://www.google.com");
            //output.Add("https://www.microsoft.com");
            output.Add("https://www.cnn.com");
            output.Add("https://www.amazon.com");
            //output.Add("https://www.facebook.com");
            //output.Add("https://www.twitter.com");
            output.Add("https://www.codeproject.com");
            //output.Add("https://www.stackoverflow.com");
            output.Add("https://en.wikipedia.org/wiki/.NET_Framework");

            return output;
        }

        /// <summary>
        /// Параллельная загрузка всех данных с сайтов одновременно Parallel.ForEach
        /// </summary>
        /// <returns></returns>
        public static List<WebsiteDataModel> RunDownloadParallelSync()
        {
            List<string> websites = PrepData();
            List<WebsiteDataModel> output = new List<WebsiteDataModel>();

            Parallel.ForEach<string>(websites, (site) =>
            {
                WebsiteDataModel results = DownloadWebsite(site);
                output.Add(results);
            });

            return output;
        }

        /// <summary>
        /// Асинхронная загрузка всех сайтов по очереди foreach
        /// </summary>
        /// <param name="progress"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<List<WebsiteDataModel>> RunDownloadAsync(IProgress<ProgressReportModel> progress, CancellationToken cancellationToken)
        {
            List<string> websites = PrepData();
            List<WebsiteDataModel> output = new List<WebsiteDataModel>();
            ProgressReportModel report = new ProgressReportModel();

            foreach (string site in websites)
            {
                WebsiteDataModel results = await DownloadWebsiteAsync(site);
                output.Add(results);

                cancellationToken.ThrowIfCancellationRequested();

                report.SitesDownloaded = output;
                report.PercentageComplete = (output.Count * 100) / websites.Count;
                progress.Report(report);
            }

            return output;
        }

        /// <summary>
        /// Параллельная загрузка сайтов в отдельном таске Task.Run ()=> Parallel.ForEach
        /// </summary>
        /// <param name="progress"></param>
        /// <returns></returns>
        public static async Task<List<WebsiteDataModel>> RunDownloadParallelAsyncV2(IProgress<ProgressReportModel> progress)
        {
            List<string> websites = PrepData();
            List<WebsiteDataModel> output = new List<WebsiteDataModel>();
            ProgressReportModel report = new ProgressReportModel();

            await Task.Run(() =>
            {
                Parallel.ForEach<string>(websites, (site) =>
                {
                    WebsiteDataModel results = DownloadWebsite(site);
                    output.Add(results);

                    report.SitesDownloaded = output;
                    report.PercentageComplete = (output.Count * 100) / websites.Count;
                    progress.Report(report);
                });
            });

            return output;
        }


        /// <summary>
        /// Последовательная загрузка всех данных foreach
        /// </summary>
        /// <returns></returns>
        public static List<WebsiteDataModel> RunDownloadSync()
        {
            List<string> websites = PrepData();
            List<WebsiteDataModel> output = new List<WebsiteDataModel>();

            foreach (string site in websites)
            {
                WebsiteDataModel results = DownloadWebsite(site);
                output.Add(results);
            }

            return output;
        }

        /// <summary>
        /// Асинхронная загрузка данных foreach (плоха тем что не сможем получить подробный статус выполнения в процентах так как WhenAll() ) 
        /// </summary>
        /// <returns></returns>
        public static async Task<List<WebsiteDataModel>> RunDownloadParallelAsync()
        {
            List<string> websites = PrepData();
            List<Task<WebsiteDataModel>> tasks = new List<Task<WebsiteDataModel>>();

            foreach (string site in websites)
            {
                tasks.Add(DownloadWebsiteAsync(site));
            }

            var results = await Task.WhenAll(tasks);

            return new List<WebsiteDataModel>(results);
        }

        private static async Task<WebsiteDataModel> DownloadWebsiteAsync(string websiteURL)
        {
            WebsiteDataModel output = new WebsiteDataModel();
            WebClient client = new WebClient();

            output.WebsiteUrl = websiteURL;
            output.WebsiteData = await client.DownloadStringTaskAsync(websiteURL);

            return output;
        }
        private static WebsiteDataModel DownloadWebsite(string websiteURL)
        {
            WebsiteDataModel output = new WebsiteDataModel();
            WebClient client = new WebClient();

            output.WebsiteUrl = websiteURL;
            output.WebsiteData = client.DownloadString(websiteURL);

            return output;
        }
    }
}