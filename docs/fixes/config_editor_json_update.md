# Correção: ConfigEditor agora usa configuração JSON simples

## O que foi alterado
- O arquivo **ConfigEditorForm.cs** foi atualizado para carregar `appsettings.json` em vez de `appsettings.jsonc`.
- Ajustado o caminho padrão de configuração:
  ```csharp
  var localPath = Path.Combine(baseDir, "appsettings.json");
  ```
- Mensagens de erro foram atualizadas para referenciar o nome correto do arquivo.

## Por que foi feita
O projeto migrou de um arquivo de configuração JSONC para JSON puro (`appsettings.json`). O ConfigEditor ainda buscava o antigo `.jsonc`, o que resultava na falha ao localizar o arquivo de configuração em tempo de execução. Alinhar o editor ao novo nome garante que a UI carregue, edite e salve as configurações corretamente.

## Impacto
- O ConfigEditor passa a funcionar com o formato de configuração atual, sem necessidade de renomear arquivos manualmente.
- Não há mudanças funcionais na lógica principal; apenas o tratamento do caminho do arquivo e as mensagens ao usuário foram atualizados.
- Não há alterações incompatíveis; os arquivos de configuração existentes continuam válidos, já que o esquema JSON permanece o mesmo.

## Passos de verificação
1. Execute o executável do ConfigEditor.
2. Verifique se ele carrega `appsettings.json` a partir do diretório da aplicação sem erros.
3. Salve alterações e confirme que o arquivo é atualizado corretamente.

*Documentado conforme as diretrizes do `AGENTS.md` na pasta `docs/fixes` do projeto.*