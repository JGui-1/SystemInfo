# Manual do Usuário – SystemInfo

## 1. Introdução
SystemInfo é uma ferramenta em linha de comando para visualizar e gerenciar informações do sistema operacional Windows.  
Através de menus interativos, você pode inspecionar classes WMI, exibir fingerprints do sistema, controlar serviços e processos, e otimizar recursos.

---

## 2. Como Navegar
Ao executar o programa (`dotnet run`), o usuário verá um **menu principal** com opções numeradas.  
Use o teclado para digitar o número correspondente à funcionalidade desejada.

---

## 3. Funcionalidades

### 3.1 Fingerprint
- Lista classes de hardware e software (CPU, memória, disco, rede etc.).
- Permite selecionar quais classes e propriedades visualizar.
- Pergunta ao usuário se deseja ver todas as propriedades disponíveis.

### 3.2 Exibir Classes WMI
- Apresenta um menu interativo com todas as classes do `WmiClassMap`.
- Permite escolher uma classe e listar valores de propriedades.
- Pergunta se o usuário deseja ver todas as propriedades disponíveis.

### 3.3 Serviços
- Detecta serviços desnecessários do Windows.
- Pergunta se o usuário deseja pará-los.
- Exemplo: Fax, XPS, Xbox Services etc.

### 3.4 Processos
- Mostra processos em execução.
- Pergunta se o usuário deseja **reduzir prioridade** de processos pouco usados.
- Pergunta se deseja **aumentar prioridade** de processos críticos.

### 3.5 Disco
- Lista partições e espaço disponível.
- Oferece ferramenta básica de otimização.

### 3.6 Rede
- Permite testar adaptadores de rede.
- Dois modos:
  - **Execução real** (`NetworkAdapterTesterReal`)
  - **Dry-run** para testes sem efeito (`NetworkAdapterTesterDryRun`)

---

## 4. Requisitos
- Windows 10 ou superior.
- .NET 6 SDK ou runtime instalado.
- Permissões de administrador para gerenciar serviços/discos.

---

## 5. Exemplos de Uso
- **Exibir versão do Windows:**
  - Menu → Fingerprint → OperatingSystem → Property: Version
- **Parar o serviço de spooler:**
  - Menu → Serviços → Selecionar “Spooler”
- **Otimizar disco:**
  - Menu → Disco → Otimizar

---

## 📌 Nota Importante
Este manual mostra:
- **Funcionalidades reais** já disponíveis no menu (Fingerprint, Serviços, Processos, Disco, Rede).
- **Cenários opcionais de uso** que dependem da confirmação do usuário (parar serviços, mudar prioridade de processos etc.).

A API também pode ser usada diretamente em outros projetos para cenários avançados (como exportar fingerprint em JSON ou integrar a uma API REST). Esses casos **não estão no menu padrão**, mas podem ser implementados usando a API já fornecida pelo código.
