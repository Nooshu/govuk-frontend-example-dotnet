# Node fingerprints CSS, the initAll module, fonts, and images.
# The SDK image has no Node, so asset generation stays in this stage.
FROM node:22-bookworm AS assets
WORKDIR /src
COPY package.json package-lock.json ./
COPY styles ./styles
COPY scripts ./scripts
RUN npm ci
RUN node scripts/build-assets-cli.mjs

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY global.json Directory.Build.props Directory.Packages.props .editorconfig ./
COPY src ./src
COPY baseline ./baseline
COPY --from=assets /src/src/GovUk.Frontend.Example/wwwroot ./src/GovUk.Frontend.Example/wwwroot
RUN dotnet publish src/GovUk.Frontend.Example/GovUk.Frontend.Example.csproj \
    -c Release \
    -o /app \
    /p:SkipGovUkAssets=true

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app ./
COPY --from=assets /src/node_modules/govuk-frontend /govuk-frontend
ENV GOVUK_FRONTEND_ROOT=/govuk-frontend
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
ENTRYPOINT ["dotnet", "GovUk.Frontend.Example.dll"]
