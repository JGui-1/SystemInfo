# API Guide – SystemInfo

Este documento descreve a API interna do projeto.

---

## 1. Visão Geral
Interfaces:
- `IWmiQueryService`
- `IWmiMethodInvoker`

---

## 2. IWmiQueryService
### `GetScalar`
```csharp
string version = wmi.GetScalar("OperatingSystem", "Version");
```

### `QuerySelect`
```csharp
var cpu = wmi.QuerySelect("Processor", new[] { "Name", "NumberOfCores" });
```

### `QueryAll`
```csharp
var disks = wmi.QueryAll("LogicalDisk");
```

### Exemplos práticos
- Versão do Windows:
```csharp
wmi.GetScalar("OperatingSystem", "Version");
```

- Nome da CPU:
```csharp
wmi.GetScalar("Processor", "Name");
```

- Espaço livre em disco:
```csharp
var c = wmi.GetScalar("LogicalDisk", "FreeSpace", "DeviceID='C:'");
```

---

## 3. IWmiMethodInvoker
### Invoke sem parâmetros
```csharp
invoker.Invoke("Service", "StopService", "Name='Fax'");
```

### Invoke com parâmetros
```csharp
invoker.Invoke("DiskDrive", "SetPowerState", null, new Dictionary<string, object>
{
    { "PowerState", 4 },
    { "Time", 0 }
});
```

---

## 4. Tratamento de Erros
- Códigos → `WmiReturnCodeHelper.Describe(code)`
- Classe não existe → exceção
- Propriedade inválida → exceção
- Serviço não encontrado → retorno != 0

---

## 5. Casos Avançados
- Integrar fingerprint:
```csharp
var props = WmiPropertiesMap.Map["Processor"];
var cpu = wmi.QuerySelect("Processor", props);
```

- Filtros dinâmicos:
```csharp
wmi.QuerySelect("Service", new[] { "Name" }, "State='Running'");
```

- Consultas em fila:
```csharp
var queue = new WmiMethodInvokerQueue(invoker);
queue.Enqueue("Service", "StopService", "Name='Fax'");
queue.Enqueue("Service", "StopService", "Name='XboxGipSvc'");
queue.ExecuteAll();
```

---

## 6. Snippets Rápidos
- Valor único:
```csharp
wmi.GetScalar("OperatingSystem", "Caption");
```

- Várias propriedades:
```csharp
wmi.QuerySelect("Processor", new[] { "Name", "NumberOfCores" });
```

- Invocar método:
```csharp
invoker.Invoke("Service", "StopService", "Name='Spooler'");
```

---

## 7. Extensão da API
Adicionar novo método em `IWmiMethodInvoker`:
```csharp
public uint RestartService(string name)
{
    Invoke("Service", "StopService", $"Name='{name}'");
    return Invoke("Service", "StartService", $"Name='{name}'");
}
```

---

## 8. Batch Operations
```csharp
var queue = new WmiMethodInvokerQueue(invoker);
queue.Enqueue("DiskDrive", "SetPowerState", null, new { PowerState = 3, Time = 0 });
queue.Enqueue("Service", "StopService", "Name='Fax'");
queue.ExecuteAll();
```

---

## 9. Performance & Async
- Consultas pesadas → rodar em threads separadas.
- Versão async:
```csharp
await Task.Run(() => wmi.QueryAll("Process"));
```
- Em UI → sempre `await`.

---

## 10. Exemplos Avançados
### Monitor em tempo real
```csharp
while (true)
{
    var usage = wmi.GetScalar("Processor", "LoadPercentage");
    Console.WriteLine($"CPU: {usage}%");
    Thread.Sleep(1000);
}
```

### Dashboard de fingerprint
- Salvar fingerprint em JSON
- Consumir em frontend (React, Angular)

### Automação com PowerShell
```powershell
dotnet run --project SystemInfo -- get-scalar OperatingSystem Version
```

---

## 📌 Nota Importante
Os exemplos deste guia incluem:
- **Chamadas reais já suportadas** (`GetScalar`, `QueryAll`, `Invoke`, etc.).
- **Extensões possíveis** que o desenvolvedor pode implementar sobre a API (ex: `RestartService`, exportar dados, dashboards).

Essas extensões são apenas **aplicações práticas** da API já existente.
