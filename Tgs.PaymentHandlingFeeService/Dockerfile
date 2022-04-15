#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BuildConfiguration=Release
RUN --mount=type=secret,id=nuget,dst=/pat cp /pat ~/.nuget/NuGet/NuGet.Config
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["Tgs.PaymentHandlingFeeService/Tgs.PaymentHandlingFeeService.csproj", "Tgs.PaymentHandlingFeeService/"]
COPY ["PaymentHandlingFee.Repositories/PaymentHandlingFee.Repositories.csproj", "PaymentHandlingFee.Repositories/"]
COPY ["PaymentHandlingFee.Core/PaymentHandlingFee.Core.csproj", "PaymentHandlingFee.Core/"]
RUN dotnet restore "Tgs.PaymentHandlingFeeService/Tgs.PaymentHandlingFeeService.csproj"
COPY . .
WORKDIR "/src/Tgs.PaymentHandlingFeeService"
RUN dotnet build "Tgs.PaymentHandlingFeeService.csproj" -c $BuildConfiguration -o /app/build

FROM build AS publish
ARG BuildConfiguration
RUN dotnet publish "Tgs.PaymentHandlingFeeService.csproj" -c $BuildConfiguration -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Tgs.PaymentHandlingFeeService.dll"]