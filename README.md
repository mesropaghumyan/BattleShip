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
- L’API expose deux endpoints de jeu : `POST /games` (crée une nouvelle partie et renvoie son identifiant) et
  `GET /games/{id}` (consulte l’état d’une partie existante).
- Jouer un coup se fait exclusivement via le service gRPC `Battlefield.FireShot` (contrat
  [`Protos/battlefield.proto`](Protos/battlefield.proto)), jamais en HTTP : il applique le tir du joueur puis,
  si la partie n’est pas terminée, la riposte immédiate de l’ordinateur, dans le même appel.
- L’application Blazor démarre via le projet `BattleShip.App`.