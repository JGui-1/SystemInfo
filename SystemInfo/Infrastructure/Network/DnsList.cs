using System.Collections.Generic;

namespace SystemInfo.Infrastructure.Network
{
    public class DnsList
    {
        private Dictionary<string, (string Primary, string Secondary)> dnsList;

        public DnsList()
        {
            dnsList = new Dictionary<string, (string Primary, string Secondary)>
            {
                {"Google DNS", ("8.8.8.8", "8.8.4.4")},
                {"Cloudflare DNS", ("1.1.1.1", "1.0.0.1")},
                {"Open DNS", ("208.67.222.222", "208.67.220.220")},
                {"Open DNS 2", ("208.67.222.222", "208.67.220.220")},
                {"Quad9 DNS (Security)", ("9.9.9.9", "149.112.112.112")},
                {"Quad9 DNS (NoSecurity)", ("9.9.9.10", "149.112.112.110")},
                {"AdGuard DNS", ("94.140.14.14", "94.140.15.15")},
                {"CleanBrowsing", ("185.228.168.9", "185.228.169.9")},
                {"Alternate DNS", ("76.76.19.19", "76.223.122.150")},
                {"Control D", ("76.76.2.0", "76.76.10.0")},
                {"Norton DNS", ("192.153.192.1", "192.153.194.1")},
                {"Norton ConnectSafe", ("199.85.126.10", "199.85.127.10")},
                {"Ultra DNS", ("204.69.234.1", "204.74.101.1")},
                {"VeriSign Public DNS", ("64.6.65.6", "64.6.64.6")},
                {"Dyn", ("216.146.36.36", "216.146.35.35")},
                {"Safe DNS", ("195.46.39.40", "195.46.39.39")},
                {"Next DNS", ("45.90.30.230", "45.90.28.230")},
                {"Comodo", ("156.154.70.22", "156.154.71.22")},
                {"Neustar 2", ("156.154.70.5", "156.154.71.5")},
                {"Neustar 1", ("156.154.70.1", "156.154.71.1")},
                {"Qwest", ("205.171.3.65", "205.171.2.65")},
                {"Hurricane Electric", ("", "74.82.42.42")},
                {"Comodo Secure", ("8.26.56.56", "8.20.247.20")},
                {"Level 3 - A", ("209.244.0.3", "209.244.0.4")},
                {"Level 3 - B", ("4.2.2.1", "4.2.2.2")},
                {"Level 3 - C", ("4.2.2.3", "4.2.2.4")},
                {"Level 3 - D", ("4.2.2.6", "4.2.2.5")},
                {"Freenom World", ("80.80.81.81", "80.80.80.80")},
                {"Orange DNS", ("195.92.195.94", "195.92.195.95")},
                {"FDN", ("80.67.169.12", "80.67.169.40")},
                {"Zen Internet", ("212.23.3.1", "212.23.8.1")},
                {"SprintLink", ("204.97.212.10", "199.2.252.10")},
                {"Yandex", ("77.88.8.8", "77.88.8.1")},
                {"DNS Advantage", ("156.154.70.1", "156.154.71.1")},
                {"Yandex DNS (Family)", ("77.88.8.88", "77.88.8.2")},
                {"DNS.WATCH", ("84.200.69.80", "84.200.70.40")},
                {"Hurricane Electric (Primary)", ("74.82.42.42", "")},
                {"CZ.NIC (Stable)", ("185.43.135.1", "185.43.136.1")},
                {"Telia DNS", ("81.218.119.11", "81.218.126.11")}
            };
        }

        public Dictionary<string, (string Primary, string Secondary)> GetDnsList()
        {
            return new Dictionary<string, (string Primary, string Secondary)>(dnsList);
        }

        public string GetSecondary(string dnsName)
        {
            return dnsList.ContainsKey(dnsName) ? dnsList[dnsName].Secondary : string.Empty;
        }
    }
}