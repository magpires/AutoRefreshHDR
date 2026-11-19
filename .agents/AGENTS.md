# Regras permanentes de desenvolvimento

Estas são minhas preferências permanentes para os agentes de IA no projeto. Mantenha-as durante a conversa, novos prompts, retomadas e compactações de contexto. Só altere estas preferências por uma instrução explícita minha. Respeite a hierarquia de instruções do Agente de IA e informe conflitos que afetem a execução.

## 1. Branch e início do trabalho

- Antes de modificar código em um repositório Git, verifique a branch atual e as alterações existentes, inclusive as já presentes no staging.
- Se a branch for `develop`, `main` ou `master`, atualize-a com `git pull --ff-only` e, em seguida, pergunte o nome da nova branch antes de criá-la. Aguarde minha resposta para criar a branch. Após a branch criada, faça uma nova análise do repositório antes de implementar as modificações no código.
- Use exatamente o nome de branch que eu informar.
- Se o pull exigir merge, houver conflitos ou a atualização ameaçar alterações preexistentes, interrompa essa operação e explique o impedimento. Não use stash, reset ou descarte de alterações como solução automática.
- Se a branch atual for outra, trabalhe nela, salvo instrução explícita minha em contrário.
- Durante a espera pelo nome da branch, você pode fazer leituras e análises que não alterem o repositório.

## 2. Alterações locais e publicação

- Por padrão, não faça commits, push, publicação de pacotes, criação de PRs, merges ou alterações em work items e rastreadores externos.
- Não adicione, remova ou prepare alterações no staging sem minha autorização explícita. Preserve o staging preexistente.
- Deixe suas alterações no diretório de trabalho para minha revisão manual. Eu revisarei o código, farei ajustes e cuidarei da publicação, seja eu mesmo commitando ou te solicitando.
- Uma solicitação explícita minha autoriza somente a ação solicitada e os passos indispensáveis à sua execução. Não interprete um pedido de implementação, correção, teste ou conclusão como autorização para publicar.
- Se uma etapa exigir uma ação restrita ainda não autorizada, conclua primeiro o trabalho local compatível, apresente o resultado revisável e peça minha autorização antes de executar a ação restrita.
- A atualização com `git pull --ff-only` prevista na seção 1 é uma exceção autorizada à restrição de operações Git; ela não autoriza merges manuais nem commits de merge.

## 3. Preservação do trabalho existente

- Preserve alterações preexistentes, arquivos não rastreados e alterações já no staging.
- Não reverta, sobrescreva nem descarte trabalho que não seja seu.
- Se suas alterações e as minhas estiverem no mesmo arquivo, preserve as minhas. Se não conseguir separá-las com segurança, explique a situação antes de alterar a parte afetada.

## 4. Instruções e skills

- Consulte e siga os arquivos `AGENTS.md` e overrides aplicáveis ao projeto e aos arquivos envolvidos.
- Use as skills pertinentes disponíveis a cada etapa e leia seus `SKILL.md` antes de aplicá-las. Não aplique todas indiscriminadamente.
- Se houver dúvida real sobre qual skill utilizar, pergunte antes da etapa afetada. Liste as skills candidatas e resuma brevemente a função de cada uma. Continue apenas o trabalho independente dessa escolha.
- Minhas restrições explícitas prevalecem sobre orientações de skills para commit, push, publicação, PR ou atualização de rastreadores. Execute as partes compatíveis; solicite autorização somente para ações restritas ainda não autorizadas.
- Se uma skill necessária estiver ausente ou inacessível, informe qual é, explique o impacto e sugira alternativas para prosseguir. Se a ausência impedir a etapa, aguarde minha orientação para essa etapa.

## 5. Dúvidas e implementação

- Informe dúvidas relevantes sobre comportamento e requisitos antes de implementar a parte afetada.
- Resolva fatos descobríveis pela leitura do código, configurações, documentação e testes do repositório, sem me perguntar o que pode ser verificado localmente.
- Continue o trabalho independente das respostas pendentes, respeitando as restrições acima.
- Quando for solicitado a resolver comentários em PRs, sempre verifique antes de resolver se aquele comentário faz sentido e realmente precisa de correção. Você pode me consultar sempre que surgir alguma dúvida sobre algum comentário no PR, apresentar alternativas para solucionar ou deixar que eu mesmo diga como solucionar.

## 6. Commits autorizados

- Quando eu autorizar um commit, use Conventional Commits sempre que possível, por exemplo: `feat:`, `fix:`, `perf:`, `refactor:`, `docs:`, `test:` e `chore:`.
- Procure sempre escrever as mensagens de commit no verbo imperativo afirmativo (3ª pessoa do singular, como se estivesse dando uma instrução ou descrevendo o que o commit faz), por exemplo: "feat: **adiciona** validação de CPF"
- Inclua somente as alterações autorizadas. Preserve alterações e staging que não pertençam ao commit solicitado.

## 7. Pull requests autorizadas

- Quando eu solicitar explicitamente a abertura de uma PR, use a skill `ton-review-pr` após a criação da PR.
- A ideia de Conventional Commits com verbos imperativos também se aplica ao título dos PRs. Siga sempre que possível.
- Para PRs abertos, siga o fluxo aplicável de `ton-review-pr` dentro da autorização concedida pelo meu pedido, sem aprovar, fazer merge ou alterar work items automaticamente.
- Se a skill estiver ausente, inacessível ou não se aplicar ao provedor da PR, informe a limitação e proponha uma alternativa de revisão.
- Um pedido para revisar uma PR existente, por si só, não autoriza commits, push, merge ou publicação de uma nova PR.