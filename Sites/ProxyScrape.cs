using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Parser.Sites {
    public class ProxyScrape {
        public List<ProxySimple> ProxyList = new List<ProxySimple>();

        public async Task<bool> LoadProxy(ProxySimple item) {
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
                        return true;
                    }
                }
            } catch (Exception) {
                return false;
            }

            return false;
        }
        
        public void ParseHttp(string country = "RU") {
            var client = new HttpClient();
            var body = client.GetAsync($"https://api.proxyscrape.com/?request=getproxies&proxytype=http&timeout=10000&country={country}&ssl=all&anonymity=all").Result;

            var lines = body.Content.ReadAsStringAsync().Result.Split('\n')
                .Select(x => x.TrimEnd('\r'))
                .Where(x => !string.IsNullOrEmpty(x)).ToArray();
            var parsed = 0;
            var index = 0;
            Console.WriteLine($"Parsing {lines.Length} HTTP items");
            Parallel.ForEach(lines, line => {
                Interlocked.Increment(ref index);
                var split = line.Split(':');
                var ip = split[0];
                var port = int.Parse(split[1]);
                lock (ProxyList) {
                    if (ProxyList.Exists(x => x.Ip == ip && x.Port == port)) {
                        return;
                    }
                }
                Console.WriteLine($"HTTP {ip}:{port} : {index}/{lines.Length}");
                if (LoadProxy(new ProxySimple {
                    Ip = ip,
                    Port = port,
                    Type = ProxyType.HTTP
                }).Result) {
                    Interlocked.Increment(ref parsed);
                };
            });
            Console.WriteLine($"Parsed {parsed} HTTP items");
        }

        public void ParseSocks4(string country = "RU") {
            var client = new HttpClient();
            var body = client.GetAsync($"https://api.proxyscrape.com/?request=getproxies&proxytype=socks4&timeout=10000&country={country}").Result;

            var lines = body.Content.ReadAsStringAsync().Result.Split('\n')
                .Select(x => x.TrimEnd('\r'))
                .Where(x => !string.IsNullOrEmpty(x)).ToArray();
            var index = 0;
            var parsed = 0;
            Console.WriteLine($"Parsing {lines.Length} SOCKS4 items");
            Parallel.ForEach(lines, line => {
                Interlocked.Increment(ref index);
                var split = line.Split(':');
                var ip = split[0];
                var port = int.Parse(split[1]);
                lock (ProxyList) {
                    if (ProxyList.Exists(x => x.Ip == ip && x.Port == port)) {
                        return;
                    }
                }
                Console.WriteLine($"SOCKS4 {ip}:{port} : {index}/{lines.Length}");
                if (LoadProxy(new ProxySimple {
                    Ip = ip,
                    Port = port,
                    Type = ProxyType.SOCKS4
                }).Result) {
                    Interlocked.Increment(ref parsed);
                };
            });
            Console.WriteLine($"Parsed {parsed} SOCKS4 items");
        }

        public void ParseSocks5(string country = "RU") {
            var client = new HttpClient();
            var body = client.GetAsync($"https://api.proxyscrape.com/?request=getproxies&proxytype=socks5&timeout=10000&country={country}").Result;

            var lines = body.Content.ReadAsStringAsync().Result.Split('\n')
                 .Select(x => x.TrimEnd('\r'))
                 .Where(x => !string.IsNullOrEmpty(x)).ToArray();
            var index = 0;
            var parsed = 0;
            Console.WriteLine($"Parsing {lines.Length} SOCKS5 items");
            Parallel.ForEach(lines, line => {
                Interlocked.Increment(ref index);
                var split = line.Split(':');
                var ip = split[0];
                var port = int.Parse(split[1]);
                lock (ProxyList) {
                    if (ProxyList.Exists(x => x.Ip == ip && x.Port == port)) {
                        return;
                    }
                }
                Console.WriteLine($"SOCKS5 {ip}:{port} : {index}/{lines.Length}");
                if (LoadProxy(new ProxySimple {
                    Ip = ip,
                    Port = port,
                    Type = ProxyType.SOCKS5
                }).Result) {
                    Interlocked.Increment(ref parsed);
                };
            });
            Console.WriteLine($"Parsed {parsed} SOCKS5 items");
        }
    }
}
