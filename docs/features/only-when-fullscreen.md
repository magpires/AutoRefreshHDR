# Aplicação Condicionada a Tela Cheia (`OnlyWhenFullscreen`)

## Motivação
Alguns aplicativos (como navegadores ou reprodutores de mídia) são utilizados em janela normal durante tarefas cotidianas e em tela cheia apenas para consumo de mídia ou jogos. Alterar brilho, taxa de atualização ou HDR assim que o processo é aberto causava mudanças indesejadas fora do modo tela cheia.

## Decisões Tomadas
- **Detecção nativa via Win32 API (P/Invoke):** Em vez de adicionar dependências externas, a verificação foi implementada em `DisplaySettingsManagerService.IsProcessFullscreen(string processName)` utilizando `GetWindowRect`, `MonitorFromWindow` e `GetMonitorInfo` (`user32.dll`). Uma janela é considerada em tela cheia quando seus limites (`RECT`) cobrem integralmente `rcMonitor` do monitor em que está exibida.
- **Restauração imediata ao sair de tela cheia (Opção A):** No loop de monitoramento de `AutoRefreshHDR/Program.cs`, além de restaurar as configurações quando o processo é encerrado, o serviço verifica a cada segundo se o processo configurado com `OnlyWhenFullscreen = true` saiu do modo tela cheia. Caso saia, restaura imediatamente HDR, Refresh Rate e Brilho originais, limpa o `LocalStorage` e retorna ao loop de espera — permitindo reapplication automática caso o usuário volte para tela cheia.
- **Persistência:** Adicionada a propriedade booleana `OnlyWhenFullscreen` em `ProgramDisplayConfig` (padrão `false` para preservar compatibilidade com configurações existentes) e a coluna `"Only Fullscreen"` no `ConfigEditor`.

## Resultados Esperados
- Programas com `OnlyWhenFullscreen = false` mantêm o comportamento tradicional (alteram configurações ao abrir e restauram ao fechar).
- Programas com `OnlyWhenFullscreen = true` só disparam alterações de display quando estão efetivamente em tela cheia e revertem imediatamente ao sair da tela cheia, mesmo com o aplicativo ainda aberto.

## Arquivos Relacionados
- [`src/Shared/Models/ProgramDisplayConfig.cs`](../../src/Shared/Models/ProgramDisplayConfig.cs)
- [`src/Shared/Services/DisplaySettingsManagerService.cs`](../../src/Shared/Services/DisplaySettingsManagerService.cs)
- [`src/AutoRefreshHDR/Program.cs`](../../src/AutoRefreshHDR/Program.cs)
- [`src/ConfigEditor/ConfigEditorForm.cs`](../../src/ConfigEditor/ConfigEditorForm.cs)
