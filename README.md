# FinanceiroAspire

## Introdu��o 

Este documento apresenta a proposta arquitetural para o **FinanceiroAspire**, atendendo aos seguintes requisitos essenciais:

- **Escalabilidade**: Capacidade de lidar com aumento da carga sem comprometer o desempenho.
- **Resili�ncia**: Toler�ncia a falhas com estrat�gias de recupera��o robustas.
- **Seguran�a**: Prote��o dos dados e sistemas contra amea�as.
- **Padr�es Arquiteturais**: Escolha da melhor abordagem entre microsservi�os, mon�litos, SOA ou serverless.
- **Integra��o**: Defini��o clara dos mecanismos de comunica��o entre os componentes.
- **Requisitos N�o-Funcionais**: Foco em desempenho, disponibilidade e confiabilidade.
- **Documenta��o**: Registro de decis�es arquiteturais e fluxos de dados.

---

## Vis�o Geral 

A arquitetura proposta utiliza **[Aspire.net](https://learn.microsoft.com/en-us/dotnet/aspire/get-started/aspire-overview)**, uma abordagem moderna para aplica��es distribu�das. Comparado ao **Akka.net**, **Orleans** e desenvolvimento tradicional de microsservi�os, o **Aspire.net** foi escolhido por sua **simplicidade na configura��o de infraestrutura e facilidade de desenvolvimento**.

A solu��o emprega:
- **Minimal APIs .NET 8**: Alta performance e c�digo simplificado.
- **RabbitMQ com MassTransit**: Comunica��o ass�ncrona confi�vel.
- **MongoDB**: Banco de dados escal�vel e eficiente.

---

## Como executar a aplica��o

### Pr�-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/products/docker-desktop)
- [Docker Compose](https://docs.docker.com/compose/install/)
- [MongoDB Compass](https://www.mongodb.com/try/download/compass)
- [Postman](https://www.postman.com/downloads/)

### Instru��es

1. Clone o reposit�rio:
   ```bash
   git clone https://github.com/diego-gimenes-repo/FinanceiroAspire

   cd FinanceiroAspire
   ```
2. Inicie os servi�os:
   ```bash
   docker-compose up -d
   ```
3. Execute a aplica��o:
   ```bash
   dotnet run --project FinanceiroAspire.AppHost/FinanceiroAspire.AppHost.csproj
   ```
4. Importe a cole��o do Postman:
   - Abra o Postman e importe o arquivo `Test Opah.postman_collection.json`.
   - Execute as requisi��es para testar a aplica��o.


### Evidencia de performance
![Teste de performance](performance.png)

## Arquitetura do Sistema

### Diagrama da Arquitetura
![Diagrama de Arquitetura](arquitetura.png)

### Explica��o do Diagrama

#### Cores dos Componentes
- **Azul**: Servi�os principais (*.NET 8 Minimal APIs*).
- **Verde**: Banco de dados (*MongoDB*).
- **Vermelho**: Mensageria (*RabbitMQ* com *[MassTransit](https://masstransit.io/)*).
- **Roxo**: Componentes de monitoramento.

#### Conex�es
- **Linhas cont�nuas**: Fluxo principal de dados.
- **Linhas pontilhadas**: Integra��es de monitoramento.

---

## Fluxo das Mensagens

![Fluxo do sistema](fluxo.png)

### Benef�cios do Fluxo Ass�ncrono

1. **Alta Disponibilidade**: O servi�o de lan�amentos responde imediatamente ap�s persist�ncia.  
2. **Desacoplamento**: Servi�os operam de forma independente via mensageria.  
3. **Escalabilidade**: Permite escalar servi�os individualmente.  
4. **Resili�ncia**: Falhas em um servi�o n�o afetam diretamente os outros.  

---

## Considera��es de Performance

### Minimal API .NET 8
- Maior performance comparado a controllers tradicionais.  
- Menor consumo de mem�ria.  
- Tempo de resposta otimizado.  
- C�digo mais limpo e de f�cil manuten��o.  

### MongoDB
- **Collections otimizadas**:
   1. **Lan�amentos**: Opera��es r�pidas de inser��o.  
   2. **ConsolidadoDiario**: Atualiza��es at�micas eficientes.  
- �ndices estrat�gicos para consultas r�pidas.  
- Replica��o configur�vel para alta disponibilidade.  

### RabbitMQ com MassTransit
- Garantia de entrega de mensagens.  
- Retentativas autom�ticas em caso de falha.  
- Escalabilidade horizontal do broker.  
- Monitoramento de filas integrado.  



Evidencias do teste de performance no servico consolidado:



---

## Pr�ximos Passos

### 1. Escalabilidade

#### 1.1 Kubernetes (Recomendado para produ��o)  
- Deployments individuais por servi�o.  
- **HPA (Horizontal Pod Autoscaling)** com base em m�tricas.  
- Rollouts controlados sem downtime.  

#### 1.2 Alternativa: Docker Swarm  
- Servi�os globais para componentes cr�ticos.  
- Replica��o configur�vel.  
- Overlay networks para comunica��o segura.  

---

### 2. Monitoramento e Observabilidade

#### 2.1 Dashboards personalizados para:
- Tempo de processamento de mensagens.  
- Taxa de transfer�ncia do MongoDB.  
- Sa�de geral do sistema.  

#### 2.2 Alertas configur�veis para:
- Filas com alto volume.  
- Lat�ncia elevada.  
- Falhas no processamento (**DLQ**).  

---

### 3. Seguran�a

- **Autentica��o JWT** para APIs.  
- **Criptografia** de dados sens�veis.  
- **Network Policies** em Kubernetes.  
- **Secrets Management** centralizado.  

---

## Trade-offs e Considera��es Finais

### 1. Performance vs Complexidade
- **Ganho significativo** de performance com Minimal APIs.  
- Complexidade extra devido ao sistema distribu�do.  

### 2. Custos Operacionais
- Investimento inicial em infraestrutura.  
- Retorno com menor consumo de recursos e manuten��o simplificada.  

### 3. Curva de Aprendizado
- Documenta��o extensa dispon�vel para todas as ferramentas utilizadas.  
- Padr�es estabelecidos facilitam a entrada de novos desenvolvedores.  

---

## Conclus�o

Esta arquitetura proporciona **alta performance, escalabilidade e manutenibilidade**, sendo ideal para **sistemas financeiros de grande porte**, garantindo processamento ass�ncrono **r�pido e confi�vel**.
