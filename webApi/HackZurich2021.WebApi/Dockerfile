#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["HackZurich2021.WebApi/HackZurich2021.WebApi.csproj", "HackZurich2021.WebApi/"]
RUN dotnet restore "HackZurich2021.WebApi/HackZurich2021.WebApi.csproj"
COPY . .
WORKDIR "/src/HackZurich2021.WebApi"
RUN dotnet build "HackZurich2021.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HackZurich2021.WebApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HackZurich2021.WebApi.dll"]