# SystemInfo

SystemInfo é uma ferramenta em .NET para coletar, exibir e manipular informações do sistema Windows utilizando **WMI (Windows Management Instrumentation)**.

## Funcionalidades
- Exibir fingerprint do sistema (CPU, memória, disco, rede, etc.).
- Consultar classes WMI e visualizar suas propriedades.
- Parar serviços desnecessários do Windows.
- Ajustar prioridades de processos com base em uso.
- Testar adaptadores de rede (execução real e modo dry-run).
- Otimizar partições de disco.

## Estrutura do Projeto
- **Core** → Abstrações (`IWmiQueryService`, `IWmiMethodInvoker`).
- **Configuration** → Mapas de classes/propriedades WMI.
- **Features** → Funcionalidades (serviços, disco, fingerprint).
- **Infrastructure** → Implementações WMI, fila de invocações, helpers.
- **Presentation** → Menus interativos.
- **Program.cs** → Ponto de entrada.

## Documentação
- [`USER_MANUAL.md`](USER_MANUAL.md) → Manual do usuário.
- [`INSTALL.md`](INSTALL.md) → Instalação e execução.
- [`MANUAL_DEV.md`](MANUAL_DEV.md) → Guia do desenvolvedor.
- [`API_GUIDE.md`](API_GUIDE.md) → Referência da API.

---

## 📌 Nota Importante
Este projeto já fornece uma API pronta para:
- Consultas (`GetScalar`, `QuerySelect`, `QueryAll`).
- Invocação de métodos WMI (`Invoke`).
- Manipulação de serviços/processos/discos.

Alguns exemplos mostrados na documentação (ex.: exportar JSON, integração com API REST, jobs em agendador, dashboards) **não são métodos nativos**, mas **cenários de uso/integração possíveis** com a API já existente.
