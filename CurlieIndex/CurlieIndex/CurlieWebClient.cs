using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace CurlieIndex
{
    //this class is needed as Curlie blocks your request if you make too many at once
    public class CurlieWebClient
    {
        private readonly Queue<Task> tasks = new Queue<Task>();
        private ProxyList proxyList = ProxyList.ReadFromFile();

        //1 request per second
        private readonly int throttle = 1000;

        public Task<HtmlDocument> LoadPage(string url)
        {
            var loadPageWorker = CreateCurlieWorker(url);
            tasks.Enqueue(loadPageWorker);
            Console.WriteLine($"task {url} enqueued");
            return loadPageWorker;
        }

        private Task<HtmlDocument> CreateCurlieWorker(string url)
        {
            return new Task<HtmlDocument>(() =>
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                var proxyAddress = proxyList.NextProxy();
                var proxy = new WebProxy(proxyAddress) {BypassProxyOnLocal = false};
                request.Proxy = proxy;
                request.Method = "GET";
                var response = (HttpWebResponse)request.GetResponse();

                Console.WriteLine($"Proxy {proxyAddress} working");

                using (var reader = new StreamReader(response.GetResponseStream() ?? throw new InvalidOperationException(), Encoding.UTF8))
                {
                    var doc = new HtmlDocument();
                    doc.Load(reader.BaseStream);
                    return doc;
                }
            });
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