using System.Collections.Generic;
using System.IO;

namespace CurlieIndex
{
    public class ProxyList
    {
        public string[] Proxies { get; private set; }
        private int currentIndex = 0;

        public string NextProxy()
        {
            var proxy = Proxies[currentIndex];
            currentIndex = (currentIndex + 1) % Proxies.Length;
            return proxy;
        }


        public static ProxyList ReadFromFile()
        {
            return new ProxyList()
            {
                Proxies = File.ReadAllLines(@"./proxies.txt")
            };
        }
    }
}