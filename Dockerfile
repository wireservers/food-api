FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src
COPY BringTheDiet.Api.csproj ./
RUN dotnet restore BringTheDiet.Api.csproj
COPY . .
RUN dotnet publish BringTheDiet.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000
ENTRYPOINT ["dotnet", "BringTheDiet.Api.dll"]
