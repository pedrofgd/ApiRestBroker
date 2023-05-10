# Infra

Pré-requisitos:
- Terraform v1.4.6+ (verificar com `terraform -v`)

## Criar a infraestrutura na AWS

1. Obter as credenciais da AWS pelo Console (access key e secret key) e exportar usando `export AWS_ACCESS_KEY_ID=<SECRET>` e `export AWS_SECRET_ACCESS_KEY=<SECRET>` ou com `aws configure` (necessário instalar a ferramenta CLI)
2. :warning: **Temporário:** gerar chave ssh para poder acessar a máquina com `ssh-keygen -t rsa -b 2048 -f ./keys/aws_key`. Esse nome já está configurado no Terraform para enviar a chave pública na instância (ou tentar usar a que já existe, como a máquina vai ser criada apenas durante o uso, não devemos ter problema com seguraça). Mais a diante tem o comando para acessar a máquina via ssh com a chave privada
3. Executar `terraform init` nesse diretório, para baixar as dependências
4. Executar `terraform validate` para garantir que todas as configurações estão corretas
5. Executar `terraform plan -out demo.tfplan`, para mostar quais recursos serão criados
6. Executar `terraform apply "demo.tfplan"` para efetivamente criar os recursos na AWS

Ao final da execução, serão exibidas variáveis de saída, como o dns público criado para a instância e o id do buckect S3.

<img width="652" alt="Captura de Tela 2023-05-01 às 18 51 05" src="https://user-images.githubusercontent.com/50634340/235538072-ce46691c-487b-4919-bfc5-cb051c5b912e.png">

Após aparecer a mensagem de sucesso na execução do Terraform, leva mais alguns minutos (entre 1 e 2) para fazer o download do código do broker no S3, instalar o sdk do .NET e iniciar a aplicação.

Para validar que tudo funcionou corretamente, utilizar a URL do broker (variável `broker` exibida como output no terminal) para faazer uma requisição via Postman (ou curl) em `GET http://<DNS_PLUBLICO_BROKER>/v1/fake/viacep/01222-020`

:warning: **Sempre após finalizar os testes,** executar `terraform destroy` e aguardar a confirmação (ou executar `terraform destroy -auto-approve`). Isso vai excluir todos os recursos criados, para não gerar custos na AWS

## Comandos e troubleshoot

Para acessar a inatancia: `ssh -i "keys/aws_key" ec2-user@<DNS_PUBLICO_IPV4>`. O DNS é exibido como "output" após a execução do comando `terraform apply`.

Acessar a máquina com ssh pode ser útil para debugar em caso de algum problema.

Para fazer a copia dos arquivos do bucket, utilizar `aws s3 sync s3://<BUCKET_ID>/broker src/` (variável `bucket` exibida como ouput no terminal). O id do bucket também é exibido após aplicar a configuração.

É possível verificar os logs da inicialização da instância com `sudo cat /var/log/cloud-init-output.log`, para debugar algum problema na execução dos comandos de `user_data`

# Referências e documentação

- [Terraform how to do SSH in AWS EC2 instance?](https://jhooq.com/terraform-ssh-into-aws-ec2/)
- [Troubleshoot: provisioning an EC2 instance with terraform InvalidKeyPair.NotFound](https://stackoverflow.com/questions/65466566/provisioning-an-ec2-instance-with-terraform-invalidkeypair-notfound/65478927#65478927)
- [Instalar o SDK do .NET ou o Runtime do .NET no CentOS](https://learn.microsoft.com/pt-br/dotnet/core/install/linux-centos)
- [Avaliar: Host ASP.NET Core no Linux com Nginx](https://learn.microsoft.com/pt-br/aspnet/core/host-and-deploy/linux-nginx?view=aspnetcore-7.0&tabs=linux-ubuntu)
- [Avaliar: Publicando uma aplicação ASP.NET Core no Linux com o Nginx](https://www.treinaweb.com.br/blog/publicando-uma-aplicacao-asp-net-core-no-linux-com-o-nginx)
- [Avaliar: How to Create Key Pair in AWS using Terraform in Right Way](https://cloudkatha.com/how-to-create-key-pair-in-aws-using-terraform-in-right-way/)
- [fileset function should include optional ignore patterns](https://github.com/hashicorp/terraform/issues/25074)
- [Troubleshoot: asp net core 6.0 kestrel server is not working](https://stackoverflow.com/questions/69532898/asp-net-core-6-0-kestrel-server-is-not-working)