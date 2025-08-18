# Manual do Desenvolvedor – SystemInfo

## 1. Introdução
Este manual explica como **funciona internamente** o projeto SystemInfo e como você pode **estender, integrar e testar** suas funcionalidades.  
O projeto foi desenhado para ser modular, escalável e fácil de integrar a outros sistemas.

### Estrutura do Projeto
- **Core** → Interfaces, abstrações e utilitários.
- **Configuration** → Mapeamentos de classes/propriedades WMI.
- **Features** → Funcionalidades (Fingerprint, Processos, Serviços, Disco).
- **Infrastructure** → Implementações concretas (WMI, sistema, output).
- **Presentation** → Menus e interação com usuário.
- **Program.cs** → Ponto de entrada.

---

## 2. Consultas WMI

### Buscar valor único (`GetScalar`)
```csharp
var wmi = new WmiQueryService();
string version = wmi.GetScalar("OperatingSystem", "Version");
Console.WriteLine($"Versão do Windows: {version}");
```

### Buscar múltiplas propriedades (`QuerySelect`)
```csharp
var wmi = new WmiQueryService();
var cpus = wmi.QuerySelect("Processor", new[] { "Name", "NumberOfCores" });
foreach (var cpu in cpus)
    Console.WriteLine($"{cpu["Name"]} - {cpu["NumberOfCores"]} cores");
```

### Buscar todos os objetos (`QueryAll`)
```csharp
var wmi = new WmiQueryService();
var disks = wmi.QueryAll("LogicalDisk");
foreach (var d in disks)
    Console.WriteLine($"{d["DeviceID"]} - {d["FreeSpace"]}");
```

### Usando WHERE
```csharp
var wmi = new WmiQueryService();
var c = wmi.GetScalar("Service", "Name", "Name='Spooler'");
Console.WriteLine($"Serviço encontrado: {c}");
```

---

## 3. Invocando Métodos WMI

### Método sem parâmetros (`StopService`)
```csharp
var invoker = new WmiMethodInvoker();
var result = invoker.Invoke("Service", "StopService", "Name='Fax'");
Console.WriteLine($"Código de retorno: {result}");
```

### Método com parâmetros (`SetPowerState`)
```csharp
var invoker = new WmiMethodInvoker();
var result = invoker.Invoke("DiskDrive", "SetPowerState", null, new Dictionary<string, object>
{
    { "PowerState", 4 }, // Hibernate
    { "Time", 0 }
});
```

### Interpretando códigos
```csharp
string msg = WmiReturnCodeHelper.Describe(result);
Console.WriteLine(msg);
```

---

## 4. Integração no Projeto
- O **menu principal** usa `WmiQueryService` e `WmiMethodInvoker` para executar ações.
- O **fingerprint** usa `WmiClassMap` e `WmiPropertiesMap` para saber o que exibir.
- As **features** ficam separadas em `Features/`, e podem ser adicionadas sem alterar o core.

---

## 5. Extensibilidade
### Nova classe no `WmiClassMap`
```csharp
["Battery"] = "Win32_Battery"
```

### Novas propriedades no `WmiPropertiesMap`
```csharp
["Battery"] = new[] { "Name", "EstimatedChargeRemaining" }
```

### Criando novo feature
- Criar classe em `Features/`.
- Usar `IWmiQueryService` para consultas.
- Adicionar ao menu principal.

---

## 6. Boas práticas
- Evite consultas que retornam milhares de objetos.
- Sempre valide se a propriedade existe.
- Rode como administrador quando mexer com serviços/discos.
- Nunca pare serviços críticos.

---

## 7. Integração Externa
- Referencie `SystemInfo.dll` em outro projeto.
- Exemplo em API REST (ASP.NET Core):
```csharp
[HttpGet("cpu")]
public IActionResult GetCpu([FromServices] IWmiQueryService wmi)
{
    var cpu = wmi.GetScalar("Processor", "Name");
    return Ok(cpu);
}
```

- Em agendadores (Quartz.NET, Task Scheduler) → chame métodos do `WmiQueryService` em jobs.

---

## 8. Persistência
Salvar fingerprint em JSON:
```csharp
var wmi = new WmiQueryService();
var sys = wmi.QuerySelect("OperatingSystem", new[] { "Caption", "Version" });
File.WriteAllText("fingerprint.json", JsonSerializer.Serialize(sys));
```

---

## 9. Testabilidade
- `NetworkAdapterTesterDryRun` é exemplo de mock.
- Você pode implementar `IWmiQueryService` fake para testes:
```csharp
class FakeWmi : IWmiQueryService
{
    public string GetScalar(string alias, string property, string whereClause = null) 
        => "FakeValue";
    // ...
}
```

---

## 📌 Nota Importante
Este manual mostra tanto:
- Funcionalidades **nativas** já implementadas no projeto (`GetScalar`, `QuerySelect`, `QueryAll`, `Invoke`, etc.).
- Quanto **cenários de uso/integração** que podem ser implementados em cima da API existente (ex: exportar fingerprint em JSON, integrar em API REST, usar em agendador).

Ou seja: nada aqui requer mudar o core do código — tudo já é possível com o que está implementado.
