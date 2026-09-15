# BattleShip

Projet .NET composé de plusieurs projets:

- `BattleShip.API` : API ASP.NET Core
- `BattleShip.App` : application Blazor WebAssembly
- `BattleShip.Models` : modèles partagés
- `BattleShip.Tests` : tests unitaires

## Prérequis

- .NET SDK 10.0

## Commandes utiles

### Restaurer les dépendances

```bash
dotnet restore BattleShip.slnx
```

### Compiler la solution

```bash
dotnet build BattleShip.slnx
```

### Exécuter les tests

```bash
dotnet test BattleShip.Tests/BattleShip.Tests.csproj
```

### Lancer l’API

```bash
dotnet run --project BattleShip.API/BattleShip.API.csproj
```

### Lancer l’API en mode surveillance

```bash
dotnet watch run --project BattleShip.API/BattleShip.API.csproj
```

### Lancer l’application Web

```bash
dotnet run --project BattleShip.App/BattleShip.App.csproj
```

### Lancer l’application Web en mode surveillance

```bash
dotnet watch run --project BattleShip.App/BattleShip.App.csproj
```

### Publier l’API

```bash
dotnet publish BattleShip.API/BattleShip.API.csproj
```

### Nettoyer les artefacts de build

```bash
dotnet clean BattleShip.slnx
```

## Notes

- La solution racine est [BattleShip.slnx](BattleShip.slnx).
- L’API expose un endpoint de démonstration sur `/weatherforecast`.
- L’application Blazor démarre via le projet `BattleShip.App`.