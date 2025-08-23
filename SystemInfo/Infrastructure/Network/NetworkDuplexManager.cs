using System;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;
using System.Threading;
using System.Security.AccessControl;

namespace SystemInfo.Infrastructure.Network
{
    public static class NetworkDuplexManager
    {
        private static readonly Guid GUID_DEVCLASS_NET = new("{4d36e972-e325-11ce-bfc1-08002be10318}");
        private const uint DIGCF_PRESENT = 0x02;

        [StructLayout(LayoutKind.Sequential)]
        struct SP_DEVINFO_DATA
        {
            public uint cbSize;
            public Guid ClassGuid;
            public uint DevInst;
            public IntPtr Reserved;
        }

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern IntPtr SetupDiGetClassDevs(Guid ClassGuid, IntPtr Enumerator, IntPtr hwndParent, uint Flags);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool SetupDiEnumDeviceInfo(IntPtr DeviceInfoSet, uint MemberIndex, ref SP_DEVINFO_DATA DeviceInfoData);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool SetupDiGetDeviceRegistryProperty(
            IntPtr DeviceInfoSet,
            ref SP_DEVINFO_DATA DeviceInfoData,
            uint Property,
            out uint PropertyRegDataType,
            byte[] PropertyBuffer,
            uint PropertyBufferSize,
            out uint RequiredSize);

        [DllImport("setupapi.dll", SetLastError = true)]
        static extern void SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);

        private const uint SPDRP_DRIVER = 0x00000009;

        public enum DuplexMode
        {
            AutoNegotiation = 0,
            _10MbpsHalf = 1,
            _10MbpsFull = 2,
            _100MbpsHalf = 3,
            _100MbpsFull = 4,
            _1GbpsFull = 6
        }

        public static void SetSpeedDuplex(DuplexMode mode)
        {
            var mainAdapter = NetworkInterface.GetAllNetworkInterfaces()
                .Where(nic => nic.OperationalStatus == OperationalStatus.Up &&
                              nic.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                              nic.GetIPProperties().GatewayAddresses.Any())
                .FirstOrDefault();

            if (mainAdapter == null)
                throw new InvalidOperationException("Nenhum adaptador principal encontrado.");

            string netCfgId = mainAdapter.Id;

            IntPtr deviceInfoSet = SetupDiGetClassDevs(GUID_DEVCLASS_NET, IntPtr.Zero, IntPtr.Zero, DIGCF_PRESENT);
            if (deviceInfoSet == IntPtr.Zero)
                throw new InvalidOperationException("Falha ao acessar lista de dispositivos.");

            try
            {
                var devInfoData = new SP_DEVINFO_DATA();
                devInfoData.cbSize = (uint)Marshal.SizeOf(devInfoData);

                int index = 0;
                while (SetupDiEnumDeviceInfo(deviceInfoSet, (uint)index, ref devInfoData))
                {
                    string driverKeyPath = GetDeviceRegistryProperty(deviceInfoSet, devInfoData, SPDRP_DRIVER);
                    if (string.IsNullOrWhiteSpace(driverKeyPath))
                    {
                        index++;
                        continue;
                    }

                    string fullPath = @"SYSTEM\\CurrentControlSet\\Control\\Class\\" + driverKeyPath;

                    using var adapterKey = Registry.LocalMachine.OpenSubKey(
                        fullPath,
                        RegistryKeyPermissionCheck.ReadWriteSubTree,
                        RegistryRights.ReadKey | RegistryRights.WriteKey
                    );

                    if (adapterKey != null)
                    {
                        var instanceId = adapterKey.GetValue("NetCfgInstanceId")?.ToString();
                        if (!string.IsNullOrEmpty(instanceId) && instanceId.Equals(netCfgId, StringComparison.OrdinalIgnoreCase))
                        {
                            adapterKey.SetValue("*SpeedDuplex", ((int)mode).ToString(), RegistryValueKind.String);
                            RestartAdapter(mainAdapter.Name);
                            return;
                        }
                    }
                    index++;
                }

                throw new InvalidOperationException("Não encontrei o adaptador no Registro.");
            }
            finally
            {
                SetupDiDestroyDeviceInfoList(deviceInfoSet);
            }
        }

        private static string GetDeviceRegistryProperty(IntPtr deviceInfoSet, SP_DEVINFO_DATA devInfoData, uint property)
        {
            byte[] buffer = new byte[1024];
            SetupDiGetDeviceRegistryProperty(deviceInfoSet, ref devInfoData, property, out _, buffer, (uint)buffer.Length, out _);
            return Encoding.Unicode.GetString(buffer).TrimEnd('\0');
        }

        private static void RestartAdapter(string name)
        {
            RunCmd($"netsh interface set interface \"{name}\" admin=disable");
            Thread.Sleep(2000);
            RunCmd($"netsh interface set interface \"{name}\" admin=enable");
        }

        private static void RunCmd(string command)
        {
            var psi = new ProcessStartInfo("cmd.exe", "/c " + command)
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                Verb = "runas"
            };
            using var proc = Process.Start(psi);
            proc?.WaitForExit();
        }
    }
}
