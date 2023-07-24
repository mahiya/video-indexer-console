#!/bin/bash -e

# 変数を定義する
region='japaneast'   # デプロイ先のリージョン
resourceGroupName=$1 # デプロイ先のリソースグループ (スクリプトの引数から取得する)
blobContainerName='uploaddata'

# リソースグループを作成する
az group create \
    --location $region \
    --resource-group $resourceGroupName

# Azure リソースをデプロイする
outputs=($(az deployment group create \
            --resource-group $resourceGroupName \
            --template-file biceps/deploy.bicep \
            --parameters blobContainerName=$blobContainerName \
            --query 'properties.outputs.*.value' \
            --output tsv))
tenantId=`echo ${outputs[0]}` # 文末の \r を削除する
subscriptionId=`echo ${outputs[1]}` # 文末の \r を削除する
storageAccountName=`echo ${outputs[2]}` # 文末の \r を削除する
apiFunctionAppName=`echo ${outputs[3]}` # 文末の \r を削除する
backendFunctionAppName=`echo ${outputs[4]}` # 文末の \r を削除する
staticWebAppName=`echo ${outputs[5]}` # 文末の \r を削除する
staticWebAppHostName=`echo ${outputs[6]}` # 文末の \r を削除する
videoIndexerResourceId=`echo ${outputs[7]}` # 文末の \r を削除する
videoIndexerLocation=`echo ${outputs[8]}` # 文末の \r を削除する
videoIndexerAccountId=${outputs[9]}

sleep 10 # Azure Functions App リソースの作成からコードデプロイが早すぎると「リソースが見つからない」エラーが発生する場合があるので、一時停止する

# Web API 用の Azure Functions をデプロイする
pushd functions/api
func azure functionapp publish $apiFunctionAppName --csharp
popd

# アップロードしたビデオを処理するための Azure Functions をデプロイする
pushd functions/backend
func azure functionapp publish $backendFunctionAppName --csharp
popd

# Azure リソース(EventGrid)をデプロイする
az deployment group create \
    --resource-group $resourceGroupName \
    --template-file biceps/post-deploy.bicep \
    --parameters storageAccountName=$storageAccountName \
                 blobContainerName=$blobContainerName \
                 functionAppName=$backendFunctionAppName \
                 functionName='ProcessUploadedData'

# Vue アプリをビルドする
pushd app
npm install
npm run build
popd

# HTML アプリを Static Apps へデプロイする
swa deploy \
    --app-location 'app/dist' \
    --tenant-id $tenantId \
    --resource-group $resourceGroupName \
    --app-name $staticWebAppName \
    --env 'production'
