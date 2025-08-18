# Guia de Instalação – SystemInfo

## 1. Requisitos
- **Sistema Operacional**: Windows 10 ou superior.
- **.NET**: SDK ou Runtime do .NET 6 instalado.
- **Permissões**: privilégios de administrador são necessários para:
  - Parar/alterar serviços.
  - Alterar prioridade de processos.
  - Otimizar partições de disco.

---

## 2. Instalação
1. Clone ou baixe o repositório:
   ```bash
   git clone https://github.com/seu-usuario/SystemInfo.git
   cd SystemInfo
   ```
2. Compile o projeto:
   ```bash
   dotnet build
   ```
3. Execute:
   ```bash
   dotnet run
   ```

---

## 3. Estrutura de Pastas
- **/Core** → Interfaces principais (`IWmiQueryService`, `IWmiMethodInvoker`).
- **/Configuration** → Mapas de classes/propriedades WMI.
- **/Features** → Funcionalidades (serviços, processos, disco, fingerprint).
- **/Infrastructure** → Implementações concretas (WMI, helpers, filas).
- **/Presentation** → Menus interativos.
- **Program.cs** → Entrada do programa.

---

## 4. Executando como Administrador
Muitas funcionalidades (ex.: parar serviços, mudar prioridades, otimizar discos) **exigem execução como administrador**.  
Para garantir:
- No PowerShell:
  ```powershell
  Start-Process "dotnet" "run" -Verb RunAs
  ```

---

## 5. Problemas Comuns
- **Erro de acesso negado** → Execute como administrador.
- **Classe WMI não encontrada** → Certifique-se de estar no Windows 10+.
- **Menu não responde** → Confira se o terminal aceita entrada interativa.

---

## 📌 Nota Importante
Este guia cobre:
- **Passos reais de instalação e execução** do projeto.
- **Permissões necessárias** para funcionalidades críticas.

Outros exemplos da documentação (como integração em APIs REST, exportação de JSON ou execução via agendadores) **não fazem parte da instalação padrão**, mas são cenários de uso possíveis aproveitando a API já fornecida pelo projeto.
