FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["SaludPortal.Admin/SaludPortal.Admin.csproj", "SaludPortal.Admin/"]

RUN dotnet restore "SaludPortal.Admin/SaludPortal.Admin.csproj"

COPY . .

RUN dotnet publish "SaludPortal.Admin/SaludPortal.Admin.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
# Install fonts required by PDFsharp on Linux
RUN apt-get update && apt-get install -y --no-install-recommends \
        fonts-liberation \
        libfontconfig1 \
    && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "SaludPortal.Admin.dll"]
