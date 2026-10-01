# ---- build ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY global.json ./
COPY E-Invoice.Domain/E-Invoice.Domain.csproj E-Invoice.Domain/
COPY E-Invoice.Application/E-Invoice.Application.csproj E-Invoice.Application/
COPY E-Invoice.Infrastructure/E-Invoice.Infrastructure.csproj E-Invoice.Infrastructure/
COPY E-Invoice.Web/E-Invoice.Web.csproj E-Invoice.Web/
RUN dotnet restore E-Invoice.Web/E-Invoice.Web.csproj
COPY E-Invoice.Domain/ E-Invoice.Domain/
COPY E-Invoice.Application/ E-Invoice.Application/
COPY E-Invoice.Infrastructure/ E-Invoice.Infrastructure/
COPY E-Invoice.Web/ E-Invoice.Web/
RUN dotnet publish E-Invoice.Web/E-Invoice.Web.csproj -c Release -o /app/publish --no-restore

# ---- run ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
# Cloud Run sends traffic to the port in $PORT (8080 by default).
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "E-Invoice.Web.dll"]
