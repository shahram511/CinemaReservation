# Build Runtime------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base

WORKDIR /app

EXPOSE 8080

EXPOSE 8081

# Build Environment---------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Copy the csproj file into the cantainer
COPY ["CinemaReservation.API/CinemaReservation.API.csproj", "CinemaReservation.API/"]
COPY ["CinemaReservation.Core/CinemaReservation.Core.csproj", "CinemaReservation.Core/"]
COPY ["CinemaReservation.Infrastructure/CinemaReservation.Infrastructure.csproj", "CinemaReservation.Infrastructure/"]

RUN dotnet restore "CinemaReservation.API/CinemaReservation.API.csproj"

COPY . .

WORKDIR "/src/CinemaReservation.API"

RUN dotnet build "CinemaReservation.API.csproj" -c Release -o /app/build


# Publish-----------------
FROM build AS publish

RUN dotnet publish "CinemaReservation.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final image assembly---------
FROM base AS final

WORKDIR /app

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "CinemaReservation.API.dll"]