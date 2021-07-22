#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["NuGet.Config", ""]
COPY ["Colorful.Discord/Colorful.Discord.csproj", "Colorful.Discord/"]
COPY ["Colorful.Common/Colorful.Common.csproj", "Colorful.Common/"]
RUN dotnet restore "Colorful.Discord/Colorful.Discord.csproj"
COPY . .
WORKDIR "/src/Colorful.Discord"
RUN dotnet build "Colorful.Discord.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Colorful.Discord.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Colorful.Discord.dll"]