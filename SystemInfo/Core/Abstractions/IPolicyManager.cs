using Microsoft.Win32;

namespace SystemInfo.Core.Abstractions
{
    public interface IPolicyManager
    {
        /// <summary>Aplica um conjunto de políticas baseadas em Registro. Retorna o total de chaves gravadas/atualizadas.</summary>
        int ApplyRegistryPolicies(params PolicySetting[] settings);

        /// <summary>Executa gpupdate /force (opcional). Retorna true se concluiu sem erro.</summary>
        bool RefreshGroupPolicy(bool machine = true, bool user = true);
    }

    public enum PolicyHive { LocalMachine, CurrentUser }

    public sealed class PolicySetting
    {
        public PolicyHive Hive { get; init; }
        public string Path { get; init; } = "";
        public string Name { get; init; } = "";
        public object Value { get; init; } = 0;
        public RegistryValueKind Kind { get; init; } = RegistryValueKind.DWord;

        public override string ToString() => $"{Hive}\\{Path}\\{Name}={Value}";
    }
}
