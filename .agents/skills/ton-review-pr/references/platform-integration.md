# Integração com plataformas de hospedagem de código

## 1. Objetivo

Este documento define como a skill `ton-review-pr` deve interagir com plataformas de hospedagem de código para consultar Pull Requests (PRs) e Merge Requests (MRs), obter alterações e publicar comentários de revisão.

A skill deve priorizar os mecanismos de integração já disponíveis no ambiente do usuário, respeitando autenticação, permissões e limitações de cada plataforma.

As operações devem ser realizadas em nome do usuário autenticado, sem solicitar ou expor credenciais desnecessariamente.

## 2. Plataformas suportadas

A skill deve reconhecer e trabalhar com as seguintes plataformas:

- **GitHub:** Pull Requests.
- **Azure DevOps:** Pull Requests em repositórios do Azure Repos.
- **GitLab:** Merge Requests.
- **Bitbucket:** Pull Requests.

Não confundir Git com uma plataforma de hospedagem. Git é um sistema de controle de versão e, isoladamente, não identifica onde o PR ou MR está hospedado.

A implementação deve considerar que cada plataforma possui APIs, modelos de dados, mecanismos de autenticação e formatos de comentários próprios.

## 3. Descoberta da plataforma

Antes de iniciar a revisão, identificar automaticamente a plataforma utilizada pelo PR ou MR.

Utilizar, preferencialmente, as seguintes informações:

- URL do PR ou MR.
- URL do repositório remoto.
- Contexto da tarefa ou solicitação recebida.
- Ferramentas e integrações disponíveis no ambiente.
- Metadados retornados pelas ferramentas de acesso ao repositório.

Exemplos de identificação:

| Plataforma | Indicadores comuns |
|---|---|
| GitHub | `github.com`, Pull Request |
| Azure DevOps | `dev.azure.com`, `visualstudio.com`, Azure Repos |
| GitLab | `gitlab.com`, Merge Request |
| Bitbucket | `bitbucket.org`, Pull Request |

Os domínios são indicadores, não uma garantia absoluta. Instâncias corporativas, servidores próprios e domínios personalizados também devem ser considerados.

### 3.1. Quando não for possível identificar

Se não houver evidências suficientes para identificar a plataforma:

1. Não presumir qual plataforma está sendo utilizada.
2. Não tentar publicar comentários utilizando uma integração escolhida aleatoriamente.
3. Perguntar ao usuário qual plataforma hospeda o PR ou MR.
4. Retomar o processo após obter a informação.

## 4. Descoberta dos mecanismos de integração

Após identificar a plataforma, verificar quais mecanismos estão disponíveis no ambiente para interagir com ela.

Priorizar, conforme disponibilidade:

1. Integrações ou conectores já autenticados.
2. Ferramentas nativas disponibilizadas pelo ambiente de desenvolvimento.
3. CLI oficial da plataforma, quando instalada e autenticada.
4. API oficial da plataforma, quando houver mecanismo seguro e autorizado para acessá-la.

Não presumir que uma ferramenta, CLI, extensão ou conector esteja instalado ou configurado.

Antes de utilizar qualquer mecanismo:

- Verificar se ele permite consultar o PR ou MR.
- Verificar se permite obter o diff e os metadados necessários.
- Verificar se permite publicar comentários de revisão.
- Identificar as permissões efetivamente disponíveis.
- Utilizar somente as operações necessárias à revisão.

Se houver mais de um mecanismo disponível, preferir aquele que ofereça integração mais confiável e suporte adequado a comentários inline.

## 5. Autenticação e permissões

A publicação deve utilizar a identidade do usuário autenticado na plataforma.

Regras obrigatórias:

- Não solicitar senhas, tokens ou credenciais diretamente no chat.
- Não imprimir credenciais nos logs ou relatórios.
- Não armazenar tokens em arquivos do projeto.
- Não tentar contornar mecanismos de autenticação.
- Não ampliar permissões por conta própria.
- Respeitar as permissões concedidas ao usuário e à integração.
- Utilizar o menor nível de privilégio necessário.

Se a autenticação estiver ausente, expirada ou sem permissões suficientes, informar claramente o problema e orientar o usuário a configurar ou renovar a autenticação pelo mecanismo apropriado.

A skill não deve afirmar que está publicando em nome do usuário se a plataforma estiver utilizando outra identidade.

## 6. Obtenção dos dados do PR/MR

Antes de revisar, obter os dados necessários para compreender o contexto da alteração.

Sempre que disponíveis, consultar:

- Identificador e título do PR/MR.
- Descrição.
- Repositório e projeto.
- Branch de origem e branch de destino.
- Autor.
- Estado atual.
- Commit de origem.
- Commit de destino ou referência equivalente.
- Arquivos alterados.
- Diff completo.
- Informações de paginação.

O diff deve ser obtido de forma que permita identificar precisamente:

- Arquivo afetado.
- Linhas adicionadas.
- Linhas removidas.
- Linhas de contexto.
- Correspondência entre linhas do diff e linhas do arquivo.
- Commit ao qual a análise está vinculada.

Não basear a revisão exclusivamente na descrição do PR/MR ou em resumos fornecidos pela plataforma.

Quando necessário, consultar os arquivos completos e o contexto do projeto para compreender as alterações.

### 6.1. Paginação e limites

As APIs podem limitar a quantidade de arquivos, comentários ou alterações retornadas em uma única requisição.

A skill deve:

- Respeitar os mecanismos de paginação da plataforma.
- Consultar todas as páginas necessárias.
- Identificar respostas incompletas.
- Respeitar limites de requisições.
- Utilizar mecanismos de espera e repetição quando apropriados.
- Não considerar uma resposta parcial como se representasse o PR/MR inteiro.

Se não for possível obter todo o conteúdo necessário, informar a limitação e delimitar o escopo efetivamente revisado.

## 7. Preparação dos comentários

Os achados devem ser analisados e classificados antes de qualquer tentativa de publicação.

Cada comentário deve conter:

- Severidade.
- Classificação: correção necessária ou sugestão de melhoria.
- Explicação objetiva do problema ou oportunidade.
- Impacto potencial.
- Recomendação prática.

O idioma obrigatório é português brasileiro.

### 7.1. Formato recomendado

Utilizar um padrão consistente:

`[SEVERIDADE] [CLASSIFICAÇÃO] Título do achado`

Exemplo:

`[ALTO] [CORREÇÃO NECESSÁRIA] Possível perda de dados durante a atualização`

Corpo do comentário:

- Descrever o cenário que provoca o problema.
- Explicar por que o comportamento é inadequado.
- Apresentar uma recomendação de correção.
- Incluir exemplos quando realmente ajudarem na compreensão.

Evitar comentários genéricos, vagos ou excessivamente extensos.

### 7.2. Comentários inline

Sempre que possível, publicar o comentário diretamente na linha ou no trecho do código relacionado ao achado.

Para isso, utilizar as informações de posicionamento exigidas pela plataforma.

É necessário respeitar as particularidades de cada integração:

- Algumas plataformas utilizam posições relativas ao diff.
- Outras exigem números de linha e caminhos de arquivo.
- Algumas exigem referências específicas aos commits de origem e destino.
- Determinadas APIs exigem coordenadas ou identificadores próprios para comentários em alterações.

Não presumir que o número de linha apresentado no diff seja automaticamente válido para a API de comentários.

### 7.3. Posicionamento inválido

Se o comentário não puder ser associado corretamente a uma linha:

1. Verificar se a linha pertence ao diff publicado.
2. Verificar se o arquivo e os commits correspondem à versão atual do PR/MR.
3. Consultar o formato de posicionamento exigido pela plataforma.
4. Tentar novamente utilizando um posicionamento válido, caso seja possível.

Não publicar um comentário em uma linha arbitrária apenas para conseguir concluir a operação.

Se a plataforma permitir comentários gerais de revisão, utilizá-los somente quando forem adequados e deixar claro que o comentário não está associado a uma linha específica.

## 8. Publicação dos comentários

Utilizar o mecanismo oficial ou a integração disponível para publicar os comentários no PR/MR.

Regras:

1. Publicar somente achados efetivamente identificados durante a revisão.
2. Priorizar comentários inline.
3. Preservar o conteúdo e a classificação definidos durante a análise.
4. Não modificar o código do projeto.
5. Não criar commits.
6. Não realizar push.
7. Não aprovar ou rejeitar o PR/MR.
8. Não realizar merge.
9. Não alterar configurações do repositório.
10. Não executar ações adicionais que não sejam necessárias à revisão.

A publicação dos comentários de revisão não deve ser confundida com a aprovação formal do PR/MR.

### 8.1. Publicação em lotes

Se houver muitos comentários, avaliar a possibilidade de publicá-los em lotes, respeitando os limites da plataforma.

A publicação em lotes deve permitir identificar:

- Comentários publicados com sucesso.
- Comentários que falharam.
- Comentários ainda não processados.

Uma falha em um comentário não deve provocar a perda de informações sobre os demais.

## 9. Prevenção de comentários duplicados

Antes de publicar um comentário, verificar se já existe um comentário equivalente no PR/MR.

Considerar, quando disponíveis:

- Arquivo.
- Trecho ou linha relacionada.
- Conteúdo do achado.
- Severidade.
- Classificação.
- Identificação do commit.
- Autor e comentários anteriormente publicados pela própria skill.

Não depender exclusivamente de comparação textual. A mesma mensagem pode aparecer em linhas diferentes e representar problemas distintos.

Se a plataforma oferecer identificadores ou mecanismos de idempotência, utilizá-los quando apropriado.

### 9.1. Comportamento em caso de repetição

Se um comentário equivalente já tiver sido publicado:

- Não publicar novamente.
- Considerar o achado como já registrado.
- Evitar gerar ruído no PR/MR.

Não presumir que uma publicação falhou apenas porque a ferramenta retornou uma resposta inesperada.

Antes de repetir uma operação cujo resultado seja incerto, consultar os comentários existentes para verificar se a primeira tentativa foi concluída.

## 10. Tratamento de falhas

Se a publicação falhar, investigar a causa antes de tentar novamente.

Possíveis causas:

- Falha de autenticação.
- Permissões insuficientes.
- Token expirado.
- Limite de requisições.
- Indisponibilidade da plataforma.
- Comentário com posicionamento inválido.
- Commit ou diff desatualizado.
- Formato de requisição incompatível.
- Limitação da integração utilizada.

### 10.1. Estratégia de recuperação

Seguir esta ordem:

1. Identificar a operação que falhou.
2. Interpretar a mensagem de erro.
3. Verificar autenticação e permissões.
4. Verificar se o PR/MR ou o diff foi atualizado.
5. Verificar se a API ou integração exige outro formato de posicionamento.
6. Tentar novamente quando houver uma correção objetiva para a causa.
7. Caso exista outro mecanismo de integração disponível, avaliar sua utilização.
8. Se não houver possibilidade segura de publicação, interromper as tentativas e apresentar os achados no chat.

Não realizar tentativas repetitivas sem uma hipótese concreta de correção.

### 10.2. Resultado incerto

Se a requisição de publicação sofrer timeout ou retornar um erro que não permita determinar se o comentário foi criado:

1. Consultar novamente os comentários do PR/MR.
2. Procurar o comentário correspondente.
3. Se ele existir, considerá-lo publicado.
4. Se não existir, avaliar uma nova tentativa.
5. Se não for possível confirmar, informar a situação como inconclusiva.

Nunca afirmar que a publicação falhou ou teve sucesso sem evidências suficientes.

## 11. Confirmação da publicação

Após publicar os comentários, confirmar o resultado utilizando a resposta da plataforma ou uma consulta posterior aos comentários.

O relatório final deve distinguir claramente:

- **Publicado:** a plataforma confirmou a criação do comentário.
- **Já existente:** o comentário equivalente já estava publicado.
- **Falhou:** houve confirmação de que a publicação não foi concluída.
- **Inconclusivo:** não foi possível determinar o resultado.

Sempre que possível, registrar a quantidade de comentários em cada situação.

Não utilizar mensagens genéricas de sucesso quando apenas parte dos comentários tiver sido publicada.

## 12. Fallback: apresentação no chat

Se não for possível publicar os comentários após as tentativas de recuperação, apresentar todos os achados diretamente no chat.

Nenhum achado identificado durante a revisão deve ser descartado apenas porque a integração falhou.

Para cada achado, informar:

- Severidade.
- Classificação.
- Arquivo.
- Linha ou trecho relacionado.
- Descrição do problema ou oportunidade.
- Impacto.
- Recomendação.

Exemplo:

**[ALTO] [CORREÇÃO NECESSÁRIA] Possível consulta sem filtro de autorização**

- **Arquivo:** `Application/Handlers/GetPatientHandler.cs`
- **Linha:** 42
- **Problema:** a consulta utiliza o identificador recebido na requisição sem verificar se o usuário possui acesso ao registro.
- **Impacto:** um usuário autenticado pode conseguir consultar informações de outro usuário.
- **Recomendação:** validar a autorização do usuário sobre o registro antes de retornar os dados.

Ao utilizar o fallback, explicar brevemente por que a publicação não foi possível e quais alternativas foram tentadas.

## 13. Segurança operacional

A integração com as plataformas deve seguir os princípios de menor privilégio e mínima intervenção.

A skill deve:

- Utilizar somente as informações necessárias à revisão.
- Não expor dados sensíveis encontrados nos arquivos.
- Não reproduzir segredos em comentários ou relatórios.
- Evitar enviar conteúdo do repositório para serviços externos não autorizados.
- Não executar comandos destrutivos.
- Não modificar arquivos do projeto.
- Não alterar configurações de autenticação.
- Não tentar obter permissões adicionais sem autorização do usuário.

Se encontrar credenciais ou segredos aparentemente expostos no código, tratar o achado como um possível incidente de segurança e evitar reproduzir o valor encontrado no comentário.

## 14. Critérios de conclusão

A integração será considerada concluída quando a skill conseguir:

1. Identificar corretamente a plataforma.
2. Acessar o PR/MR utilizando a identidade autenticada.
3. Obter os metadados e as alterações necessárias.
4. Preparar comentários com posicionamento válido.
5. Publicar os comentários permitidos.
6. Verificar os resultados.
7. Apresentar um relatório fiel ao que realmente aconteceu.

Se alguma etapa não puder ser executada, a skill deve informar a limitação sem mascarar o resultado.

## 15. Princípio fundamental

A prioridade é entregar uma revisão tecnicamente correta, confiável e útil.

Publicar comentários é importante, mas não é mais importante do que preservar a precisão da análise, a segurança do repositório e a confiança do usuário.

**Nunca inventar uma publicação, nunca duplicar comentários deliberadamente e nunca sacrificar a qualidade da revisão para concluir uma integração.**