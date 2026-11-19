# Exemplos de comentários de revisão

## 1. Objetivo

Este documento apresenta exemplos práticos de comentários que a skill `ton-review-pr` deve produzir durante revisões de Pull Requests e Merge Requests.

O objetivo é estabelecer um padrão de qualidade para os comentários, garantindo que sejam:

- Tecnicamente fundamentados.
- Objetivos e fáceis de compreender.
- Respeitosos e construtivos.
- Proporcionais à gravidade do achado.
- Associados ao trecho de código correto sempre que possível.
- Escritos em português brasileiro.

Os exemplos são ilustrativos. A skill deve adaptar os comentários ao contexto real do projeto, evitando reproduzir textos mecanicamente.

## 2. Estrutura padrão de um comentário

Sempre que aplicável, utilizar a seguinte estrutura:

**Título:**

`[SEVERIDADE] [CLASSIFICAÇÃO] Descrição resumida do achado`

**Corpo:**

1. Contexto: o que está acontecendo?
2. Problema ou oportunidade: por que isso merece atenção?
3. Impacto: qual consequência pode ocorrer?
4. Recomendação: como resolver ou melhorar?

Nem todos os comentários precisam conter explicitamente os quatro elementos. Problemas simples podem ser explicados em poucas linhas.

O mais importante é que o comentário seja compreensível e justificado.

## 3. Exemplos por severidade

### 3.1. Crítico

Utilizar quando houver risco grave e imediato, como comprometimento de segurança, exposição significativa de dados, corrupção de informações ou indisponibilidade relevante.

**Exemplo: exposição de credenciais**

`[CRÍTICO] [CORREÇÃO NECESSÁRIA] Credencial sensível exposta no código`

A chave de acesso está definida diretamente no código-fonte. Isso permite que qualquer pessoa com acesso ao repositório obtenha essa credencial.

Remova o valor do código e utilize um mecanismo seguro de gerenciamento de segredos. Caso essa credencial já tenha sido compartilhada ou publicada, considere-a comprometida e providencie sua rotação.

**Exemplo: acesso indevido a dados**

`[CRÍTICO] [CORREÇÃO NECESSÁRIA] Consulta permite acesso a registros de outros usuários`

A consulta utiliza o identificador recebido na requisição sem validar se o usuário autenticado possui autorização para acessar esse registro.

Isso pode permitir acesso indevido a informações de terceiros. A autorização deve ser verificada antes de retornar os dados.

### 3.2. Alto

Utilizar quando houver defeitos importantes, vulnerabilidades relevantes ou comportamentos capazes de comprometer funcionalidades essenciais.

**Exemplo: exceção não tratada em operação crítica**

`[ALTO] [CORREÇÃO NECESSÁRIA] Falha na integração pode interromper o processamento`

A chamada à integração externa pode lançar uma exceção que não é tratada neste fluxo. Como consequência, a operação pode ser interrompida sem que o restante do processamento seja concluído ou que a falha seja tratada adequadamente.

Avalie o tratamento da exceção e a estratégia de recuperação apropriada para essa operação.

**Exemplo: operação assíncrona sem aguardar conclusão**

`[ALTO] [CORREÇÃO NECESSÁRIA] Operação assíncrona não é aguardada`

A chamada não utiliza `await`, permitindo que o fluxo continue antes da conclusão da operação.

Isso pode provocar inconsistências, especialmente se as próximas instruções dependerem do resultado dessa execução. Aguarde a conclusão da operação ou utilize outra estratégia explicitamente adequada ao cenário.

### 3.3. Médio

Utilizar quando houver problemas que possam gerar comportamentos incorretos em cenários específicos, dificuldades de manutenção ou riscos moderados.

**Exemplo: possível consulta N+1**

`[MÉDIO] [CORREÇÃO NECESSÁRIA] Consulta pode gerar múltiplas idas desnecessárias ao banco`

A consulta relacionada está sendo executada dentro da iteração. Caso a coleção possua muitos registros, isso pode gerar diversas requisições ao banco de dados e degradar o desempenho.

Avalie a possibilidade de carregar os dados necessários em uma consulta adequada ou utilizar uma projeção que evite essas consultas adicionais.

**Exemplo: duplicação de regra de negócio**

`[MÉDIO] [SUGESTÃO DE MELHORIA] Regra de negócio duplicada em diferentes fluxos`

Esta validação já possui uma implementação equivalente em outro fluxo da aplicação. Manter as duas implementações separadas pode provocar divergências quando a regra precisar ser alterada.

Avalie a possibilidade de reutilizar a implementação existente, desde que isso não prejudique a separação de responsabilidades.

### 3.4. Baixo

Utilizar para problemas de menor impacto, mas que ainda representem uma melhoria objetiva de qualidade ou manutenção.

**Exemplo: nomenclatura pouco expressiva**

`[BAIXO] [SUGESTÃO DE MELHORIA] Nome da variável não representa claramente seu conteúdo`

O nome `data` não deixa evidente qual informação está sendo armazenada neste contexto.

Considere utilizar um nome mais específico, como `patientRecords`, para facilitar a leitura e compreensão do fluxo.

**Exemplo: condição redundante**

`[BAIXO] [SUGESTÃO DE MELHORIA] Condição pode ser simplificada`

A condição atual pode ser simplificada sem alterar o comportamento esperado.

Avalie a possibilidade de reduzir essa expressão para facilitar sua leitura e manutenção.

## 4. Exemplos de correções necessárias

### 4.1. Validação ausente

`[MÉDIO] [CORREÇÃO NECESSÁRIA] Parâmetro não possui validação adequada`

O valor recebido pode ser nulo ou inválido, mas o fluxo utiliza a informação sem verificar essas condições.

Isso pode provocar uma exceção durante o processamento. Inclua a validação apropriada antes de utilizar o valor.

### 4.2. Recursos não liberados

`[MÉDIO] [CORREÇÃO NECESSÁRIA] Recurso descartável não está sendo liberado`

O objeto implementa `IDisposable`, mas não existe garantia de que seu recurso será liberado após a utilização.

Considere utilizar `using` ou `await using`, conforme o tipo do recurso, para garantir a liberação adequada mesmo em caso de exceção.

### 4.3. Ausência de cancelamento

`[BAIXO] [SUGESTÃO DE MELHORIA] Operação não propaga o CancellationToken`

O fluxo recebe um `CancellationToken`, mas ele não é encaminhado para a operação assíncrona seguinte.

Isso pode impedir o cancelamento adequado de operações que já não são necessárias, especialmente em requisições HTTP interrompidas pelo cliente.

Avalie propagar o token até as operações que oferecem suporte ao cancelamento.

## 5. Exemplos específicos para .NET WebAPI

### 5.1. Uso incorreto de DI

`[ALTO] [CORREÇÃO NECESSÁRIA] Serviço com dependência Scoped é utilizado por Singleton`

O serviço possui uma dependência com ciclo de vida `Scoped`, mas está sendo registrado como `Singleton`.

Essa configuração pode fazer com que uma dependência seja utilizada fora do seu ciclo de vida esperado, causando comportamentos incorretos e possíveis problemas de concorrência.

Revise os lifetimes envolvidos e ajuste os registros de DI para respeitar o ciclo de vida das dependências.

### 5.2. Consulta EF Core sem projeção

`[BAIXO] [SUGESTÃO DE MELHORIA] Consulta carrega mais dados do que o necessário`

Este fluxo precisa apenas de alguns campos, mas a consulta está materializando a entidade completa.

Considere utilizar `Select` para projetar somente as informações necessárias. Isso pode reduzir o volume de dados transferidos e melhorar a eficiência da consulta.

### 5.3. Ausência de paginação

`[MÉDIO] [CORREÇÃO NECESSÁRIA] Endpoint permite retornar uma coleção sem limite`

A consulta pode retornar todos os registros disponíveis, sem um limite para a quantidade de resultados.

Em bases com grande volume de dados, isso pode aumentar o consumo de memória e o tempo de resposta da API.

Implemente paginação ou um limite adequado à finalidade desse endpoint.

### 5.4. Status HTTP inadequado

`[MÉDIO] [CORREÇÃO NECESSÁRIA] Resposta HTTP não representa corretamente o resultado da operação`

O endpoint retorna `200 OK` mesmo quando o recurso solicitado não existe.

Isso dificulta o tratamento correto da resposta pelos consumidores da API. Avalie retornar `404 Not Found` nesse cenário, mantendo a semântica HTTP adequada.

### 5.5. Regra de negócio no controller

`[BAIXO] [SUGESTÃO DE MELHORIA] Controller concentra lógica que poderia pertencer ao caso de uso`

O controller está executando diretamente uma sequência de regras de negócio, além de lidar com aspectos HTTP.

Considerando a organização atual do projeto, avalie transferir essa responsabilidade para o caso de uso correspondente, mantendo o controller focado na entrada e saída da requisição.

Não utilizar este comentário apenas porque existe lógica no controller. A sugestão deve estar fundamentada em problemas reais de responsabilidade, complexidade ou manutenção.

## 6. Exemplos específicos para aplicações Windows com .NET

### 6.1. Bloqueio da interface

`[ALTO] [CORREÇÃO NECESSÁRIA] Operação bloqueante pode congelar a interface`

A operação está sendo executada diretamente na thread da interface e pode demorar para ser concluída.

Durante esse período, a janela pode deixar de responder às interações do usuário.

Avalie executar a operação de forma assíncrona, mantendo as atualizações dos controles na thread apropriada da interface.

### 6.2. Atualização de controles fora da UI thread

`[ALTO] [CORREÇÃO NECESSÁRIA] Controle da interface é acessado por outra thread`

A atualização deste controle ocorre a partir de uma thread diferente daquela responsável pela interface.

Dependendo da tecnologia e do contexto, isso pode provocar exceções ou comportamentos inconsistentes.

Utilize o mecanismo apropriado para encaminhar a atualização à thread da interface, como `Invoke` ou `BeginInvoke` no WinForms ou `Dispatcher` no WPF.

### 6.3. Assinatura de eventos sem remoção

`[MÉDIO] [CORREÇÃO NECESSÁRIA] Assinatura de evento pode manter objeto vivo indevidamente`

O objeto se inscreve no evento, mas não há uma remoção correspondente durante seu ciclo de vida.

Se o publicador permanecer ativo, poderá manter uma referência ao objeto inscrito, dificultando sua coleta pelo Garbage Collector.

Avalie remover a inscrição quando ela deixar de ser necessária.

### 6.4. Lógica excessiva no code-behind

`[BAIXO] [SUGESTÃO DE MELHORIA] Code-behind concentra responsabilidades distintas`

Este trecho concentra manipulação da interface, acesso a dados e regras de negócio.

Considerando a complexidade atual do formulário, separar essas responsabilidades pode facilitar testes e manutenção.

Avalie uma organização compatível com a arquitetura existente, sem introduzir MVVM ou outra abordagem arquitetural apenas por preferência.

## 7. Exemplos de sugestões de melhoria

Nem todo comentário precisa apontar um defeito. A skill também deve identificar oportunidades reais de melhoria.

### 7.1. Redução de complexidade

`[BAIXO] [SUGESTÃO DE MELHORIA] Fluxo pode ser dividido para facilitar a compreensão`

O método executa várias etapas com responsabilidades distintas, tornando mais difícil acompanhar o processamento.

Considere extrair etapas coesas para métodos menores, com nomes que representem suas responsabilidades.

### 7.2. Reutilização de implementação existente

`[MÉDIO] [SUGESTÃO DE MELHORIA] Implementação semelhante já existe no projeto`

O projeto já possui uma implementação que atende a esse mesmo cenário. Manter duas versões pode aumentar o custo de manutenção e permitir comportamentos divergentes.

Avalie reutilizar a implementação existente, verificando se os contextos realmente possuem as mesmas regras.

### 7.3. Cobertura de testes

`[MÉDIO] [SUGESTÃO DE MELHORIA] Cenário relevante ainda não possui teste automatizado`

O novo fluxo contempla um comportamento importante, mas não há testes cobrindo o cenário de falha.

Considere adicionar um teste que valide esse comportamento, garantindo que futuras alterações não introduzam regressões.

### 7.4. Clareza na expressão

`[BAIXO] [SUGESTÃO DE MELHORIA] Expressão pode ser simplificada para melhorar a leitura`

A condição possui verificações redundantes que dificultam a identificação do resultado esperado.

Considere simplificar a expressão, preservando o comportamento atual.

## 8. Exemplos de comentários inadequados

A skill deve evitar comentários que não agreguem informação técnica ou que não apresentem justificativa suficiente.

### 8.1. Crítica sem explicação

**Inadequado:**

`[MÉDIO] Isso está errado. Corrija.`

**Adequado:**

`[MÉDIO] [CORREÇÃO NECESSÁRIA] Resultado pode ser incorreto quando a coleção estiver vazia`

A implementação acessa o primeiro elemento sem verificar se a coleção possui registros. Quando estiver vazia, ocorrerá uma exceção.

Valide a existência de elementos antes de realizar o acesso.

### 8.2. Exigência baseada em preferência pessoal

**Inadequado:**

`[BAIXO] Prefiro que você utilize expressão lambda neste método.`

**Adequado:**

Não publicar comentário se a implementação atual estiver correta, clara e consistente com o projeto.

Uma preferência pessoal não representa, por si só, um problema de qualidade.

### 8.3. Recomendação arquitetural sem contexto

**Inadequado:**

`[MÉDIO] Todo esse código deveria utilizar Clean Architecture e CQRS.`

**Adequado:**

Não exigir a adoção de uma arquitetura ou padrão sem demonstrar um problema concreto na implementação atual.

Se houver um problema real de separação de responsabilidades, apontar o problema e recomendar uma solução compatível com a arquitetura existente.

### 8.4. Comentário genérico sobre performance

**Inadequado:**

`[MÉDIO] Isso pode ficar lento. Otimize.`

**Adequado:**

`[MÉDIO] [CORREÇÃO NECESSÁRIA] Iteração realiza consulta individual para cada registro`

A consulta é executada repetidamente dentro do laço. Em coleções maiores, isso pode aumentar significativamente o número de requisições ao banco.

Avalie consolidar a consulta ou carregar previamente os dados necessários.

### 8.5. Exigir abstração desnecessária

**Inadequado:**

`[BAIXO] Crie uma interface para essa classe.`

**Adequado:**

Não exigir interfaces, factories, wrappers ou outras abstrações sem uma necessidade concreta, como substituição de implementação, isolamento de dependências ou testabilidade.

### 8.6. Comentário sem relação com o PR

**Inadequado:**

Apontar um problema antigo de outro arquivo que não foi alterado e que não possui relação com o comportamento introduzido pelo PR.

**Adequado:**

Concentrar a revisão nos problemas introduzidos ou agravados pelas alterações.

Problemas preexistentes só devem ser mencionados quando forem indispensáveis para explicar um defeito provocado pelo PR ou quando houver autorização para uma revisão mais ampla.

## 9. Exemplos de posicionamento inline

### 9.1. Comentário na linha exata

Quando o problema estiver claramente associado a uma linha, publicar o comentário nessa linha.

Exemplo:

```csharp
var patient = await context.Patients
    .FirstAsync(x => x.Id == request.PatientId);
```

Comentário:

`[ALTO] [CORREÇÃO NECESSÁRIA] Consulta não verifica se o registro existe`

O método `FirstAsync` lança uma exceção quando nenhum registro é encontrado. Avalie utilizar `FirstOrDefaultAsync` e tratar explicitamente a ausência do registro, conforme o contrato esperado pelo endpoint.

### 9.2. Problema que envolve várias linhas

Quando o problema depender de um conjunto de instruções, associar o comentário ao trecho mais representativo, utilizando um intervalo de linhas quando a plataforma permitir.

Não publicar comentários idênticos em todas as linhas envolvidas.

### 9.3. Problema sem linha adequada no diff

Se o problema estiver relacionado a um contexto que não permite comentário inline, utilizar um comentário geral de revisão quando a plataforma oferecer esse recurso.

Não forçar um posicionamento incorreto apenas para manter todos os comentários inline.

## 10. Exemplos de situações sem comentários

A skill deve reconhecer que nem toda oportunidade teórica justifica uma observação.

Não comentar quando:

- O código está correto e atende ao objetivo.
- A implementação segue uma convenção válida do projeto.
- A sugestão representa apenas preferência pessoal.
- A melhoria é puramente estética e não oferece benefício relevante.
- O problema não pode ser demonstrado com evidências suficientes.
- O achado já foi registrado em outro comentário.
- A alteração não introduz nem agrava o problema.
- O comportamento questionado é intencional e está justificado pelo contexto.

Uma revisão sem comentários pode representar um excelente resultado.

## 11. Exemplos de classificação

A classificação deve refletir o impacto real do achado, e não apenas a aparência do código.

| Severidade | Critério geral |
|---|---|
| Crítico | Comprometimento grave de segurança, dados ou disponibilidade |
| Alto | Defeito importante ou risco significativo para funcionalidades essenciais |
| Médio | Defeito localizado ou risco moderado de manutenção, confiabilidade ou desempenho |
| Baixo | Melhoria de baixo impacto ou problema pontual de legibilidade |

A classificação deve considerar o contexto, a probabilidade de ocorrência, a extensão do impacto e a existência de alternativas.

Não elevar a severidade apenas para chamar a atenção do desenvolvedor.

## 12. Exemplos de comunicação construtiva

Preferir:

- "Avalie a possibilidade de..."
- "Considere utilizar..."
- "Neste cenário, isso pode provocar..."
- "A implementação atual permite..."
- "Uma alternativa seria..."
- "Para preservar o comportamento esperado, recomenda-se..."

Evitar:

- "Você fez errado."
- "Isso não faz sentido."
- "Nunca faça isso."
- "Código horrível."
- "É óbvio que..."
- "Qualquer desenvolvedor deveria saber..."

O foco deve estar no código e em seus efeitos, nunca na capacidade ou competência de quem o escreveu.

## 13. Checklist de qualidade antes de publicar

Antes de publicar cada comentário, verificar:

- [ ] O achado é tecnicamente válido?
- [ ] O problema foi introduzido ou agravado pelo PR?
- [ ] Existe evidência suficiente no código ou no contexto?
- [ ] A severidade é proporcional ao impacto?
- [ ] A classificação está correta?
- [ ] O comentário explica o motivo da observação?
- [ ] A recomendação é prática e adequada ao projeto?
- [ ] O comentário está em português brasileiro?
- [ ] O posicionamento inline corresponde ao trecho correto?
- [ ] Já existe um comentário equivalente?
- [ ] O comentário agrega valor real à revisão?

Se qualquer ponto essencial não puder ser atendido, reavaliar o achado antes de publicá-lo.

## 14. Resultado esperado

Os comentários da skill devem transmitir a análise de um desenvolvedor experiente que conhece boas práticas, entende o contexto do projeto e sabe distinguir problemas reais de preferências pessoais.

A revisão deve ser rigorosa, mas não burocrática.

**O objetivo não é encontrar algo para comentar em cada arquivo. É identificar aquilo que realmente merece a atenção de quem está desenvolvendo.**