# Diretrizes de Revisão — Aplicações Windows com .NET

## 1. Objetivo

Este documento estabelece critérios técnicos específicos para revisão de aplicações desktop desenvolvidas com C# e tecnologias do ecossistema .NET para Windows.

Atue como um engenheiro de software sênior especializado em aplicações desktop, com conhecimento de arquitetura, interfaces gráficas, gerenciamento de recursos, concorrência, desempenho e boas práticas de desenvolvimento.

A revisão deve considerar tanto a qualidade da implementação quanto o comportamento da aplicação durante sua execução.

Avalie os impactos das alterações sobre a experiência do usuário, a estabilidade da interface, o consumo de recursos e a manutenibilidade do sistema.

As tecnologias contempladas incluem, mas não se limitam a:

- Windows Forms (WinForms).
- Windows Presentation Foundation (WPF).
- WinUI.
- Aplicações desktop baseadas em .NET.
- Outras tecnologias compatíveis com o ecossistema Windows e .NET.

Adapte os critérios à tecnologia efetivamente utilizada pelo projeto.

Não aplique padrões específicos de WPF a projetos Windows Forms, ou vice-versa.

## 2. Arquitetura e organização da aplicação

Avalie se as responsabilidades estão distribuídas adequadamente.

Considere:

- Separação entre interface gráfica e regras de negócio.
- Organização dos serviços.
- Responsabilidades dos formulários, janelas e controles.
- Gerenciamento de dependências.
- Reutilização de componentes.
- Organização das funcionalidades.
- Acoplamento entre interface e persistência.
- Testabilidade.
- Consistência com a arquitetura existente.

Evite concentrar regras de negócio complexas em componentes responsáveis exclusivamente pela interface.

Entretanto, não exija camadas ou abstrações adicionais quando a complexidade da aplicação não justificar sua existência.

## 3. Windows Forms (WinForms)

Quando o projeto utilizar Windows Forms, considere as características específicas do modelo de eventos e dos controles visuais.

### 3.1. Formulários e responsabilidades

Avalie:

- Complexidade dos formulários.
- Separação entre lógica visual e regras de negócio.
- Manipulação adequada de eventos.
- Reutilização de componentes.
- Gerenciamento de estado.
- Inicialização e encerramento dos formulários.
- Descarte de controles e recursos.
- Acoplamento entre formulários.

Identifique formulários que concentram responsabilidades excessivas ou que dependem diretamente de implementações que poderiam ser isoladas de maneira simples.

Não exija MVVM em projetos WinForms sem justificativa arquitetural.

### 3.2. Eventos

Verifique:

- Registro e remoção adequados de eventos.
- Inscrições duplicadas.
- Eventos que permanecem associados após o encerramento de componentes.
- Tratamento de exceções em manipuladores.
- Execução excessiva de operações dentro de eventos.
- Dependências entre eventos que dificultam a compreensão do fluxo.

Considere o risco de vazamentos de memória causados por assinaturas de eventos que mantêm referências a objetos que deveriam ser liberados.

### 3.3. Controles visuais

Avalie:

- Uso adequado dos controles.
- Atualização correta dos valores exibidos.
- Validação de entradas.
- Habilitação e desabilitação de comandos.
- Consistência visual.
- Responsividade.
- Comportamento diante de dados inválidos.
- Gerenciamento de recursos gráficos.

Não recomende substituir controles nativos por bibliotecas externas sem um benefício concreto.

## 4. Windows Presentation Foundation (WPF)

Quando o projeto utilizar WPF, considere os mecanismos de binding, recursos, templates e organização visual.

### 4.1. XAML

Avalie:

- Organização dos arquivos XAML.
- Clareza da árvore visual.
- Reutilização de estilos e templates.
- Uso adequado de Resources.
- Consistência entre controles.
- Ausência de estruturas visuais excessivamente complexas.
- Uso adequado de DataTemplates e ControlTemplates.
- Organização de layouts.
- Acessibilidade.

Evite recomendar extrações de UserControls apenas para reduzir o tamanho de arquivos XAML quando isso não melhorar a organização ou a reutilização.

### 4.2. Data Binding

Verifique:

- Caminhos de binding corretos.
- Modos de binding adequados.
- Atualização de propriedades.
- Uso apropriado de UpdateSourceTrigger.
- Implementação correta de notificações de alteração.
- Tratamento de valores nulos.
- Uso adequado de converters.
- Ausência de bindings redundantes.
- Consistência entre View e ViewModel.

Investigue problemas em que a interface deixa de refletir alterações no estado da aplicação.

### 4.3. INotifyPropertyChanged

Avalie:

- Implementação correta de notificações.
- Disparo de PropertyChanged quando necessário.
- Nomes corretos das propriedades notificadas.
- Notificação de propriedades dependentes.
- Atualização consistente de coleções e objetos.
- Uso adequado de ObservableCollection.

Não exija notificações para propriedades que não participam de mecanismos de binding ou não precisam refletir alterações dinamicamente.

### 4.4. Commands

Quando utilizados, verifique:

- Implementação correta de ICommand.
- Comportamento de CanExecute.
- Atualização de CanExecute quando necessário.
- Separação entre ações da interface e lógica de negócio.
- Tratamento de exceções.
- Execução assíncrona quando apropriado.

Evite colocar regras de negócio diretamente em comandos visuais.

### 4.5. Code-behind

Não considere o uso de code-behind automaticamente incorreto.

Avalie se o código presente nele está relacionado à interação e ao comportamento visual da interface.

Identifique situações em que regras de negócio, acesso a dados ou responsabilidades complexas estejam excessivamente concentradas no code-behind.

Não exija a extração de código quando isso apenas criar abstrações desnecessárias.

## 5. MVVM

Quando o projeto utilizar MVVM, avalie se as responsabilidades estão distribuídas adequadamente.

Considere:

- View responsável pela apresentação.
- ViewModel responsável pelo estado e pelas ações de apresentação.
- Model e serviços responsáveis pelos dados e regras correspondentes.
- Comunicação adequada entre os componentes.
- Injeção de dependência.
- Commands.
- Data Binding.
- Notificação de alterações.
- Testabilidade dos ViewModels.

### 5.1. ViewModels

Verifique:

- Responsabilidades excessivas.
- Dependências desnecessárias da interface gráfica.
- Acesso direto à infraestrutura sem necessidade.
- Acoplamento excessivo entre ViewModels.
- Manipulação inadequada de estado.
- Métodos complexos.
- Ausência de tratamento de erros.
- Dificuldade de testes.

Um ViewModel pode possuir diversas propriedades e comandos sem que isso represente automaticamente uma violação de responsabilidade única.

Avalie a coesão de suas responsabilidades.

### 5.2. Comunicação entre componentes

Quando houver mecanismos de mensagens, eventos ou Messenger, avalie:

- Clareza da comunicação.
- Acoplamento.
- Ciclo de vida das inscrições.
- Possibilidade de vazamentos de memória.
- Dependências implícitas.
- Tratamento de mensagens.

Não introduza mecanismos de comunicação complexos quando uma dependência direta for suficiente.

## 6. Thread Safety e responsividade da interface

Este é um dos pontos mais importantes na revisão de aplicações desktop.

A interface deve permanecer responsiva durante operações potencialmente demoradas.

### 6.1. Thread principal da interface

Verifique:

- Operações demoradas executadas na thread da interface.
- Acesso a controles a partir de threads inadequadas.
- Uso correto de Dispatcher no WPF.
- Uso correto de Invoke e BeginInvoke no WinForms.
- Atualização segura de componentes visuais.
- Possíveis condições de corrida.
- Bloqueios da interface.
- Deadlocks.

Considere que controles visuais normalmente possuem restrições de acesso relacionadas à thread que os criou.

Não recomende acesso direto aos controles a partir de threads de trabalho.

### 6.2. Operações assíncronas

Avalie:

- Uso adequado de async/await.
- Ausência de bloqueios com .Wait(), .Result e GetAwaiter().GetResult().
- Uso de Task para operações assíncronas.
- Tratamento correto de exceções.
- Propagação de CancellationToken.
- Cancelamento de operações ao encerrar telas ou componentes.
- Controle de operações simultâneas.
- Atualização segura da interface após operações em background.

Não utilize Task.Run indiscriminadamente.

Operações de I/O devem utilizar APIs assíncronas apropriadas sempre que disponíveis.

Task.Run pode ser apropriado para trabalho intensivo de CPU, desde que a execução em background seja realmente necessária.

### 6.3. Concorrência

Investigue:

- Acesso simultâneo a coleções não thread-safe.
- Compartilhamento inadequado de estado.
- Atualização concorrente de propriedades.
- Operações sobre recursos descartados.
- Condições de corrida durante o encerramento da aplicação.
- Múltiplas execuções de comandos que deveriam ser exclusivas.

Considere os efeitos de operações assíncronas que continuam executando após o fechamento de uma janela.

## 7. Ciclo de vida de componentes e recursos

Avalie o ciclo de vida dos objetos e a liberação correta dos recursos.

Considere:

- Implementação e uso de IDisposable.
- Uso adequado de IAsyncDisposable.
- Descarte de componentes visuais.
- Liberação de streams.
- Gerenciamento de conexões.
- Desinscrição de eventos.
- Cancelamento de operações pendentes.
- Liberação de recursos gráficos.
- Gerenciamento de temporizadores.
- Encerramento de tarefas em background.

### 7.1. Vazamentos de memória

Investigue especialmente:

- Eventos que mantêm referências a objetos encerrados.
- Timers não interrompidos.
- Referências estáticas desnecessárias.
- Coleções que continuam armazenando objetos sem necessidade.
- Componentes mantidos vivos por inscrições em eventos.
- Recursos que não são descartados.
- Tarefas que mantêm referências a telas já encerradas.

Não recomende forçar coleta de lixo como solução para problemas de gerenciamento de memória.

O Garbage Collector não substitui o descarte correto de recursos.

### 7.2. Recursos gráficos

Quando aplicável, avalie o descarte adequado de:

- Bitmap.
- Image.
- Font.
- Brush.
- Pen.
- Graphics.
- Streams associados a arquivos e imagens.

Considere que determinados recursos possuem gerenciamento externo ao heap gerenciado pelo .NET.

## 8. Tratamento de exceções e estabilidade

Avalie como a aplicação reage a falhas durante sua execução.

Verifique:

- Exceções não tratadas.
- Captura silenciosa de erros.
- Tratamento inadequado em eventos.
- Operações assíncronas sem observação de falhas.
- Mensagens de erro inadequadas.
- Estados inconsistentes após falhas.
- Recuperação de operações.
- Logging.

Não apresente detalhes técnicos sensíveis diretamente ao usuário final.

Quando uma operação falhar, a aplicação deve manter um comportamento previsível e, quando possível, permitir recuperação.

Não recomende capturar todas as exceções indiscriminadamente.

## 9. Experiência do usuário e acessibilidade

Avalie a experiência proporcionada pelas alterações.

Considere:

- Responsividade.
- Feedback durante operações demoradas.
- Indicadores de carregamento.
- Mensagens de erro claras.
- Validação de formulários.
- Navegação por teclado.
- Foco dos controles.
- Atalhos.
- Acessibilidade.
- Consistência visual.
- Comportamento em diferentes resoluções.
- Escalabilidade de DPI.
- Estados vazios e estados de erro.

Não transforme preferências estéticas em defeitos técnicos.

Priorize problemas que dificultem o uso, provoquem confusão ou comprometam a acessibilidade.

## 10. Persistência e acesso a dados

Quando a aplicação desktop acessar bancos de dados ou outras fontes de persistência, considere:

- Separação entre interface e persistência.
- Gerenciamento de conexões.
- Uso de operações assíncronas.
- Tratamento de falhas.
- Consistência transacional.
- Validação dos dados.
- Consultas eficientes.
- Descarte de recursos.
- Segurança das credenciais.

Não mantenha conexões abertas durante toda a vida da aplicação sem uma justificativa adequada.

Não execute operações demoradas de persistência diretamente na thread da interface.

Quando houver Entity Framework Core, aplique também os critérios definidos em dotnet-webapi.md, considerando as particularidades do contexto desktop.

## 11. Segurança

Considere os riscos específicos de aplicações instaladas e executadas localmente.

Investigue:

- Credenciais armazenadas em código ou arquivos versionados.
- Segredos armazenados de maneira insegura.
- Manipulação insegura de arquivos.
- Execução de comandos externos com entradas não confiáveis.
- Caminhos de arquivos controlados pelo usuário.
- Desserialização insegura.
- Exposição indevida de informações.
- Dependências vulneráveis.
- Permissões excessivas.
- Comunicação insegura com serviços externos.

Não presuma que uma aplicação desktop seja confiável apenas porque executa localmente.

Não recomende armazenar senhas em texto puro ou depender exclusivamente de ofuscação para proteger segredos.

## 12. Integrações com APIs e serviços externos

Quando a aplicação consumir serviços externos, avalie:

- Uso adequado de HttpClient.
- Reutilização de clientes HTTP.
- Configuração de timeouts.
- Cancelamento de requisições.
- Tratamento de erros de comunicação.
- Políticas de retry.
- Atualização segura da interface.
- Indicação de estados de carregamento.
- Comportamento diante de desconexões.
- Tratamento de respostas inválidas.

Evite operações síncronas de rede na thread principal.

Considere que o usuário pode fechar a aplicação enquanto uma requisição ainda está em andamento.

## 13. Configurações e inicialização

Avalie:

- Carregamento de configurações.
- Validação de parâmetros obrigatórios.
- Inicialização de dependências.
- Tratamento de falhas durante a inicialização.
- Gerenciamento de configurações por ambiente.
- Armazenamento seguro de informações.
- Compatibilidade com o ambiente de execução.

Não recomende valores fixos no código quando eles deveriam ser configuráveis.

Verifique se falhas de inicialização são apresentadas e registradas adequadamente.

## 14. Testabilidade

Avalie se os componentes e as regras de negócio podem ser testados adequadamente.

Considere:

- Testes unitários de regras de negócio.
- Testes de ViewModels.
- Testes de serviços.
- Testes de comandos.
- Testes de validação.
- Testes de integração.
- Testes de operações assíncronas.
- Testes de tratamento de erros.

Não exija testes automatizados de interface para toda alteração visual.

Priorize a testabilidade das regras e comportamentos relevantes.

Evite recomendações que introduzam abstrações exclusivamente para permitir testes de componentes triviais.

## 15. Compatibilidade com versões e tecnologias

Antes de recomendar qualquer recurso:

1. Identifique a versão do .NET.
2. Identifique a tecnologia desktop utilizada.
3. Verifique a compatibilidade do recurso.
4. Considere as bibliotecas e versões existentes.
5. Respeite os padrões adotados pelo projeto.

Não recomende APIs de WPF em projetos WinForms.

Não recomende recursos exclusivos de versões recentes do .NET sem verificar a compatibilidade.

Não exija migração de tecnologia apenas por preferência pessoal.

## 16. Critérios para comentários

Cada comentário deve estar relacionado às alterações do PR e apresentar uma justificativa técnica.

Utilize os seguintes formatos:

### Correção necessária

[SEVERIDADE] [CORREÇÃO NECESSÁRIA] Título objetivo

Explique o problema, seu impacto e uma recomendação concreta de correção.

### Sugestão de melhoria

[SEVERIDADE] [SUGESTÃO DE MELHORIA] Título objetivo

Explique a oportunidade, o benefício esperado e uma abordagem recomendada.

Todos os comentários devem estar em português brasileiro.

Evite comentários vagos, repetitivos ou baseados apenas em preferências pessoais.

## 17. Checklist de revisão Windows

Antes de concluir a revisão, considere:

- [ ] A arquitetura respeita os padrões do projeto?
- [ ] A interface está separada das regras de negócio?
- [ ] Os formulários, janelas e controles possuem responsabilidades adequadas?
- [ ] O Data Binding está funcionando corretamente, quando aplicável?
- [ ] O MVVM está implementado adequadamente, quando utilizado?
- [ ] Os eventos possuem ciclo de vida adequado?
- [ ] A interface permanece responsiva?
- [ ] O acesso aos controles respeita as restrições de thread?
- [ ] As operações assíncronas estão implementadas corretamente?
- [ ] Existem riscos de concorrência?
- [ ] As operações são canceladas quando necessário?
- [ ] Os recursos são descartados adequadamente?
- [ ] Existem riscos de vazamento de memória?
- [ ] O tratamento de exceções é adequado?
- [ ] A experiência do usuário foi preservada?
- [ ] A acessibilidade foi considerada?
- [ ] O acesso a dados é eficiente e seguro?
- [ ] As integrações externas possuem tratamento adequado de falhas?
- [ ] As configurações são tratadas corretamente?
- [ ] As recomendações respeitam a tecnologia e a versão utilizadas?

O checklist é uma ferramenta de apoio, não uma exigência de publicação de comentários para cada item.

## 18. Resultado esperado

Uma revisão de aplicações Windows deve demonstrar conhecimento das particularidades do desenvolvimento desktop.

O revisor deve identificar não apenas erros funcionais, mas também problemas que possam comprometer a responsividade, a estabilidade, o gerenciamento de recursos e a experiência do usuário.

Respeite as características da tecnologia utilizada.

Não imponha MVVM a projetos que não precisam dele.

Não considere code-behind automaticamente inadequado.

Não confunda abstração com qualidade.

Priorize problemas comprováveis e sugestões de melhoria justificadas.

O objetivo é contribuir para aplicações desktop estáveis, responsivas, seguras e fáceis de manter.