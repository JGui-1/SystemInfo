
using System;

namespace SystemInfo.Infrastructure.QoS
{
    public static class Manager
    {
        public static void List(string backend, string scope = "")
        {
            if (backend.Equals("registry", StringComparison.OrdinalIgnoreCase))
                RegBackend.List();
            else
                Console.WriteLine("[QoS] Backend não suportado: " + backend);
        }

        public static void Create(string backend, QoSOptions opt, bool overwrite = false)
        {
            if (backend.Equals("registry", StringComparison.OrdinalIgnoreCase))
                RegBackend.Create(opt, overwrite);
            else
                Console.WriteLine("[QoS] Backend não suportado: " + backend);
        }

        public static void Delete(string backend, string name, string scope = "")
        {
            if (backend.Equals("registry", StringComparison.OrdinalIgnoreCase))
                RegBackend.Delete(name);
            else
                Console.WriteLine("[QoS] Backend não suportado: " + backend);
        }
    }
}
