#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Colorful.Web/Colorful.Web.csproj", "Colorful.Web/"]
RUN dotnet restore "Colorful.Web/Colorful.Web.csproj"
COPY . .
WORKDIR "/src/Colorful.Web"
RUN dotnet build "Colorful.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Colorful.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Colorful.Web.dll"]
