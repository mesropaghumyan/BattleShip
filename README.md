# BattleShip

Bataille navale jouable du navigateur jusqu'au serveur : moteur de jeu indépendant du transport, API ASP.NET Core (Minimal API + gRPC), interface Blazor WebAssembly. Projet réalisé dans le cadre du TP ASP.NET Core / Blazor (HTS Learning) — voir `docs/cours_c_asp_net_bataille_navale.md`.

## Fonctionnalités livrées

- Moteur de jeu complet : grilles 10x10, flotte de 5 navires placés aléatoirement (sans chevauchement), résolution des tirs (raté/touché/coulé), détection de fin de partie.
- Adversaire ordinateur à deux niveaux, sélectionnable avant la partie : facile (tir aléatoire) et difficile (stratégie chasse/cible avec parité damier).
- Création et consultation de partie en HTTP (Minimal API) ; l'action de tir est exposée exclusivement en gRPC (gRPC-Web), résolvant en un seul appel le tir du joueur et la riposte de l'ordinateur.
- Interface Blazor WebAssembly : création de partie, deux grilles (la sienne, complète, et celle de l'adversaire, qui ne révèle jamais l'emplacement d'un navire non encore touché), jeu, gestion des incidents de communication.
- Historique chronologique des coups joués (camp, coordonnée, résultat), affiché pendant la partie.
- Récapitulatif de fin de partie : nombre de tirs, taux de réussite et durée, par camp.
- Validation FluentValidation sur toutes les entrées serveur (HTTP et gRPC), avec mapping d'erreurs gRPC explicite par cas.
- Tests métier et d'intégration (101 tests), capables de détecter une règle violée, pas seulement le cas nominal.

Décisions d'architecture détaillées : `_bmad-output/planning-artifacts/architecture/architecture-BattleShip-2026-09-15/ARCHITECTURE-SPINE.md` et `docs/adr/`.

## Prérequis

- .NET SDK 10.0

## Démarrage rapide

Toutes les commandes se lancent depuis la racine du dépôt.

### 1. Restaurer les dépendances

```bash
dotnet restore BattleShip.slnx
```

### 2. Compiler

```bash
dotnet build BattleShip.slnx
```

### 3. Lancer les tests

```bash
dotnet test BattleShip.slnx
```

Résultat attendu : 101 tests réussis, 0 échec.

### 4. Lancer l'API

```bash
dotnet run --project BattleShip.API/BattleShip.API.csproj
```

Écoute sur `http://localhost:5268`. Sans argument, `dotnet run` utilise déjà le profil `http` déclaré en premier dans `launchSettings.json` — c'est le comportement voulu, il n'y a rien à changer.

### 5. Lancer l'application Web (dans un second terminal)

```bash
dotnet run --project BattleShip.App/BattleShip.App.csproj
```

Ouvre automatiquement le navigateur sur `http://localhost:5277`.

> ⚠️ **Ne pas lancer l'App avec `--launch-profile https`.** L'App appelle l'API sur une adresse codée en dur en HTTP (`http://localhost:5268`) ; si l'App elle-même est servie en HTTPS (`https://localhost:7297`), le navigateur bloque par défaut cet appel HTTP par sécurité (« contenu mixte »), même si l'API répond et si CORS l'autorise. Rester sur le profil `http` par défaut des deux côtés évite ce blocage.

### Jouer une partie complète

1. Sur la page d'accueil de l'App, choisir la difficulté de l'adversaire (facile ou difficile) et créer une partie.
2. Cliquer sur une case de la grille adverse pour tirer : le résultat (raté/touché/coulé) s'affiche, suivi immédiatement de la riposte de l'ordinateur sur votre propre grille.
3. L'historique des coups se remplit au fil de la partie, camp par camp.
4. La partie se termine dès qu'une flotte est entièrement coulée ; le récapitulatif (tirs, taux de réussite, durée par camp) s'affiche alors et toute case redevient non cliquable.

### Nettoyer les artefacts de build

```bash
dotnet clean BattleShip.slnx
```

### En cas de problème

- Pour arrêter l'API ou l'App : `Ctrl+C` dans le terminal correspondant.
- Si un port (5268 ou 5277) est déjà occupé par une exécution précédente non arrêtée, arrêter ce processus avant de relancer.

## Arbitrages du backlog

Le socle imposé (moteur de jeu, API HTTP + gRPC, interface Blazor, validation, tests) est livré intégralement. Au-delà du socle, l'équipe a choisi :

- **Retenu :** adversaire en mode difficile (chasse/cible), difficulté réglable avant la partie, historique des coups joués, statistiques de fin de partie — les quatre pistes de backlog jugées les plus démonstratives de la maîtrise du moteur de jeu et du contrat API dans le temps imparti.
- **Écarté :** multijoueur réseau (non-goal explicite du socle : identité de camp fixée à `enum Side { Human, Computer }`, pas d'identité joueur), sauvegarde/persistance (état en mémoire uniquement, choix assumé pour rester dans le périmètre du TP), déploiement hébergé (démonstration en local uniquement), personnalisation et accessibilité clavier (pistes de backlog non retenues par manque de temps, pas par choix de conception).

## Limites connues

- **État en mémoire uniquement :** une partie ne survit pas à un redémarrage du serveur (choix assumé, pas de base de données ni de fichier de persistance).
- **Pas de test de régression sur les options JSON du client Blazor** (`GameHttpClient`) : un bug de désérialisation (camelCase) s'est déjà produit une fois en cours de développement et a été corrigé, mais rien dans `dotnet test` ne le rattraperait s'il revenait.
- **`IGameStore.WithGame<TResult>` exige un type référence**, ce qui force certains appelants (tests) à contourner la contrainte plutôt que de retourner directement une valeur.
- **Le service gRPC `FireShot` n'a pas de filet d'exception générique** en miroir du pipeline HTTP : une exception non anticipée renvoie un statut gRPC nu (`Internal`/`Unknown`) plutôt qu'un message structuré.
- **Aucun test de composant Blazor** (pas d'outillage bUnit dans le dépôt) : le rendu de l'interface (grille, historique, statistiques) est vérifié manuellement, pas par `dotnet test`.
- **Incohérence mineure dans `Protos/battlefield.proto`** : `FireShotRequest` transmet `row`/`col` bruts, `ShotResult` les enveloppe dans un message `Coordinate` — conservé tel quel pour ne pas casser le contrat déjà implémenté et testé.

Détail complet, preuves et justifications : `docs/adr/`, `docs/REVUE-IA.md` et `_bmad-output/implementation-artifacts/deferred-work.md`.

## Traçabilité de l'usage de l'IA

Conformément aux exigences du cours, les échanges IA décisifs, les décisions d'architecture et les revues argumentées sont consignés dans :

- [`docs/PROMPTS.md`](docs/PROMPTS.md) — échanges IA décisifs, décision et preuve pour chacun.
- [`docs/adr/`](docs/adr/) — un Architecture Decision Record par décision structurante.
- [`docs/REVUE-IA.md`](docs/REVUE-IA.md) — revues argumentées (hypothèse, expérience, observation, décision, preuve).

## Structure du dépôt

- `BattleShip.Models` : moteur de jeu et DTOs partagés, sans aucune dépendance externe.
- `BattleShip.API` : Minimal API (HTTP) + service gRPC, validation, état en mémoire.
- `BattleShip.App` : interface Blazor WebAssembly.
- `BattleShip.Tests` : tests unitaires (`Unit/`) et d'intégration (`Integration/`).
- `Protos/battlefield.proto` : contrat gRPC partagé par chemin relatif entre l'API et l'App.
- `BattleShip.slnx` : solution racine.
- `docs/` : support de cours, `PROMPTS.md`, `REVUE-IA.md`, gabarit ADR et ADR réels (`docs/adr/`).
- `_bmad-output/` : artefacts de planification et de suivi de projet (spec, architecture, épics, specs de story, rétrospectives).
