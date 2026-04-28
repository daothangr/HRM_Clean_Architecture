FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["HRM.API/HRM.API.csproj", "HRM.API/"]
COPY ["HRM.Application/HRM.Application.csproj", "HRM.Application/"]
COPY ["HRM.Infrastructure/HRM.Infrastructure.csproj", "HRM.Infrastructure/"]
COPY ["HRM.Shared/HRM.Shared.csproj", "HRM.Shared/"]
COPY ["HRM.Domain/HRM.Domain.csproj", "HRM.Domain/"]

RUN dotnet restore "HRM.API/HRM.API.csproj"

COPY . .
WORKDIR "/src/HRM.API"
RUN dotnet publish "HRM.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "HRM.API.dll"]