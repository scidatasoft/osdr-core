docker build -t docker.your-company.com/osdr-service-persistence:ci-local -f Sds.Osdr.Persistence/Dockerfile .
docker build -t docker.your-company.com/osdr-service-frontend:ci-local -f Sds.Osdr.Domain.FrontEnd/Dockerfile .
docker build -t docker.your-company.com/osdr-service-backend:ci-local -f Sds.Osdr.Domain.BackEnd/Dockerfile .
docker build -t docker.your-company.com/osdr-service-sagahost:ci-local -f Sds.Osdr.Domain.SagaHost/Dockerfile .
docker build -t docker.your-company.com/osdr-service-web-api:ci-local -f Sds.Osdr.WebApi/Dockerfile .
docker build -t docker.your-company.com/osdr-service-integration:ci-local -f Sds.Osdr.IntegrationTests/Dockerfile .
docker build -t docker.your-company.com/osdr-service-webapi-integration:ci-local -f Sds.Osdr.WebApi.IntegrationTests/Dockerfile .
docker image ls docker.your-company.com/osdr-service-*