docker build -t leanda/core-persistence:latest -f Sds.Osdr.Persistence/Dockerfile .
docker build -t leanda/core-frontend:latest -f Sds.Osdr.Domain.FrontEnd/Dockerfile .
docker build -t leanda/core-backend:latest -f Sds.Osdr.Domain.BackEnd/Dockerfile .
docker build -t leanda/core-sagahost:latest -f Sds.Osdr.Domain.SagaHost/Dockerfile .
docker build -t leanda/core-web-api:latest -f Sds.Osdr.WebApi/Dockerfile .
docker build -t leanda/integration:latest -f Sds.Osdr.IntegrationTests/Dockerfile .
docker build -t leanda/webapi-integration:latest -f Sds.Osdr.WebApi.IntegrationTests/Dockerfile .
docker image ls leanda/core-*