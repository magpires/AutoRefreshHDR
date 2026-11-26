# Correção na Detecção de Tela Cheia sem Mudança na Quantidade de Processos

## Motivação
O loop principal de `AutoRefreshHDR/Program.cs` aguardava uma alteração no número total de processos do sistema (`while (processCount == Process.GetProcesses().Length) Thread.Sleep(1000);`) antes de executar a varredura de `ProgramDisplayConfigs`.

Como consequência, quando um programa configurado com `OnlyWhenFullscreen = true` já estava aberto em janela normal e o usuário entrava em tela cheia (ex.: `F11` ou vídeo em tela cheia), nenhum processo novo era criado ou encerrado. O serviço permanecia preso no loop de espera e não aplicava as configurações de monitor. O mesmo ocorria ao sair de tela cheia e tentar reentrar com o aplicativo ainda aberto.

## Decisões Tomadas
- **Espera combinada (`processCount` + `HasAnyFullscreenProgram`):**
  - Filtrados na inicialização os perfis ativos com `OnlyWhenFullscreen = true` (`fullscreenConfigs`).
  - O loop de espera continua dormindo `1000 ms` por iteração (em `ProcessPriorityClass.Idle`), mas agora sai da espera se `processCount` mudar **ou** se `HasAnyFullscreenProgram(fullscreenConfigs)` retornar `true`.
  - Caso não haja programas ativos com `OnlyWhenFullscreen = true`, o custo adicional é nulo (o array `fullscreenConfigs` fica vazio).
- **Prevenção de vazamento de handles (`Dispose`):**
  - Adicionado `GetProcessCount()` em `Program.cs` e bloco `try/finally` em `DisplaySettingsManagerService.IsProcessFullscreen(string)` para chamar `Dispose()` em todos os objetos `Process` retornados por `Process.GetProcesses()` e `Process.GetProcessesByName()`, evitando acúmulo de handles nativos durante o polling contínuo a cada 1 segundo.

## Resultados Esperados
- Aplicativos configurados com `OnlyWhenFullscreen = true` que já estejam abertos em janela normal passam a aplicar as configurações de monitor em até ~1 segundo após entrarem em tela cheia, sem depender da abertura ou fechamento de outros processos no Windows.
- A reentrada em tela cheia após uma restauração anterior também passa a funcionar normalmente.
- Consumo de CPU e memória mantém-se estável e baixo.

## Arquivos Relacionados
- [`src/AutoRefreshHDR/Program.cs`](../../src/AutoRefreshHDR/Program.cs)
- [`src/Shared/Services/DisplaySettingsManagerService.cs`](../../src/Shared/Services/DisplaySettingsManagerService.cs)
