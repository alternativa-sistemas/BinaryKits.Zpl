# TestConsole - Testes Delphi para BinaryKits.Zpl

Esta pasta contém exemplos de código Delphi para testar a integração com o `BinaryKits.Zpl.NativeWrapper`.

## Arquivos de Teste

### Testes Locais (DLL)
- **TestZpl.dpr** - Teste de conversão ZPL para PNG usando a DLL nativa
  - Demonstra chamada direta da função `ConvertZplToPng`
  - Inclui tratamento de exceções FPU/SSE
  - Gera arquivo `delphi_test.png` como saída

### Testes Web API
- **TestWebApi.dpr** - Teste de conversão ZPL usando a Web API
  - Usa `ZplApiClient.pas` para comunicação HTTP
  - Não requer DLLs locais
  - Ideal para aplicações distribuídas

## Como Preparar o Ambiente de Testes

### 1. Compilar os Projetos para x86

Na pasta `src\`, execute:

```powershell
.\build-x86.ps1
```

### 2. Copiar DLLs para TestConsole

Na pasta `tests\TestConsole\`, execute:

```batch
copy-dlls.bat
```

Este script copia automaticamente:
- Todas as DLLs necessárias
- Bibliotecas nativas (x86, x64, arm64)
- Arquivos PDB para debug

### 3. Compilar e Executar os Testes

Abra o projeto no Delphi:
- `TestZpl.dproj` - Para teste com DLL
- `TestWebApi.dpr` - Para teste com Web API

## Estrutura de Arquivos

```
TestConsole/
├── TestZpl.dpr              # Teste DLL
├── TestWebApi.dpr           # Teste Web API
├── ZplApiClient.pas         # Cliente HTTP para Web API
├── Program.cs               # Teste C# (opcional)
├── copy-dlls.bat            # Script para copiar DLLs
├── .gitignore               # Ignora DLLs e builds
└── README.md                # Este arquivo
```

## Notas Importantes

### DLLs não são versionadas
As DLLs e dependências **não** são incluídas no Git. Use `copy-dlls.bat` para obtê-las após clonar o repositório.

### Requisitos
- .NET Framework 4.7.2 ou superior
- Delphi (testado com Delphi 10.4 Sydney)
- Windows (32-bit ou 64-bit)

### Troubleshooting

**Erro: "Não foi possível carregar a DLL"**
- Execute `copy-dlls.bat` para copiar as DLLs
- Verifique se o .NET Framework 4.7.2 está instalado

**Erro: "Diferença de versão de plataforma"**
- Certifique-se de que usou `build-x86.ps1` para compilar
- Verifique se todas as DLLs estão na pasta TestConsole

**Exceções de ponto flutuante**
- O código já inclui `SetExceptionMask(exAllArithmeticExceptions)`
- Isso é necessário porque SkiaSharp usa instruções SSE
