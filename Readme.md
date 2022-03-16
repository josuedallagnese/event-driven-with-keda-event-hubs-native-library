<p style="font-size: 25px" align="center"><b>Event Driven Autoscaling + Keda + Azure Event Hubs + Native Library</b></p>

1) **Pré-requisitos**
* Assinatura no portal Azure para criação de um Event Hubs Namespace e uma Storage Account:

- Crie uma **Storage Account** com o nome e o local que preferir, com dois containers privado chamado "**keda-worker**" e "**reply-keda-worker**".
- Crie um **Event Hubs Namespace** com o nome e o local que preferir, definindo o Pricing tier para **Standard** e o **Throughput Units** para 2.
- Dentro do namespace criado, adicione 2 hubs chamados respectivamente de "**keda-worker**" e "**reply-keda-worker**" com 2 partições.

2) **Installar o keda em seu cluster de kubernetes como achar melhor:**

https://keda.sh/docs/2.6/deploy/

3) **Alterar o arquivo deployment.yaml substituindo os valores destacados:**
    - <EVENT HUBS CONNECTION STRING EM BASE 64>: **para preencher esse valor use a connection string do hub "keda-worker" com o EntityPath. Não use a connection string do namespace.**
    - <BLOB STORAGE ACCOUNT CONNECTION STRING EM BASE 64>

5) **Criar o namespace "worker"**:
    - kubectl create namespace worker

4) **Aplique contra o seu ambiente de kubernetes**.
    - kubectl apply -f deployment.yaml

**OBS**: se preferir modifique os valores de pollingInterval, cooldownPeriod. Eles foram configurados com intervalos menores para visualizar o Autoscaling acontecendo.

5) **Abra a solução Keda.sln e configure em todos os arquivos de configuração os parametros para rodar local:**
    - ConnectionString: **para preencher esse valor use a connection string do hub "keda-worker" com o EntityPath. Não use a connection string do namespace.**
    - ReplyConnectionString: **para preencher esse valor use a connection string do hub "reply-keda-worker" com o EntityPath. Não use a connection string do namespace.**
    - BlobStorageConnectionString

6) **Rode o projeto producer** escolhe a opção 1 e acompanhe pelo kubernetes o deployment fazer o autoscaling automatico.

7) **Para limpar todos os recursos do teste:**
    - kubectl delete namespace worker
