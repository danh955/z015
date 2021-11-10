#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
ARG ConnectionStrings__SqlDatabase
ENV ConnectionStrings__SqlDatabase=${ConnectionStrings__SqlDatabase}

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Z015.Website/Z015.Website.csproj", "Z015.Website/"]
COPY ["Z015.AppFeature/Z015.AppFeature.csproj", "Z015.AppFeature/"]
COPY ["Z015.Repository/Z015.Repository.csproj", "Z015.Repository/"]
RUN dotnet restore "Z015.Website/Z015.Website.csproj"
COPY . .
WORKDIR "/src/Z015.Website"
RUN dotnet build "Z015.Website.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Z015.Website.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Z015.Website.dll"]