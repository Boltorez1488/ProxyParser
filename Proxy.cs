using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SocksSharp;
using SocksSharp.Proxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Yove.Proxy;

namespace Parser {
    public enum Anonimous {
        High,
        Middle,
        Low,
        None
    }

    public class ProxyItem {
        public string Ip { get; set; }
        public short Port { get; set; }
        public string City { get; set; }
        public string Speed { get; set; }
        public string Type { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public Anonimous Security { get; set; }

        public static Anonimous ParseAnonimous(string input) {
            switch (input) {
                case "Высокая":
                    return Anonimous.High;
                case "Средняя":
                    return Anonimous.Middle;
                case "Низкая":
                    return Anonimous.Low;
            }
            return Anonimous.None;
        }
    }

    public interface IProxyParser {
        void Parse();
    }

    public enum ProxyType {
        HTTP,
        HTTPS,
        SOCKS4,
        SOCKS5,
    }

    public class ProxyFree {
        public string Ip { get; set; }
        public int Port { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public ProxyType Type { get; set; }
        public string City { get; set; }
        public string Host { get; set; }
        public string Provider { get; set; }

        public long Timeout { get; set; }

        public HttpMessageHandler GetHandler() {
            switch (Type) {
                case ProxyType.HTTP:
                case ProxyType.HTTPS:
                    return new HttpClientHandler {
                        Proxy = new WebProxy(Ip, Port)
                    };
                case ProxyType.SOCKS5:
                    return new ProxyClientHandler<Socks5>(new ProxySettings {
                        Host = Ip,
                        Port = Port,
                        ConnectTimeout = 100000,
                        ReadWriteTimeOut = 100000
                    });
                case ProxyType.SOCKS4:
                    return new ProxyClientHandler<Socks4>(new ProxySettings {
                        Host = Ip,
                        Port = Port,
                        ConnectTimeout = 100000,
                        ReadWriteTimeOut = 100000
                    });
            }
            return null;
        }
    }

    public class ProxySimple {
        public string Ip { get; set; }
        public int Port { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public ProxyType Type { get; set; }

        public long Timeout { get; set; }
        
        public HttpMessageHandler GetHandler() {
            switch (Type) {
                case ProxyType.HTTP:
                case ProxyType.HTTPS:
                    return new HttpClientHandler {
                        Proxy = new WebProxy(Ip, Port)
                    };
                case ProxyType.SOCKS5:
                    return new ProxyClientHandler<Socks5>(new ProxySettings {
                        Host = Ip,
                        Port = Port,
                        ConnectTimeout = 100000,
                        ReadWriteTimeOut = 100000
                    });
                case ProxyType.SOCKS4:
                    return new ProxyClientHandler<Socks4>(new ProxySettings {
                        Host = Ip,
                        Port = Port,
                        ConnectTimeout = 100000,
                        ReadWriteTimeOut = 100000
                    });
            }
            return null;
        }
    }
}
