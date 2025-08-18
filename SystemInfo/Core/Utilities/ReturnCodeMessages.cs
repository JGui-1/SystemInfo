using System.Collections.Generic;

namespace SystemInfo.Core.Utilities
{
    public static class ReturnCodeMessages
    {
        private static readonly Dictionary<object, string> Codes = new()
        {
            { 0, "Sucesso" },
            { 1, "Acesso negado" },
            { 2, "Acesso negado (geral)" },
            { 3, "Dependência ausente/estado inválido" },
            { 5, "Acesso negado (Win32)" },
            { 9, "Já no estado solicitado" },
            { 64, "Não suportado" },
            { 65, "Parâmetro inválido" },
            { 66, "Sistema ocupado" },
            { 67, "Método não disponível" },
            { 68, "Método falhou" },
            { -1, "Erro desconhecido" }
        };

        public static string Describe(object code) =>
            code == null ? "Nulo" : (Codes.TryGetValue(code, out var s) ? s : $"Código {code}");
    }
}
