FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src
COPY ["Soundscripter/Soundscripter.csproj", "Soundscripter/"]
RUN dotnet restore "Soundscripter/Soundscripter.csproj"
COPY . .
WORKDIR "/src/Soundscripter"
RUN dotnet build "Soundscripter.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Soundscripter.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY ./gcloud_credentials.json /app/credentials.json
ENTRYPOINT ["dotnet", "Soundscripter.dll"]