FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /app
# copy csproj and restore as distinct layers
COPY *.sln .
COPY BackgroundJob/*.csproj ./BackgroundJob/
RUN dotnet restore
# copy everything else and build app
COPY BackgroundJob/. ./BackgroundJob/
WORKDIR /app/BackgroundJob
RUN dotnet publish -c Release -o published

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS runtime
WORKDIR /app
COPY --from=build /app/BackgroundJob/published ./
ENTRYPOINT ["dotnet", "BackgroundJob.dll"]