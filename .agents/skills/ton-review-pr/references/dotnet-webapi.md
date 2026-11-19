# Diretrizes de Revisão — .NET WebAPI

## 1. Objetivo

Este documento estabelece critérios técnicos específicos para revisão de aplicações WebAPI desenvolvidas em C# e ASP.NET Core.

Atue como um engenheiro de software sênior especializado no ecossistema .NET, com conhecimento de arquitetura de aplicações, APIs REST, persistência de dados, segurança, desempenho e boas práticas de desenvolvimento.

Avalie não apenas a implementação individual, mas também o fluxo completo da funcionalidade, considerando suas dependências e consequências para o restante da aplicação.

Os critérios deste documento devem ser aplicados de maneira contextualizada, respeitando a arquitetura e as convenções existentes no repositório.

Não imponha tecnologias, bibliotecas ou padrões arquiteturais que não sejam necessários ao projeto.

## 2. ASP.NET Core e estrutura da aplicação

Avalie se os componentes da aplicação estão organizados adequadamente e se respeitam as responsabilidades de cada camada.

Considere:

- Separação entre entrada HTTP, regras de negócio e persistência.
- Responsabilidades dos Controllers e Minimal APIs.
- Organização de endpoints.
- Uso adequado de Middleware.
- Configuração do pipeline HTTP.
- Registro e configuração de serviços.
- Gerenciamento de configurações por ambiente.
- Tratamento centralizado de exceções.
- Uso correto de filtros e mecanismos de extensibilidade.
- Consistência na estrutura das respostas HTTP.

### 2.1. Controllers e endpoints

Verifique se os endpoints:

- Possuem responsabilidades claras.
- Não concentram regras de negócio complexas.
- Delegam adequadamente operações para os componentes responsáveis.
- Validam os dados de entrada.
- Retornam códigos HTTP coerentes.
- Evitam exposição desnecessária de detalhes internos.
- Respeitam os contratos definidos pela API.

Evite recomendar a extração de responsabilidades quando o endpoint já possui uma implementação simples e adequada.

### 2.2. Middleware e pipeline HTTP

Avalie:

- A ordem de execução dos Middlewares.
- O posicionamento dos mecanismos de autenticação e autorização.
- O tratamento de exceções.
- A configuração de CORS.
- O uso adequado de Rate Limiting.
- A propagação de CancellationToken.
- A configuração de HTTPS e redirecionamentos.
- O comportamento em diferentes ambientes.

Considere que a ordem dos Middlewares pode alterar diretamente o comportamento e a segurança da aplicação.

Não recomende alterações de ordem sem compreender as dependências entre os componentes.

## 3. Injeção de dependência (Dependency Injection)

Avalie se o registro e o consumo de dependências respeitam os mecanismos nativos do .NET.

Verifique:

- Adequação dos tempos de vida Singleton, Scoped e Transient.
- Dependências incompatíveis entre diferentes escopos.
- Possíveis problemas de Captive Dependency.
- Registro duplicado ou desnecessário de serviços.
- Dependências circulares.
- Uso excessivo de Service Locator.
- Acoplamento desnecessário à implementação concreta.
- Descarte correto de recursos gerenciados pelo container.

### 3.1. Tempos de vida

Considere especialmente:

- Serviços Singleton não devem depender diretamente de serviços Scoped.
- Serviços que manipulam estado por requisição devem possuir escopo adequado.
- DbContext normalmente deve ser utilizado com ciclo de vida Scoped em aplicações WebAPI.
- Serviços Transient não devem manter estado compartilhado de maneira insegura.

Avalie o comportamento real da dependência antes de registrar um problema.

Não classifique automaticamente um serviço como incorreto apenas por utilizar um tempo de vida diferente do mais comum.

## 4. Programação assíncrona

Avalie cuidadosamente o uso de async/await.

Verifique:

- Operações de I/O executadas de maneira assíncrona.
- Uso adequado de métodos assíncronos disponíveis nas bibliotecas utilizadas.
- Ausência de bloqueios desnecessários com `.Result`, `.Wait()` ou `GetAwaiter().GetResult()`.
- Propagação de CancellationToken.
- Tratamento correto de exceções em operações assíncronas.
- Uso adequado de Task e ValueTask.
- Ausência de operações assíncronas desnecessariamente sequenciais.
- Ausência de concorrência insegura sobre recursos compartilhados.

### 4.1. Concorrência

Investigue especialmente:

- Acesso concorrente ao mesmo DbContext.
- Execução paralela de operações sobre objetos não thread-safe.
- Compartilhamento inadequado de estado entre requisições.
- Problemas de sincronização.
- Operações assíncronas iniciadas sem acompanhamento adequado.

Não utilize Task.Run indiscriminadamente para transformar operações de I/O em operações assíncronas.

Não recomende paralelização quando ela puder introduzir concorrência insegura ou não oferecer benefício mensurável.

## 5. Entity Framework Core e persistência

Avalie o uso do Entity Framework Core considerando correção, desempenho, consistência e manutenibilidade.

### 5.1. Consultas

Verifique:

- Uso adequado de IQueryable e materialização.
- Consultas desnecessariamente complexas.
- Carregamento excessivo de entidades.
- Problemas de N+1.
- Uso desnecessário de Include e ThenInclude.
- Projeções desnecessárias ou inadequadas.
- Consultas executadas repetidamente.
- Paginação.
- Ordenação.
- Filtros aplicados no banco de dados.
- Uso adequado de AsNoTracking.
- Consultas que podem gerar grandes volumes de dados.

Priorize operações executadas no banco de dados quando isso evitar materializações desnecessárias.

Não recomende AsNoTracking indiscriminadamente em operações que dependam do rastreamento de alterações.

### 5.2. Alterações e persistência

Verifique:

- Uso adequado de SaveChanges e SaveChangesAsync.
- Gerenciamento de transações.
- Consistência entre operações relacionadas.
- Tratamento de concorrência otimista.
- Atualização de entidades.
- Integridade referencial.
- Operações em lote.
- Risco de perda de atualizações.
- Uso correto de migrations.

Avalie se operações que precisam ser atômicas estão protegidas adequadamente.

Não recomende transações explícitas quando o mecanismo utilizado já garantir a atomicidade necessária.

### 5.3. DbContext

Considere:

- Tempo de vida adequado.
- Ausência de compartilhamento concorrente.
- Descarte correto.
- Configuração de relacionamentos.
- Configuração de índices.
- Configuração de chaves e restrições.
- Comportamento de tracking.
- Uso de interceptors e configurações globais.

Nunca assuma que DbContext é thread-safe.

### 5.4. Consultas eficientes

Investigue padrões como:

- Buscar todos os registros para filtrar em memória.
- Executar consultas dentro de loops.
- Materializar resultados antes de aplicar filtros.
- Buscar entidades completas quando apenas algumas propriedades são necessárias.
- Carregar relacionamentos desnecessários.
- Ignorar paginação em consultas potencialmente volumosas.

Quando identificar um problema, explique o impacto esperado e sugira uma alternativa concreta.

## 6. MediatR e organização de casos de uso

Quando o projeto utilizar MediatR, avalie se Commands, Queries, Handlers e Notifications estão organizados de maneira coerente.

### 6.1. Commands e Queries

Verifique:

- Separação adequada entre operações de leitura e escrita.
- Clareza da intenção de cada request.
- Responsabilidades dos Handlers.
- Contratos de entrada e saída.
- Validações.
- Tratamento de erros.
- Uso adequado de CancellationToken.

Não exija CQRS quando o projeto não utiliza essa abordagem.

Não recomende criar Commands ou Queries apenas para encapsular operações triviais sem benefício arquitetural.

### 6.2. Handlers

Avalie se cada Handler:

- Possui uma responsabilidade clara.
- Implementa corretamente o caso de uso.
- Não concentra responsabilidades excessivas.
- Utiliza as dependências necessárias.
- Trata adequadamente cenários excepcionais.
- Respeita as regras de negócio.
- Evita duplicação de lógica.
- Mantém uma implementação compreensível.

Um Handler não precisa ser artificialmente pequeno, mas deve possuir responsabilidades coerentes.

### 6.3. Pipeline Behaviors

Quando utilizados, avalie:

- Ordem de execução dos Behaviors.
- Validações centralizadas.
- Logging.
- Tratamento de exceções.
- Transações.
- Medição de desempenho.
- Propagação de CancellationToken.

Verifique se responsabilidades transversais estão sendo aplicadas consistentemente.

Não recomende mover toda lógica para Pipeline Behaviors. Regras de negócio específicas devem permanecer nos componentes responsáveis pelo caso de uso.

### 6.4. Notifications

Verifique:

- Uso adequado de notificações para eventos internos.
- Consequências da execução de múltiplos handlers.
- Tratamento de falhas parciais.
- Dependências entre handlers.
- Necessidade de consistência transacional.

Não confunda Notifications do MediatR com eventos distribuídos ou garantias de entrega persistente.

Se a operação exigir confiabilidade entre banco de dados e mensageria, avalie a necessidade de padrões como Outbox, conforme a arquitetura do projeto.

## 7. Arquitetura Vertical Slice

Quando o projeto utilizar Vertical Slice Architecture, respeite a organização por funcionalidades e casos de uso.

Avalie:

- Coesão das funcionalidades.
- Organização de Commands, Queries e respectivos Handlers.
- Proximidade entre código relacionado.
- Separação adequada de responsabilidades.
- Reutilização de componentes compartilhados.
- Independência entre funcionalidades quando apropriado.
- Ausência de dependências desnecessárias entre slices.

Não imponha uma estrutura de camadas tradicional.

Não exija que cada funcionalidade possua uma estrutura idêntica quando sua complexidade não justificar isso.

### 7.1. Compartilhamento de código

Considere que reutilização nem sempre significa criar uma abstração compartilhada.

Avalie se o compartilhamento:

- Reduz duplicação relevante.
- Mantém responsabilidades claras.
- Não cria acoplamento excessivo.
- Não transforma componentes específicos em abstrações genéricas desnecessárias.

Evite tanto duplicação excessiva quanto centralização artificial.

## 8. APIs REST e contratos HTTP

Avalie se a API respeita os princípios e convenções HTTP.

### 8.1. Métodos HTTP

Verifique o uso coerente de:

- GET para leitura.
- POST para criação ou execução de operações não representadas adequadamente por outros métodos.
- PUT para substituição de recursos.
- PATCH para atualização parcial.
- DELETE para remoção.

Considere as características de segurança e idempotência dos métodos.

Não recomende alterações apenas por interpretação rígida de REST quando o contrato existente possuir uma justificativa válida.

### 8.2. Códigos de status

Avalie o uso adequado dos códigos HTTP, considerando:

- 200 OK.
- 201 Created.
- 202 Accepted.
- 204 No Content.
- 400 Bad Request.
- 401 Unauthorized.
- 403 Forbidden.
- 404 Not Found.
- 409 Conflict.
- 422 Unprocessable Content, quando adotado pelo projeto.
- 500 Internal Server Error.

Verifique se os códigos representam corretamente o resultado da operação.

### 8.3. Contratos de requisição e resposta

Considere:

- Consistência dos DTOs.
- Separação entre entidades de persistência e contratos HTTP.
- Validação dos dados de entrada.
- Exposição de informações internas.
- Compatibilidade com consumidores existentes.
- Consistência dos formatos de resposta.
- Versionamento da API, quando aplicável.

Não exponha entidades do banco de dados diretamente sem considerar os riscos de acoplamento e exposição de informações.

Avalie o contexto antes de exigir DTOs adicionais para operações simples.

### 8.4. Paginação e filtros

Verifique:

- Paginação em consultas volumosas.
- Limites máximos de registros.
- Validação dos parâmetros.
- Ordenação determinística.
- Filtros eficientes.
- Comportamento diante de páginas inexistentes.
- Consistência dos metadados de paginação.

Evite consultas que possam retornar volumes ilimitados de registros sem justificativa.

## 9. Validação de dados e regras de negócio

Avalie se as entradas são validadas adequadamente e se as regras de negócio estão protegidas.

Considere:

- Dados obrigatórios.
- Limites de tamanho.
- Formatos.
- Intervalos numéricos.
- Consistência entre campos.
- Regras de domínio.
- Estados inválidos.
- Valores nulos.
- Dados provenientes de sistemas externos.

Diferencie validações estruturais de regras de negócio.

Validações de formato e obrigatoriedade podem ser tratadas na entrada da aplicação, enquanto invariantes de negócio devem ser protegidas no local adequado.

Quando o projeto utilizar FluentValidation ou outro mecanismo, respeite sua organização.

Não duplique validações sem necessidade.

Nunca presuma que a validação realizada pelo frontend seja suficiente para proteger a API.

## 10. Tratamento de exceções e logging

Avalie se as falhas são tratadas de maneira previsível e observável.

Verifique:

- Uso de tratamento centralizado de exceções.
- Ausência de captura silenciosa de exceções.
- Preservação do contexto original da falha.
- Logging estruturado.
- Ausência de informações sensíveis nos logs.
- Uso adequado dos níveis de log.
- Consistência das respostas de erro.
- Correlação entre requisições e registros de log.

### 10.1. Tratamento de exceções

Investigue:

- Blocos catch vazios.
- Exceções capturadas e ignoradas.
- Captura excessivamente genérica.
- Uso inadequado de exceções para controle de fluxo.
- Reempacotamento que perde informações relevantes.
- Retorno de mensagens internas ao consumidor da API.

Não recomende capturar e registrar uma exceção em todos os níveis da aplicação.

O tratamento deve ocorrer no componente que possui contexto e responsabilidade para lidar com a falha.

### 10.2. Respostas de erro

Quando apropriado, considere o uso de ProblemDetails.

Avalie se as respostas:

- Possuem estrutura consistente.
- Não expõem Stack Trace ou detalhes internos em produção.
- Informam adequadamente o problema.
- Mantêm códigos HTTP coerentes.
- Permitem identificar erros de validação.

Respeite os contratos de erro já utilizados pela aplicação.

## 11. Segurança

A segurança deve ser tratada como prioridade durante a revisão.

### 11.1. Autenticação e autorização

Verifique:

- Configuração adequada de autenticação.
- Uso correto de políticas de autorização.
- Proteção dos endpoints.
- Validação de claims.
- Verificação de permissões.
- Ausência de confiança em identificadores fornecidos pelo cliente.
- Riscos de acesso indevido a recursos de outros usuários.

Não confunda autenticação com autorização.

Um usuário autenticado não necessariamente possui permissão para acessar determinado recurso.

### 11.2. Exposição de informações

Investigue:

- Senhas ou tokens em código-fonte.
- Segredos em arquivos de configuração versionados.
- Dados pessoais expostos desnecessariamente.
- Informações sensíveis em logs.
- Stack Traces em respostas HTTP.
- Detalhes internos de infraestrutura.

Não reproduza segredos encontrados em comentários ou relatórios.

### 11.3. Proteção contra vulnerabilidades

Considere:

- SQL Injection.
- Insecure Direct Object Reference (IDOR).
- Mass Assignment.
- Server-Side Request Forgery (SSRF).
- Cross-Site Scripting (XSS), quando aplicável.
- Cross-Site Request Forgery (CSRF), quando aplicável.
- Validação inadequada de arquivos enviados.
- Configurações CORS excessivamente permissivas.
- Ausência de Rate Limiting em operações sensíveis.
- Deserialização insegura.
- Falhas de autorização em operações de leitura e escrita.

Não classifique uma vulnerabilidade como confirmada sem identificar um cenário de exploração plausível.

## 12. Integrações externas e mensageria

Quando a API consumir serviços externos ou utilizar mensageria, avalie:

- Configuração de HttpClient.
- Uso adequado de IHttpClientFactory.
- Timeouts.
- Cancelamento de requisições.
- Tratamento de falhas de comunicação.
- Políticas de retry.
- Circuit Breaker, quando aplicável.
- Idempotência.
- Tratamento de mensagens duplicadas.
- Garantias de entrega.
- Consistência entre persistência e publicação de eventos.

Evite políticas de retry indiscriminadas para operações que não sejam idempotentes.

Não recomende retry infinito.

Considere o padrão Outbox quando houver necessidade de garantir consistência entre persistência de dados e publicação de mensagens.

Avalie se falhas externas podem deixar o sistema em estado inconsistente.

## 13. Desempenho e escalabilidade

Investigue problemas que possam afetar o comportamento da aplicação sob carga.

Considere:

- Consultas ineficientes.
- Alocações excessivas.
- Operações bloqueantes.
- Serialização de grandes volumes de dados.
- Falta de paginação.
- Chamadas externas redundantes.
- Cache inadequado.
- Concorrência.
- Configurações de timeout.
- Consumo desnecessário de memória.

Não recomende cache apenas porque uma informação é frequentemente consultada.

Avalie consistência, invalidação, concorrência e impacto real da estratégia.

Não apresente hipóteses de desempenho como defeitos confirmados sem evidências suficientes.

## 14. Configurações e ambientes

Verifique:

- Separação de configurações por ambiente.
- Uso adequado de Options Pattern.
- Validação de configurações obrigatórias.
- Ausência de valores sensíveis versionados.
- Consistência entre ambientes.
- Configurações de logging.
- Timeouts e limites.
- Configurações de conexão.

Considere o comportamento da aplicação quando uma configuração obrigatória estiver ausente ou inválida.

Não recomende hardcoding de valores que deveriam ser configuráveis.

## 15. Compatibilidade e evolução de contratos

Avalie o impacto das alterações sobre consumidores existentes.

Considere:

- Mudanças em endpoints.
- Alterações de nomes de propriedades.
- Alterações de tipos.
- Mudanças de obrigatoriedade.
- Alterações de códigos HTTP.
- Modificações de comportamento.
- Versionamento.
- Compatibilidade com integrações existentes.

Uma alteração aparentemente pequena pode quebrar consumidores externos.

Não presuma que todos os consumidores serão atualizados simultaneamente.

## 16. Testes automatizados

Avalie os testes considerando os comportamentos introduzidos pelo PR.

Quando aplicável, considere:

- Testes unitários de regras de negócio.
- Testes de Handlers.
- Testes de validação.
- Testes de integração com banco de dados.
- Testes de endpoints.
- Testes de autorização.
- Testes de contratos.
- Cenários de falha em integrações externas.
- Testes de concorrência, quando relevantes.

Verifique se os testes realmente validam o comportamento esperado.

Não exija testes unitários isolados para todos os métodos privados ou detalhes de implementação.

Priorize testes que protejam regras de negócio e comportamentos relevantes.

## 17. Compatibilidade com versões e bibliotecas

Antes de recomendar uma API, recurso ou padrão:

1. Identifique a versão do .NET utilizada.
2. Verifique a compatibilidade do recurso.
3. Considere as versões dos pacotes instalados.
4. Respeite as convenções existentes.
5. Evite recomendações baseadas em APIs indisponíveis no projeto.

Não recomende atualizações de dependências como solução automática para problemas que podem ser resolvidos na implementação atual.

## 18. Critérios para comentários

Cada comentário deve estar relacionado a uma alteração do PR e apresentar uma justificativa técnica.

Utilize os seguintes formatos:

### Correção necessária

[SEVERIDADE] [CORREÇÃO NECESSÁRIA] Título objetivo

Explique o comportamento incorreto, o impacto e a recomendação de correção.

### Sugestão de melhoria

[SEVERIDADE] [SUGESTÃO DE MELHORIA] Título objetivo

Explique a oportunidade de melhoria, o benefício esperado e uma abordagem recomendada.

Todos os comentários devem estar em português brasileiro.

Evite comentários genéricos, redundantes ou baseados apenas em preferências pessoais.

## 19. Checklist de revisão WebAPI

Antes de concluir a análise, considere:

- [ ] A estrutura respeita a arquitetura existente?
- [ ] Os endpoints possuem responsabilidades adequadas?
- [ ] A injeção de dependência está correta?
- [ ] O código assíncrono está implementado adequadamente?
- [ ] O uso do EF Core é eficiente e seguro?
- [ ] As operações de persistência mantêm a consistência necessária?
- [ ] Commands e Queries possuem responsabilidades claras, quando aplicável?
- [ ] Os Handlers estão adequadamente organizados?
- [ ] Os contratos HTTP são consistentes?
- [ ] Os códigos de status são apropriados?
- [ ] As entradas são validadas?
- [ ] As regras de negócio estão protegidas?
- [ ] O tratamento de exceções é adequado?
- [ ] Os logs são úteis e não expõem informações sensíveis?
- [ ] A autenticação e a autorização estão corretas?
- [ ] As integrações externas possuem tratamento adequado de falhas?
- [ ] Existem riscos de desempenho?
- [ ] As alterações preservam a compatibilidade necessária?
- [ ] Os testes contemplam os comportamentos relevantes?
- [ ] As recomendações respeitam a versão do .NET utilizada?

O checklist é uma ferramenta de apoio, não uma exigência de publicação de comentários para cada item.

## 20. Resultado esperado

Uma revisão de WebAPI deve demonstrar conhecimento prático de ASP.NET Core, C#, Entity Framework Core e arquitetura de aplicações.

O revisor deve conseguir identificar tanto erros funcionais quanto decisões técnicas que possam comprometer a evolução, a segurança ou a confiabilidade do sistema.

Priorize problemas comprováveis e sugestões de melhoria justificadas.

Não invente defeitos.

Não imponha arquiteturas.

Não transforme preferências pessoais em exigências técnicas.

A revisão deve ajudar a equipe a construir APIs confiáveis, seguras, eficientes e fáceis de manter.