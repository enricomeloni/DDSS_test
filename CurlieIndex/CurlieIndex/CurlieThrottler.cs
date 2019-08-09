using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace CurlieIndex
{
    //this class is needed as Curlie blocks your request if you make too many at once
    public class CurlieThrottler
    {
        private readonly Queue<Task> tasks = new Queue<Task>();

        private HtmlWeb CurlieWeb { get; } = new HtmlWeb();

        //1 request per second
        private readonly int throttle = 1000;

        public Task<HtmlDocument> LoadPage(string url)
        {
            var loadPageWorker = new Task<HtmlDocument>(() => CurlieWeb.Load(url));
            tasks.Enqueue(loadPageWorker);
            Console.WriteLine($"task {url} enqueued");
            return loadPageWorker;
        }

        public async void Start()
        {
            while (true)
            {
                if (tasks.Count != 0)
                {
                    var task = tasks.Dequeue();
                    task.Start();
                    Console.WriteLine("task started");
                }

                await Task.Delay(throttle);
            }
        }
    }
}