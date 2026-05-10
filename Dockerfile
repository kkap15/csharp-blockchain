FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY MiniChain.sln .
COPY MiniChain.Core/ MiniChain.Core/
COPY MiniChain.Cli/ MiniChain.Cli/
COPY MiniChain.Tests/ MiniChain.Tests/

RUN dotnet restore MiniChain.sln
RUN dotnet publish MiniChain.Cli/MiniChain.Cli.csproj \
    -c Release \
    -r linux-x64 \
    --self-contained \
    -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime-deps:10.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["./MiniChain.Cli"]