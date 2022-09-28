#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Tests/Tests.csproj", "Tests/"]
COPY ["Data/Data.csproj", "Data/"]
COPY ["TourmalineCore.Authentication.Service/TourmalineCore.Authentication.Service.csproj", "TourmalineCore.Authentication.Service/"]
RUN dotnet restore "Tests/Tests.csproj"
COPY . .
WORKDIR "/src/Tests"
RUN dotnet build "Tests.csproj" -c Release -o /app/build

FROM build AS tests
WORKDIR "/src/Tests"
ENTRYPOINT ["dotnet", "test", "--logger:\"junit;LogFilePath=/var/log/tc-auth-tests/test-result.xml;MethodFormat=Class;FailureBodyFormat=Verbose\"", "/p:CollectCoverage=true", "/p:Exclude=[*.Tests]*", "/p:ExcludeByFile=\"./**/Migrations/*.cs\"", "/p:CoverletOutput=\"../\"", "/p:MergeWith=\"../coverage.json\"", "/maxcpucount:1"]
