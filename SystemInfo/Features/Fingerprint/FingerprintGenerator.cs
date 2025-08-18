using System;
using System.Linq;
using SystemInfo.Core.Abstractions;
using SystemInfo.Configuration;
using SystemInfo.Core.Utilities;

namespace SystemInfo.Features.Fingerprint
{
    public class FingerprintGenerator
    {
        private readonly IWmiQueryService _wmi;
        private readonly IOutput _output;

        public FingerprintGenerator(IWmiQueryService wmi, IOutput output)
        {
            _wmi = wmi;
            _output = output;
        }

        public void InteractiveShow()
        {
            _output.WriteLine("=== Fingerprint Interativo ===");
            var aliases = WmiClassMap.GetAllAliases().ToList();

            for (int i = 0; i < aliases.Count; i++)
                _output.WriteLine($"{i + 1}. {aliases[i]}");

            _output.Write("Digite os números das classes que deseja exibir (ex: 1,3,5): ");
            var input = _output.ReadLine();
            if (string.IsNullOrWhiteSpace(input)) return;

            var selectedIndexes = input.Split(',')
                                       .Select(s => int.TryParse(s.Trim(), out var idx) ? idx - 1 : -1)
                                       .Where(idx => idx >= 0 && idx < aliases.Count)
                                       .Distinct()
                                       .ToList();

            foreach (var idx in selectedIndexes)
            {
                var alias = aliases[idx];
                _output.WriteLine($"\n>>> {alias}");

                string[] props;
                if (WmiPropertiesMap.Main.TryGetValue(alias, out var definedProps) && definedProps?.Length > 0)
                {
                    _output.WriteLine($"Propriedades sugeridas: {string.Join(", ", definedProps)}");
                    _output.Write("Digite as propriedades que deseja exibir (ou 'todas'): ");
                    var propInput = _output.ReadLine();

                    if (propInput?.Trim().ToLower() == "todas")
                        props = definedProps;
                    else if (!string.IsNullOrWhiteSpace(propInput))
                        props = propInput.Split(',').Select(p => p.Trim()).Where(p => !string.IsNullOrWhiteSpace(p)).ToArray();
                    else
                        props = definedProps;
                }
                else
                {
                    _output.WriteLine("Nenhum conjunto de propriedades predefinido. Exibindo primeiras propriedades disponíveis.");
                    props = null; // pegar todas (primeira instância)
                }

                // Mostrar propriedades principais/selecionadas (primeira instância)
                var results = props != null ? _wmi.QuerySelect(alias, props) : _wmi.QueryAll(alias);
                bool printedAny = false;
                foreach (var dict in results)
                {
                    foreach (var kv in dict)
                        _output.WriteLine($"  {kv.Key}: {ValueFormatter.Format(kv.Value)}");
                    printedAny = true;
                    break; // primeira instância
                }
                if (!printedAny) _output.WriteLine("  (Sem resultados)");

                // Perguntar se deseja exibir TODAS as propriedades da classe (primeira instância)
                _output.Write("Deseja exibir TODAS as propriedades dessa classe? (s/n): ");
                if (_output.ReadLine()?.Trim().ToLower() == "s")
                {
                    foreach (var dict in _wmi.QueryAll(alias))
                    {
                        _output.WriteLine("-- Todas as propriedades --");
                        foreach (var kv in dict.OrderBy(k => k.Key))
                            _output.WriteLine($"  {kv.Key}: {ValueFormatter.Format(kv.Value)}");
                        break;
                    }
                }
            }
        }
    }
}
