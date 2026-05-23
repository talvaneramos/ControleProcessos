FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ControleProcessos/ControleProcessos.csproj ./ControleProcessos/
RUN dotnet restore ./ControleProcessos/ControleProcessos.csproj

COPY ControleProcessos/ ./ControleProcessos/
RUN dotnet publish ./ControleProcessos/ControleProcessos.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "ControleProcessos.dll"]
