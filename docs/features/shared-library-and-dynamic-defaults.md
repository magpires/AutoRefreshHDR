# Biblioteca `Shared` e Preenchimento Dinâmico de Configurações do Monitor

## Motivação
Ao adicionar um novo programa no `ConfigEditor`, os campos de Refresh Rate (`null`) e Brilho (`100`) não refletiam o estado atual do monitor do usuário. Como o projeto `AutoRefreshHDR` já possuía toda a lógica de leitura de taxa de atualização e brilho (via DDC/CI), era necessário reutilizar essa capacidade no `ConfigEditor` sem acoplamento inadequado entre dois executáveis (`WinExe`) e sem duplicar os modelos de configuração.

## Decisões Tomadas
- **Criação do projeto `Shared` (Class Library):**
  - Descartada a referência direta de `ConfigEditor` $\to$ `AutoRefreshHDR`, pois referenciar um `WinExe` a partir de outro `WinExe` gera acoplamento entre hosts, conflito de `TargetFramework` (`net8.0-windows` vs `net8.0-windows10.0.22621.0`) e vazamento de artefatos de build (`.bat`, `Utils/`, `LocalStorage`).
  - Centralizados em `src/Shared/` os modelos (`DisplayConfig`, `ProgramDisplayConfig`, `DisplaySettingsManager`) e o serviço `DisplaySettingsManagerService`, eliminando a duplicação de modelos que existia entre `AutoRefreshHDR` e `ConfigEditor`.
  - Configurado `<SupportedOSPlatformVersion>7.0</SupportedOSPlatformVersion>` em `Shared.csproj`, eliminando alertas `CA1416` na compilação.
- **Métodos `TryGet...` sem efeitos colaterais de UI:**
  - Adicionados `TryGetCurrentRefreshRate(out uint refreshRate)` e `TryGetCurrentBrightness(out uint brightnessPercentage)` em `DisplaySettingsManagerService`, garantindo também a liberação de handles físicos via `try/finally` com `DestroyPhysicalMonitors`.
  - Os métodos originais (`GetCurrentRefreshRate` e `GetCurrentBrightness`) foram mantidos delegando aos novos métodos para preservar compatibilidade com o `AutoRefreshHDR`.
- **Valores padrão com fallback no `ConfigEditor`:**
  - Criado o helper `GetDefaultDisplaySettings()` em `ConfigEditorForm`, usado tanto por `AddRow()` quanto por `AddRowByFile()`.
  - Caso a leitura do monitor falhe (ex.: monitor sem suporte a DDC/CI ou retorno `0` de taxa de atualização), aplica-se o fallback **60 Hz** para Refresh Rate e **50%** para Brilho.

## Resultados Esperados
- Código compartilhado em ponto único de verdade (`Shared`), compilando todos os projetos com 0 erros e 0 avisos.
- Novos registros no `ConfigEditor` já iniciam preenchidos com a taxa de atualização e o brilho atuais do monitor (ou com o fallback 60 Hz / 50% quando indisponíveis).

## Arquivos Relacionados
- [`AutoRefreshHDR.sln`](../../AutoRefreshHDR.sln)
- [`src/Shared/Shared.csproj`](../../src/Shared/Shared.csproj)
- [`src/Shared/Models/DisplayConfig.cs`](../../src/Shared/Models/DisplayConfig.cs)
- [`src/Shared/Models/ProgramDisplayConfig.cs`](../../src/Shared/Models/ProgramDisplayConfig.cs)
- [`src/Shared/Models/DisplaySettingsManager.cs`](../../src/Shared/Models/DisplaySettingsManager.cs)
- [`src/Shared/Services/DisplaySettingsManagerService.cs`](../../src/Shared/Services/DisplaySettingsManagerService.cs)
- [`src/AutoRefreshHDR/AutoRefreshHDR.csproj`](../../src/AutoRefreshHDR/AutoRefreshHDR.csproj)
- [`src/AutoRefreshHDR/Program.cs`](../../src/AutoRefreshHDR/Program.cs)
- [`src/ConfigEditor/ConfigEditor.csproj`](../../src/ConfigEditor/ConfigEditor.csproj)
- [`src/ConfigEditor/ConfigEditorForm.cs`](../../src/ConfigEditor/ConfigEditorForm.cs)
- [`src/ConfigEditor/Services/ConfigService.cs`](../../src/ConfigEditor/Services/ConfigService.cs)
