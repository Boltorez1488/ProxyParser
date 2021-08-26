using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using AngleSharp.Html.Parser;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ScrapySharp.Network;
using System.Diagnostics;
using System.Threading;
using ScrapySharp.Html.Forms;
using HtmlAgilityPack;
using ScrapySharp.Extensions;
using AngleSharp.Dom;
using System.Text.RegularExpressions;
using Parser.Sites;
using OpenQA.Selenium.Chrome;

namespace Parser {
    public class Program {
        static void ParseAgents() {
            var lines = File.ReadAllLines("parse.txt");
            var sb = new StringBuilder();
            foreach (var line in lines) {
                sb.AppendLine($"\"{line}\",");
            }
            File.WriteAllText("parsed.txt", sb.ToString());
        }

        static void WriteJson(string fpath, object obj) {
            using (FileStream fs = File.Open(fpath, FileMode.OpenOrCreate)) {
                using (StreamWriter sw = new StreamWriter(fs)) {
                    using (JsonTextWriter jw = new JsonTextWriter(sw)) {
                        jw.Formatting = Formatting.Indented;
                        jw.IndentChar = ' ';
                        jw.Indentation = 4;

                        new JsonSerializer().Serialize(jw, obj);
                    }
                }
            }
        }
        
        static void Main(string[] args) {
            Console.WriteLine("Parse spys.one");
            var p1 = new SpysOne();
            if (File.Exists("proxy.json")) {
                p1.ProxyList = JsonConvert.DeserializeObject<List<ProxyFree>>(File.ReadAllText("proxy.json"));
            }
            p1.Parse("US");
            if (p1.ProxyList.Count > 0) {
                p1.ProxyList.Sort((x, y) => x.Timeout.CompareTo(y.Timeout));
                WriteJson("proxy.json", p1.ProxyList);
            }

            Console.WriteLine("Parse proxyscrape.com");
            var p2 = new ProxyScrape();
            if (File.Exists("proxy_scrape.json")) {
                p2.ProxyList = JsonConvert.DeserializeObject<List<ProxySimple>>(File.ReadAllText("proxy_scrape.json"));
            }
            p2.ParseHttp("US");
            p2.ParseSocks5("US");
            p2.ParseSocks4("US");
            p2.ProxyList.Sort((x, y) => x.Timeout.CompareTo(y.Timeout));
            if (p2.ProxyList.Count > 0) {
                p2.ProxyList.Sort((x, y) => x.Timeout.CompareTo(y.Timeout));
                WriteJson("proxy_scrape.json", p2.ProxyList);
            }

            Console.WriteLine("Generate all bundle");
            var all = new List<ProxySimple>();
            foreach(var item in p1.ProxyList) {
                if (all.Exists(x => x.Ip == item.Ip && x.Port == item.Port))
                    continue;
                all.Add(new ProxySimple {
                    Ip = item.Ip,
                    Port = item.Port,
                    Type = item.Type,
                    Timeout = item.Timeout
                });
            }
            foreach (var item in p2.ProxyList) {
                if (all.Exists(x => x.Ip == item.Ip && x.Port == item.Port))
                    continue;
                all.Add(item);
            }

            all.Sort((x, y) => x.Timeout.CompareTo(y.Timeout));
            WriteJson("proxy_all.json", all);
            Console.WriteLine($"Written {all.Count} all items");
        }
    }
}
