# Bloqueio de `Only Fullscreen` quando HDR está Ativado

## Motivação
Jogos e conteúdos em HDR geralmente dependem que o modo HDR do Windows esteja ativo de forma contínua para renderizar corretamente. Quando um perfil combinava `HDR = true` com `OnlyWhenFullscreen = true`, o HDR era ligado e desligado conforme a janela entrava ou saía de tela cheia, causando problemas de compatibilidade e artefatos visuais.

## Decisões Tomadas
- **Trava puramente na interface (`ConfigEditor`) em vez de ignorar em runtime:**
  - Optou-se por desmarcar (`false`) e bloquear (`ReadOnly = true`) a célula `OnlyWhenFullscreen` diretamente no `ConfigEditorForm` sempre que `Hdr` for marcado.
  - Isso evita adicionar verificações extras no loop contínuo do `AutoRefreshHDR` (custo zero de CPU no serviço em background) e garante que o `appsettings.jsonc` salvo seja sempre coerente com o comportamento real.
- **Eventos de sincronização na grade:**
  - `CurrentCellDirtyStateChanged`: realiza `CommitEdit` imediato ao clicar em células `DataGridViewCheckBoxCell`, garantindo que `CellValueChanged` dispare sem exigir que o usuário mude de célula.
  - `DataBindingComplete`: aplica o estado de bloqueio em todas as linhas ao carregar um arquivo de configuração existente.
  - `ApplyFullscreenLock(int rowIndex, bool isHdrEnabled)`: centraliza a lógica visual (fundo `SystemColors.Control`, texto `SystemColors.GrayText` e `ReadOnly`) e de valor (`false`).
- **Tooltip nativo por célula (`cell.ToolTipText`):**
  - Em vez de um componente `ToolTip` externo associado ao controle `_grid` inteiro (que não disparava de forma confiável ao mover o mouse entre células já dentro da grade), utilizou-se a propriedade nativa `DataGridViewCell.ToolTipText` (com `ShowCellToolTips = true` padrão do `DataGridView`).
  - Quando bloqueada, a célula exibe a mensagem: *"Only Fullscreen is not available when HDR is enabled. This option only works in SDR mode."*

## Resultados Esperados
- Ao marcar `HDR` para um programa, `Only Fullscreen` é automaticamente desmarcado, fica visualmente desabilitado (acinzentado) e exibe a explicação em inglês ao passar o mouse.
- Ao desmarcar `HDR`, `Only Fullscreen` volta a ficar editável e sem tooltip.
- Configurações salvas com `HDR = true` já carregam com `Only Fullscreen` bloqueado ao reabrir o `ConfigEditor`.

## Arquivos Relacionados
- [`src/ConfigEditor/ConfigEditorForm.cs`](../../src/ConfigEditor/ConfigEditorForm.cs)
