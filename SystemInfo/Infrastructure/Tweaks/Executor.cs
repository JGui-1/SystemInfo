using System;
using System.Management.Automation;


namespace SystemInfo.Infrastructure.Tweaks
{
    public static class Executor
    {
        private static void RunScript(string script)
        {
            try
            {
                using (PowerShell ps = PowerShell.Create())
                {
                    ps.AddScript(script);
                    var results = ps.Invoke();

                    foreach (var result in results)
                        Console.WriteLine(result.ToString());
                }
                Console.WriteLine("\n✅ Sucesso!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erro: {ex.Message}");
            }
        }

        // ----------------------------
        // Métodos para cada tweak
        // ----------------------------

        public static void ApplyAlignTaskbarLeft() =>
            RunScript(TweaksRepository.AllTweaks["align-taskbar-left"].ApplyScript);
        public static void UnapplyAlignTaskbarLeft() =>
            RunScript(TweaksRepository.AllTweaks["align-taskbar-left"].UnapplyScript);

        public static void ApplyDebloatWindows() =>
            RunScript(TweaksRepository.AllTweaks["debloat-windows"].ApplyScript);

        public static void ApplyDetailedBsod() =>
            RunScript(TweaksRepository.AllTweaks["detailed-bsod"].ApplyScript);
        public static void UnapplyDetailedBsod() =>
            RunScript(TweaksRepository.AllTweaks["detailed-bsod"].UnapplyScript);

        public static void ApplyDisableBackgroundMsStoreApps() =>
            RunScript(TweaksRepository.AllTweaks["disable-background-ms-store-apps"].ApplyScript);
        public static void UnapplyDisableBackgroundMsStoreApps() =>
            RunScript(TweaksRepository.AllTweaks["disable-background-ms-store-apps"].UnapplyScript);

        public static void ApplyDisableCopilot() =>
            RunScript(TweaksRepository.AllTweaks["disable-copilot"].ApplyScript);
        public static void UnapplyDisableCopilot() =>
            RunScript(TweaksRepository.AllTweaks["disable-copilot"].UnapplyScript);

        public static void ApplyDisableCoreIsolation() =>
            RunScript(TweaksRepository.AllTweaks["disable-core-isolation"].ApplyScript);
        public static void UnapplyDisableCoreIsolation() =>
            RunScript(TweaksRepository.AllTweaks["disable-core-isolation"].UnapplyScript);

        public static void ApplyDisableDefenderRtp() =>
            RunScript(TweaksRepository.AllTweaks["disable-defender-rtp"].ApplyScript);
        public static void UnapplyDisableDefenderRtp() =>
            RunScript(TweaksRepository.AllTweaks["disable-defender-rtp"].UnapplyScript);

        public static void ApplyDisableDynamicTicking() =>
            RunScript(TweaksRepository.AllTweaks["disable-dynamic-ticking"].ApplyScript);
        public static void UnapplyDisableDynamicTicking() =>
            RunScript(TweaksRepository.AllTweaks["disable-dynamic-ticking"].UnapplyScript);

        public static void ApplyDisableFastStartup() =>
            RunScript(TweaksRepository.AllTweaks["disable-fast-startup"].ApplyScript);
        public static void UnapplyDisableFastStartup() =>
            RunScript(TweaksRepository.AllTweaks["disable-fast-startup"].UnapplyScript);

        public static void ApplyDisableGamebar() =>
            RunScript(TweaksRepository.AllTweaks["disable-gamebar"].ApplyScript);
        public static void UnapplyDisableGamebar() =>
            RunScript(TweaksRepository.AllTweaks["disable-gamebar"].UnapplyScript);

        public static void ApplyDisableHibernation() =>
            RunScript(TweaksRepository.AllTweaks["disable-hibernation"].ApplyScript);
        public static void UnapplyDisableHibernation() =>
            RunScript(TweaksRepository.AllTweaks["disable-hibernation"].UnapplyScript);

        public static void ApplyDisableLocationTracking() =>
            RunScript(TweaksRepository.AllTweaks["disable-location-tracking"].ApplyScript);
        public static void UnapplyDisableLocationTracking() =>
            RunScript(TweaksRepository.AllTweaks["disable-location-tracking"].UnapplyScript);

        public static void ApplyDisableLockscreenTips() =>
            RunScript(TweaksRepository.AllTweaks["disable-lockscreen-tips"].ApplyScript);
        public static void UnapplyDisableLockscreenTips() =>
            RunScript(TweaksRepository.AllTweaks["disable-lockscreen-tips"].UnapplyScript);

        public static void ApplyDisableMouseAcceleration() =>
            RunScript(TweaksRepository.AllTweaks["disable-mouse-acceleration"].ApplyScript);
        public static void UnapplyDisableMouseAcceleration() =>
            RunScript(TweaksRepository.AllTweaks["disable-mouse-acceleration"].UnapplyScript);

        public static void ApplyDisableWifiSense() =>
            RunScript(TweaksRepository.AllTweaks["disable-wifi-sense"].ApplyScript);
        public static void UnapplyDisableWifiSense() =>
            RunScript(TweaksRepository.AllTweaks["disable-wifi-sense"].UnapplyScript);

        public static void ApplyEnableDarkMode() =>
            RunScript(TweaksRepository.AllTweaks["enable-dark-mode"].ApplyScript);
        public static void UnapplyEnableDarkMode() =>
            RunScript(TweaksRepository.AllTweaks["enable-dark-mode"].UnapplyScript);

        public static void ApplyEnableEndTaskRightClick() =>
            RunScript(TweaksRepository.AllTweaks["enable-end-task-right-click"].ApplyScript);
        public static void UnapplyEnableEndTaskRightClick() =>
            RunScript(TweaksRepository.AllTweaks["enable-end-task-right-click"].UnapplyScript);

        public static void ApplyEnableHpet() =>
            RunScript(TweaksRepository.AllTweaks["enable-hpet"].ApplyScript);
        public static void UnapplyEnableHpet() =>
            RunScript(TweaksRepository.AllTweaks["enable-hpet"].UnapplyScript);

        public static void ApplyOptimizeNetworkSettings() =>
            RunScript(TweaksRepository.AllTweaks["optimize-network-settings"].ApplyScript);
        public static void UnapplyOptimizeNetworkSettings() =>
            RunScript(TweaksRepository.AllTweaks["optimize-network-settings"].UnapplyScript);

        public static void ApplyOptimizeNvidiaSettings() =>
            RunScript(TweaksRepository.AllTweaks["optimize-nvidia-settings"].ApplyScript);

        public static void ApplyRemoveGamingApps() =>
            RunScript(TweaksRepository.AllTweaks["remove-gaming-apps"].ApplyScript);

        public static void ApplyRemoveOnedrive() =>
            RunScript(TweaksRepository.AllTweaks["remove-onedrive"].ApplyScript);
        public static void UnapplyRemoveOnedrive() =>
            RunScript(TweaksRepository.AllTweaks["remove-onedrive"].UnapplyScript);

        public static void ApplyRevertContextMenu() =>
            RunScript(TweaksRepository.AllTweaks["revert-context-menu"].ApplyScript);
        public static void UnapplyRevertContextMenu() =>
            RunScript(TweaksRepository.AllTweaks["revert-context-menu"].UnapplyScript);

        public static void ApplyRunDiskCleanup() =>
            RunScript(TweaksRepository.AllTweaks["run-disk-cleanup"].ApplyScript);

        public static void ApplySetServicesToManual() =>
            RunScript(TweaksRepository.AllTweaks["set-services-to-manual"].ApplyScript);
        public static void UnapplySetServicesToManual() =>
            RunScript(TweaksRepository.AllTweaks["set-services-to-manual"].UnapplyScript);
    }
}
