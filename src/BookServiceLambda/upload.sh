#!/bin/bash

# Definir o nome da função Lambda a ser atualizada
FUNCTION_NAME="minha-funcao-lambda"

# Definir a região onde a função Lambda está localizada
REGION="us-east-1"

# Definir o diretório de publicação do projeto .NET
PROJECT_DIR="C:\Users\jacks\OneDrive\Documentos\Aws_Deploy\CloudFormation\src\BookServiceLambda"

# Caminho para o diretório de saída do dotnet publish
PUBLISH_DIR="$PROJECT_DIR/bin/Release/net8.0/linux-x64/publish"

# Caminho para o arquivo .zip gerado
ZIP_FILE_PATH="minha-lambda.zip"

# Passo 1: Publicar a aplicação .NET para o Linux
echo "Publicando a aplicação .NET..."
dotnet publish -c Release -r linux-x64 --self-contained false

# Passo 2: Verificar se a publicação foi concluída com sucesso
if [ $? -ne 0 ]; then
    echo "Erro ao executar o comando dotnet publish"
    exit 1
fi

# Passo 3: Compactar os arquivos gerados em um arquivo .zip
echo "Compactando os arquivos em um arquivo .zip..."
Compress-Archive -Path "$PUBLISH_DIR/*" -DestinationPath "$ZIP_FILE_PATH"

# Passo 4: Verificar se a criação do arquivo .zip foi bem-sucedida
if [ ! -f "$ZIP_FILE_PATH" ]; then
    echo "Erro ao criar o arquivo .zip"
    exit 1
fi

# Passo 5: Atualizar a função Lambda com o arquivo .zip
echo "Atualizando a função Lambda '$FUNCTION_NAME'..."

aws lambda update-function-code \
    --function-name "$FUNCTION_NAME" \
    --zip-file fileb://"$ZIP_FILE_PATH" \
    --region "$REGION"

# Passo 6: Verificar se o comando foi bem-sucedido
if [ $? -eq 0 ]; then
    echo "Função Lambda '$FUNCTION_NAME' atualizada com sucesso!"
else
    echo "Erro ao atualizar a função Lambda."
    exit 1
fi
