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

- [Manual do usuário](docs/USER_MANUAL.md)
- [Instalação e execução](docs/INSTALL.md)
- [Guia do desenvolvedor](docs/MANUAL_DEV.md)
- [Referência da API](docs/API_GUIDE.md)



---

## 📌 Nota Importante
Este projeto já fornece uma API pronta para:
- Consultas (`GetScalar`, `QuerySelect`, `QueryAll`).
- Invocação de métodos WMI (`Invoke`).
- Manipulação de serviços/processos/discos.

Alguns exemplos mostrados na documentação (ex.: exportar JSON, integração com API REST, jobs em agendador, dashboards) **não são métodos nativos**, mas **cenários de uso/integração possíveis** com a API já existente.
