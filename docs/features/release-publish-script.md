# Script de Publicação de Releases (`scripts/publish-release`)

## Motivação
Publicar manualmente ambos os projetos (`AutoRefreshHDR` e `ConfigEditor`) pelo terminal ou Visual Studio com os mesmos parâmetros de runtime e destino era repetitivo e sujeito a erros. O script automatiza a criação de releases versionadas no padrão `releases/x.x.x`.

## Decisões Tomadas
- **Combinação PowerShell + Batch (`.ps1` + `.bat`) na pasta `scripts/`:**
  - `publish-release.ps1` concentra toda a lógica de versionamento semântico, criação/limpeza de pastas, confirmações interativas e execução do `dotnet publish`.
  - `publish-release.bat` funciona como atalho executável com duplo clique no Windows Explorer, chamando o PowerShell com `-ExecutionPolicy Bypass` e mantendo a janela aberta ao final (`pause`).
  - A pasta `releases/` já é ignorada pelo `.gitignore` (`[Rr]eleases/`), enquanto os scripts em `scripts/` permanecem versionados no repositório.
- **Estratégia de versionamento (`releases/x.x.x`):**
  - Cria automaticamente a pasta `releases/` na raiz do repositório caso não exista.
  - Se não houver pastas no formato `x.x.x`, sugere `1.0.0`.
  - Se já houver versões, identifica a maior versão semântica e sugere o incremento de `+1` no *patch* (`Major.Minor.Patch+1`).
  - Permite ao usuário informar manualmente outra versão no formato `x.x.x`.
  - Se a versão escolhida já existir, solicita confirmação explícita antes de apagar todo o conteúdo da pasta e republicar.
- **Parâmetros de publicação:**
  - Ambos os projetos são publicados na mesma pasta `releases/x.x.x` (necessário para que o `ConfigEditor` encontre `appsettings.jsonc` e `RestartAutoRefreshHDR.bat` no mesmo diretório).
  - Configuração: `-c Release`, runtime `-r win-x64`, dependente do framework (`--self-contained false`), arquivo único (`-p:PublishSingleFile=true`) e compilação antecipada ReadyToRun (`-p:PublishReadyToRun=true`).

## Resultados Esperados
- Geração rápida e padronizada de releases completas contendo os executáveis de arquivo único (`AutoRefreshHDR.exe` e `ConfigEditor.exe` compilados em ReadyToRun), `Utils/hdr_switch_tray.exe`, scripts `.bat` e `appsettings.jsonc` prontos para distribuição em `releases/x.x.x`.

## Arquivos Relacionados
- [`scripts/publish-release.ps1`](../../scripts/publish-release.ps1)
- [`scripts/publish-release.bat`](../../scripts/publish-release.bat)
