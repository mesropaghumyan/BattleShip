---
stepsCompleted: [step-01, step-02]
inputDocuments: ['_bmad-output/specs/spec-battleship/SPEC.md', '_bmad-output/specs/spec-battleship/game-design.md', '_bmad-output/planning-artifacts/architecture/architecture-BattleShip-2026-09-15/ARCHITECTURE-SPINE.md']
---

# BattleShip - Epic Breakdown

## Overview

This document provides the complete epic and story breakdown for BattleShip, decomposing SPEC-battleship (capabilities CAP-1..CAP-12), its game-design.md companion, and the ARCHITECTURE-SPINE.md (AD-1..AD-11) into implementable stories.

## Requirements Inventory

### Functional Requirements

FR1: Le moteur de jeu crée deux grilles (10x10), place les flottes aléatoirement (5 navires : 5/4/3/3/2 cases) et résout les tirs jusqu'à la fin de partie, indépendamment du transport et de l'UI. (CAP-1)
FR2: Un contrat d'API Minimal API permet de créer une partie, connaître son état (`GameStateDto` : `OwnGrid` + `OpponentGrid`) et la faire évoluer, en respectant les règles de visibilité. (CAP-2)
FR3: Une boucle de jeu fait alterner joueur et ordinateur, produit un résultat compréhensible et interdit tout coup après la fin de partie. (CAP-3)
FR4: Un adversaire ordinateur joue des coups valides ; en mode facile il tire aléatoirement, en mode difficile il applique une stratégie chasse/cible (hunt & target) avec parité damier. (CAP-4)
FR5: Une interface Blazor WebAssembly permet de créer une partie, afficher les deux grilles, jouer et voir les résultats/fin de partie, avec gestion des incidents de communication. (CAP-5)
FR6: L'action de tir (jouer un coup) est exposée exclusivement en gRPC (via gRPC-Web), et résout en une seule requête le tir du joueur + la riposte de l'ordinateur (`ShotTurnReply`). (CAP-6)
FR7: Les entrées serveur (HTTP et gRPC) sont validées avec FluentValidation, avec un mapping de codes d'erreur gRPC explicite par cas. (CAP-7)
FR8: Des tests métier et d'intégration couvrent les règles du jeu et les contrats, capables de détecter une règle violée (pas seulement le cas nominal). (CAP-8)
FR9: L'équipe documente son usage de l'IA via `PROMPTS.md`, `docs/adr/`, `REVUE-IA.md` (≥ 3 revues argumentées). (CAP-9)
FR10: L'interface affiche un historique chronologique des coups joués (camp, coordonnée, résultat). (CAP-10)
FR11: Un récapitulatif de fin de partie affiche le nombre de tirs, le taux de réussite et la durée, par camp. (CAP-11)
FR12: L'utilisateur choisit la difficulté de l'adversaire (facile/difficile) avant de lancer une partie. (CAP-12)

### NonFunctional Requirements

NFR1: API imposée en ASP.NET Core Minimal API sur .NET 10 (pas de contrôleurs MVC).
NFR2: Front imposé en Blazor WebAssembly.
NFR3: `BattleShip.Models` ne dépend d'aucun autre projet (noyau de domaine transport-indépendant).
NFR4: FluentValidation obligatoire sur toutes les entrées serveur.
NFR5: Au moins un échange gRPC fonctionnel (gRPC-Web) est obligatoire.
NFR6: Tests métier et d'intégration obligatoires, capables de détecter une règle violée.
NFR7: Historique Git exploitable (commits relus par le binôme).
NFR8: Délai fixe de 5 jours ; seul le commit poussé avant le début du QCM du jour 5 est évalué.
NFR9: Le projet doit pouvoir être lancé par un autre binôme en suivant uniquement le `README.md`.
NFR10: État des parties conservé en mémoire uniquement (pas de BD ni fichier de persistance).
NFR11: `BattleShip.App` ne prend jamais de `ProjectReference` vers `BattleShip.API` (communication réseau uniquement).

### Additional Requirements

- **Pas de starter template à générer** : le scaffold des 4 projets (.NET 10 ; API = Minimal API ; App = Blazor WebAssembly ; Models = classlib ; Tests = xUnit) existe déjà dans le dépôt et est adopté tel quel `[ADOPTED]`. L'Epic 1 enrichit le scaffold existant, il ne régénère pas la solution.
- Le contrat gRPC (`battlefield.proto`) vit dans un dossier `Protos/` à la racine du dépôt, référencé par chemin relatif depuis `API` (Server) et `App` (Client), jamais via `ProjectReference` (AD-9, AD-8).
- L'état de partie est géré via une abstraction unique `IGameStore` (Singleton, `ConcurrentDictionary<Guid, Game>`, méthode atomique `Mutate`), accédée uniquement via `GameService` — jamais directement par les endpoints ou le service gRPC (AD-3).
- Codes d'erreur gRPC fixés par cas : coordonnée hors grille → `InvalidArgument` ; case déjà jouée / tir après fin de partie → `FailedPrecondition` ; `GameId` inconnu → `NotFound` (AD-11).
- Identité des camps via `enum Side { Human, Computer }`, pas d'identifiant joueur individuel (AD-10).
- CORS : une seule politique nommée dans l'API, limitée à l'origine de l'App.
- Organisation des tests : un seul projet `BattleShip.Tests`, dossiers `Unit/` (domaine pur) et `Integration/` (`WebApplicationFactory`) — pas de nouveau projet de test (AD-7).

### UX Design Requirements

Aucun document UX séparé (pas de run `bmad-ux`). Les exigences d'interface sont couvertes par FR5, FR10, FR11, FR12 (SPEC.md CAP-5/10/11/12) et par la convention de coordonnées du spine (notation A1..J10 à l'affichage uniquement, `Coordinate(Row, Col)` en interne).

### FR Coverage Map

FR1: Epic 1 - Moteur de jeu (grilles, flotte, résolution des tirs, fin de partie)
FR2: Epic 1 - Contrat API HTTP (créer/consulter une partie)
FR3: Epic 1 - Boucle de jeu (alternance joueur/ordinateur, fin de partie)
FR4: Epic 1 (stratégie facile) + Epic 2 (stratégie difficile) - Adversaire ordinateur
FR5: Epic 1 - Interface Blazor de jeu
FR6: Epic 1 - Tir en gRPC (round-trip joueur + riposte IA)
FR7: Epic 1 - Validation FluentValidation + codes d'erreur gRPC
FR8: Epic 1 & Epic 2 (transverse) - Tests métier et d'intégration inclus dans chaque story
FR9: Epic 3 - Livrables IA (PROMPTS.md, docs/adr/, REVUE-IA.md)
FR10: Epic 2 - Historique des coups joués
FR11: Epic 2 - Statistiques de fin de partie
FR12: Epic 2 - Difficulté réglable (sélection avant partie)

## Epic List

### Epic 1: Partie jouable de bout en bout contre l'ordinateur
Le joueur crée une partie, joue une partie complète contre un ordinateur (adversaire aléatoire), voit le résultat de chaque tir et la fin de partie. Le socle technique complet est démontrable : Minimal API, Blazor WebAssembly, gRPC-Web pour le tir, FluentValidation. Chaque story inclut ses tests (FR8).
**FRs covered:** FR1, FR2, FR3, FR4 (stratégie facile), FR5, FR6, FR7

### Epic 2: Adversaire plus fort, historique et statistiques
Le joueur choisit la difficulté avant de lancer une partie (dont un adversaire "difficile" en mode chasse/cible), suit l'historique des coups en direct pendant la partie, et consulte des statistiques (tirs, taux de réussite, durée) à la fin de partie. Chaque story inclut ses tests (FR8).
**FRs covered:** FR4 (stratégie difficile), FR10, FR11, FR12

### Epic 3: Traçabilité IA et préparation de la remise
L'équipe dispose de PROMPTS.md, docs/adr/ et REVUE-IA.md à jour et reliés à des preuves reproductibles, et d'un README.md permettant à un autre binôme de lancer et comprendre le projet. Les entrées PROMPTS/ADR/REVUE-IA se documentent au fil de l'eau dès l'Epic 1 ; cet epic regroupe la mise en place du processus et l'audit final avant remise.
**FRs covered:** FR9 (+ NFR7, NFR8, NFR9)
