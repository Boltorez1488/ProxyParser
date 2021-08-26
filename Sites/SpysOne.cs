using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.Extensions;
using OpenQA.Selenium.Support.UI;
using ScrapySharp.Extensions;
using ScrapySharp.Html.Forms;
using ScrapySharp.Network;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Yove.Proxy;

namespace Parser.Sites {
    public class SpysOne {
        public List<ProxyFree> ProxyList = new List<ProxyFree>();

        private int proxyIndex = 0;
        public HttpMessageHandler NeedHandler {
            get {
            nx:
                if (proxyIndex < ProxyList.Count) {
                    var proxy = ProxyList[proxyIndex++];
                    return proxy.GetHandler();
                } else {
                    if (ProxyList.Count == 0)
                        return null;
                    proxyIndex = 0;
                    goto nx;
                }
            }
        }

        async Task LoadProxy(ProxyFree item) {
            HttpClient client = new HttpClient(item.GetHandler(), true);

            var time = Stopwatch.StartNew();
            try {
                var res = await client.GetAsync("https://api.ipify.org?format=json");
                time.Stop();
                item.Timeout = time.ElapsedMilliseconds;

                if (res.StatusCode == HttpStatusCode.OK) {
                    var obj = JObject.Parse(await res.Content.ReadAsStringAsync());
                    if (obj["ip"].ToObject<string>() == item.Ip) {
                        lock (ProxyList) ProxyList.Add(item);
                    }
                }
            } catch (Exception) {
                return;
            }
        }

        public void ParseHtml(string html) {
            var parser = new HtmlParser();
            var document = parser.ParseDocument(html);

            var items = document.QuerySelectorAll(".spy1xx, .spy1x").Skip(2).ToArray();
            int index = 0;
            Console.WriteLine($"Parsing {items.Length} items");
            Parallel.ForEach(items, item => {
                Interlocked.Increment(ref index);

                var ipBlock = item.QuerySelector("td .spy14");
                if (ipBlock == null)
                    return;
                ipBlock.QuerySelector("script").Remove();
                var ipPort = ipBlock.TextContent;

                var ip = ipPort.Split(':')[0];
                var port = int.Parse(ipPort.Split(':')[1]);
                lock(ProxyList) {
                    if (ProxyList.Exists(x => x.Ip == ip && x.Port == port)) {
                        return;
                    }
                }

                string type;
                if (item.QuerySelector("td:nth-child(2) .spy1") != null) {
                    type = item.QuerySelector("td:nth-child(2) .spy1").TextContent;
                    if (item.QuerySelector("td:nth-child(2) .spy14") != null) {
                        type += item.QuerySelector("td:nth-child(2) .spy14").TextContent;
                    }
                } else {
                    type = item.QuerySelector("td:nth-child(2)").TextContent;
                }

                var city = item.QuerySelector("td:nth-child(4)").TextContent;

                var hostOrg = item.QuerySelector("td:nth-child(5)");
                var host = hostOrg.QuerySelector(".spy1").TextContent;
                var provider = hostOrg.QuerySelector(".spy14").TextContent;

                Console.WriteLine($"{ipPort} : {index}/{items.Length}");

                var pType = Enum.Parse(typeof(ProxyType), type);
                if (pType == null)
                    throw new Exception($"{ip}:{port} {type} == null");

                try {
                    LoadProxy(new ProxyFree {
                        Ip = ip,
                        Port = port,
                        Type = (ProxyType)pType,
                        City = city,
                        Host = host,
                        Provider = provider
                    }).Wait();
                } catch (Exception) {

                }
            });

            Console.WriteLine($"Parsed {ProxyList.Count} items");
        }

        public static void Wait(ChromeDriver driver) {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
            wait.Until(x => {
                return x.ExecuteJavaScript<string>("return document.readyState;") == "complete";
            });
        }

        public void Parse(string country = "RU") {
            var chromeOptions = new ChromeOptions();
           // chromeOptions.AddArguments(new List<string>() { "headless" });
            var chromeDriverService = ChromeDriverService.CreateDefaultService();
            var driver = new ChromeDriver(chromeDriverService, chromeOptions);
            driver.Url = $"http://spys.one/free-proxy-list/{country}/";
            driver.Navigate();
            Wait(driver);
            Thread.Sleep(1000);

            driver.FindElementByCssSelector("#xpp option[value='5']").Click();
            //driver.ExecuteScript("document.querySelector('#xpp').selectedIndex = 5; document.querySelector('form').submit()");
            Wait(driver);

            var html = driver.PageSource;
            driver.Quit();

            ParseHtml(html);
        }
    }
}
