using System;
using System.Net;
using HtmlAgilityPack;

namespace CurlieIndex
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var curlie = new Curlie();
            curlie.Begin().Wait();
            return;
        }
    }
}
