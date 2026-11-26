# Melhorias de UX no `ConfigEditor`: Ordem das Colunas e Fluxo de Adição

## Motivação
1. **Prioridade visual (`Active`):** Ao revisar a lista de programas configurados, saber rapidamente quais perfis estão ativos é a informação mais consultada. Colocar `Active` ao final da grade dificultava a leitura.
2. **Entrada manual de aplicativos (Store/UWP):** O botão `+ Add` havia sido alterado para abrir diretamente um `OpenFileDialog`. Porém, executáveis de aplicativos da Microsoft Store/UWP não são facilmente acessíveis pelo explorador de arquivos padrão, obrigando o usuário a selecionar um `.exe` qualquer e renomeá-lo depois.

## Decisões Tomadas
- **Coluna `Active` em primeiro lugar:** Reordenada a criação de colunas no `DataGridView` em `ConfigEditorForm` para: `Active`, `Program Name`, `Refresh (Hz)`, `HDR`, `Brightness (0-100)` e `Only Fullscreen`.
- **Separação em dois botões na barra de ações (`+ Add` e `+ Add File...`):**
  - Descartada a ideia de uma coluna extra com botão `[...]` em cada linha, pois criaria ruído visual e exigiria dois cliques para adicionar um arquivo (criar linha + clicar na célula).
  - **`+ Add`:** Restaura o comportamento de adicionar uma linha limpa (`ProgramName = string.Empty`) e já seleciona/foca a célula `Program Name` da nova linha para digitação imediata.
  - **`+ Add File...`:** Mantém o fluxo prático via `OpenFileDialog` (com verificação de duplicatas) para executáveis tradicionais.
- **Correção na mensagem de duplicata:** Ajustada a interpolação da string de alerta para exibir corretamente o nome do arquivo (`'{fileName}'` em vez de chaves literais duplas).

## Resultados Esperados
- Leitura mais rápida do status de ativação de cada programa logo à esquerda da grade.
- Flexibilidade para cadastrar tanto aplicativos UWP/Store manualmente (`+ Add`) quanto executáveis convencionais via diálogo (`+ Add File...`), em um único clique.

## Arquivos Relacionados
- [`src/ConfigEditor/ConfigEditorForm.cs`](../../src/ConfigEditor/ConfigEditorForm.cs)
