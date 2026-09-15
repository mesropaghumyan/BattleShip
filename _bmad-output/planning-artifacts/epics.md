---
stepsCompleted: [step-01, step-02, step-03]
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

## Epic 1: Partie jouable de bout en bout contre l'ordinateur

Le joueur crée une partie, joue une partie complète contre un ordinateur (adversaire aléatoire), voit le résultat de chaque tir et la fin de partie. Le socle technique complet est démontrable : Minimal API, Blazor WebAssembly, gRPC-Web pour le tir, FluentValidation.

### Story 1.1: Moteur de jeu — grilles et placement des flottes

As a joueur,
I want que le système prépare deux grilles 10x10 avec une flotte de 5 navires placée aléatoirement sans chevauchement ni débordement,
So that une partie peut démarrer sur un état de jeu valide et équitable. (FR1, CAP-1, AD-1)

**Acceptance Criteria:**

**Given** une nouvelle partie à initialiser
**When** le moteur (`GameEngine`) place la flotte (Porte-avions 5, Croiseur 4, Contre-torpilleur 3, Sous-marin 3, Torpilleur 2) sur une grille 10x10
**Then** aucun navire ne chevauche un autre et aucun navire ne dépasse les limites de la grille
**And** l'orientation de chaque navire est horizontale ou verticale, jamais diagonale

**Given** deux grilles générées pour une même partie (joueur et ordinateur)
**When** on inspecte les deux placements
**Then** ils sont indépendants et générés séparément (aucune corrélation entre les deux flottes)

**Given** le moteur de placement
**When** des tests unitaires s'exécutent dans `BattleShip.Tests/Unit`
**Then** ils couvrent : absence de chevauchement, absence de débordement, respect de la composition de flotte (FR8)
**And** ces tests ne référencent que `BattleShip.Models` (AD-7)

### Story 1.2: Moteur de jeu — résolution des tirs et fin de partie

As a joueur,
I want que chaque tir sur une grille soit résolu (raté/touché/coulé) et que la partie détecte sa propre fin,
So that une partie complète peut se jouer jusqu'à la victoire. (FR1, FR3, CAP-1, CAP-3, AD-1)

**Acceptance Criteria:**

**Given** une partie initialisée (Story 1.1)
**When** `Game` applique un tir sur une case non encore jouée
**Then** le résultat est raté, touché, ou coulé (si toutes les cases du navire sont touchées) et l'état de la partie est mis à jour

**Given** une case déjà jouée sur la même grille
**When** un nouveau tir cible cette case
**Then** le tir est rejeté sans modifier l'état de la partie et ne compte pas comme un nouveau coup

**Given** tous les navires d'un camp sont coulés
**When** le dernier tir touche le dernier navire restant
**Then** la partie passe au statut `Won`/`Lost` selon le camp, et le camp gagnant est identifié

**Given** une partie au statut `Won`/`Lost`
**When** un nouveau tir est tenté
**Then** il est rejeté (aucun coup accepté après la fin de partie)

**Given** la logique de résolution des tirs
**When** des tests unitaires s'exécutent
**Then** ils couvrent chaque cas ci-dessus, y compris un test qui détecterait une règle violée (ex. compter un tir sur case déjà jouée) avant/après correction (FR8)

### Story 1.3: Adversaire ordinateur — stratégie aléatoire (facile)

As a joueur,
I want que l'ordinateur joue des coups valides selon une stratégie aléatoire simple,
So that je peux jouer une partie complète contre un adversaire cohérent. (FR4, CAP-4, AD-6)

**Acceptance Criteria:**

**Given** l'interface `IOpponentStrategy` définie dans le domaine
**When** `EasyOpponentStrategy.ChooseShot` est appelée avec l'état de la grille visible par l'ordinateur
**Then** elle retourne une coordonnée non encore jouée, choisie aléatoirement

**Given** une grille où toutes les cases sauf une ont déjà été jouées
**When** `EasyOpponentStrategy.ChooseShot` est appelée
**Then** elle retourne cette dernière case restante (jamais une case déjà jouée)

**Given** la stratégie facile
**When** des tests unitaires s'exécutent
**Then** ils vérifient qu'elle ne rejoue jamais une case déjà jouée et qu'elle respecte les mêmes règles de validité qu'un tir humain (FR8)

### Story 1.4: API — créer une partie et consulter son état

As a joueur,
I want créer une partie via l'API et consulter son état sans voir les navires adverses non découverts,
So that le client dispose d'un état de jeu fiable et respectueux du secret du jeu. (FR2, FR7, CAP-2, CAP-7, AD-2, AD-3, AD-5)

**Acceptance Criteria:**

**Given** une requête `POST /games` valide (difficulté = facile pour cet epic)
**When** l'endpoint est appelé
**Then** une nouvelle partie est créée via `GameService`/`IGameStore` (Singleton, `Mutate` atomique) et son `GameId` est retourné

**Given** une requête `POST /games` invalide (ex. champ manquant)
**When** l'endpoint est appelé
**Then** FluentValidation rejette la requête avec un `ValidationProblem` (RFC 7807) et aucune partie n'est créée

**Given** une partie existante
**When** `GET /games/{id}` est appelé
**Then** la réponse est un `GameStateDto` composite avec `OwnGrid` (navires du joueur visibles) et `OpponentGrid` (uniquement touché/raté/coulé, aucune coordonnée de navire adverse non coulé)

**Given** un `GameId` inconnu
**When** `GET /games/{id}` est appelé
**Then** l'API retourne 404

**Given** le endpoint et le mapping `GameViewMapper`
**When** des tests d'intégration s'exécutent via `WebApplicationFactory`
**Then** ils vérifient qu'aucune coordonnée de navire non coulé n'apparaît jamais dans `OpponentGrid` sérialisé (FR8, AD-2)

### Story 1.5: API — jouer un coup via gRPC

As a joueur,
I want envoyer un tir via gRPC et recevoir en une seule réponse le résultat de mon tir et celui de la riposte de l'ordinateur,
So that la boucle de jeu avance d'un tour complet à chaque action. (FR3, FR6, FR7, CAP-3, CAP-6, CAP-7, AD-3, AD-4, AD-9, AD-11)

**Acceptance Criteria:**

**Given** `battlefield.proto` défini dans `Protos/` (racine du dépôt, référencé par chemin relatif depuis API et App, sans `ProjectReference`)
**When** le client Blazor appelle `BattlefieldService.FireShot` en gRPC-Web avec un `GameId` et une coordonnée valides
**Then** le serveur applique le tir joueur via `GameService`, puis — si la partie n'est pas terminée — applique immédiatement la riposte de `EasyOpponentStrategy`
**And** `ShotTurnReply` porte le résultat du tir joueur, le résultat du tir ordinateur (absent si la partie s'est terminée après le tir joueur), et le statut de partie

**Given** une coordonnée hors grille
**When** `FireShot` est appelé
**Then** la réponse est une `RpcException` avec `StatusCode.InvalidArgument`

**Given** une case déjà jouée ou une partie déjà terminée
**When** `FireShot` est appelé
**Then** la réponse est une `RpcException` avec `StatusCode.FailedPrecondition`

**Given** un `GameId` inconnu
**When** `FireShot` est appelé
**Then** la réponse est une `RpcException` avec `StatusCode.NotFound`

**Given** le service gRPC
**When** des tests d'intégration s'exécutent via `WebApplicationFactory` (canal gRPC in-process)
**Then** ils couvrent le round-trip complet (tir + riposte) et chaque cas d'erreur avec son code attendu (FR8)

### Story 1.6: Interface Blazor — créer une partie et jouer

As a joueur,
I want créer une partie et jouer depuis le navigateur, avec les deux grilles et les résultats visibles,
So that je peux dérouler une partie complète du début à la victoire/défaite sans quitter l'interface. (FR5, CAP-5, AD-8)

**Acceptance Criteria:**

**Given** la page `NewGame.razor`
**When** le joueur crée une partie
**Then** l'application appelle `POST /games` (`GameHttpClient`) et navigue vers `Play.razor` avec le `GameId` obtenu

**Given** la page `Play.razor` avec une partie en cours
**When** le joueur clique sur une case de la grille adverse
**Then** l'application appelle `FireShot` en gRPC-Web (`ShotGrpcClient`) et affiche le résultat du tir joueur puis celui de l'ordinateur

**Given** un incident de communication (API indisponible, erreur réseau)
**When** l'appel HTTP ou gRPC échoue
**Then** un message d'erreur est affiché sans bloquer le reste de l'interface (l'utilisateur peut réessayer)

**Given** une partie qui se termine (`Won`/`Lost`)
**When** le statut de partie change
**Then** l'interface affiche clairement la fin de partie et le résultat (victoire/défaite)

**Given** `BattleShip.App`
**When** on inspecte ses références de projet
**Then** aucune `ProjectReference` vers `BattleShip.API` n'existe ; seuls les appels réseau (HTTP, gRPC-Web) relient les deux projets (AD-8)

## Epic 2: Adversaire plus fort, historique et statistiques

Le joueur choisit la difficulté avant de lancer une partie (dont un adversaire "difficile" en mode chasse/cible), suit l'historique des coups en direct pendant la partie, et consulte des statistiques (tirs, taux de réussite, durée) à la fin de partie.

### Story 2.1: Adversaire ordinateur — stratégie difficile (chasse/cible)

As a joueur,
I want affronter un adversaire ordinateur capable de cibler intelligemment mes navires après un coup touché,
So that la partie reste intéressante au-delà du tir purement aléatoire. (FR4, CAP-4, CAP-12, AD-6)

**Acceptance Criteria:**

**Given** `HardOpponentStrategy` implémentant `IOpponentStrategy`
**When** aucun tir précédent n'est touché (phase de recherche)
**Then** elle tire uniquement sur les cases d'une couleur du damier (parité `(x + y) % 2 == 0`), parmi les cases non encore jouées

**Given** un tir précédent touché non encore coulé (phase de ciblage)
**When** `HardOpponentStrategy.ChooseShot` est appelée
**Then** elle teste une des 4 cases adjacentes non encore jouées à ce tir touché

**Given** deux tirs touchés alignés (même ligne ou colonne) sur un navire non coulé
**When** `HardOpponentStrategy.ChooseShot` est appelée
**Then** elle poursuit dans l'axe identifié jusqu'à couler le navire ou essuyer un échec, puis revient en phase de recherche

**Given** la stratégie difficile
**When** des tests unitaires s'exécutent
**Then** ils couvrent les trois phases (recherche, ciblage, alignement) et vérifient qu'elle ne rejoue jamais une case déjà jouée (FR8)

### Story 2.2: Sélection de la difficulté avant la partie

As a joueur,
I want choisir la difficulté de l'ordinateur (facile ou difficile) au moment de créer une partie,
So that je peux adapter le niveau de challenge à mon envie. (FR12, CAP-12)

**Acceptance Criteria:**

**Given** la page `NewGame.razor`
**When** le joueur crée une partie
**Then** un sélecteur "facile / difficile" est proposé avant la création

**Given** une requête `POST /games` avec `Difficulty = Hard`
**When** l'endpoint traite la création
**Then** la partie est enregistrée avec `HardOpponentStrategy` comme adversaire (Story 2.1) ; `Difficulty = Easy` utilise `EasyOpponentStrategy` (Story 1.3)

**Given** une requête `POST /games` sans valeur de difficulté ou avec une valeur invalide
**When** FluentValidation traite la requête
**Then** elle est rejetée avec un `ValidationProblem` explicite

**Given** l'endpoint `POST /games` et le sélecteur Blazor
**When** des tests d'intégration s'exécutent
**Then** ils vérifient qu'une partie créée en mode difficile utilise bien `HardOpponentStrategy` pour ses ripostes (`FireShot`, Story 1.5) (FR8)

### Story 2.3: Historique des coups joués

As a joueur,
I want voir la liste chronologique des coups joués (par qui, où, avec quel résultat) pendant la partie,
So that je peux suivre le déroulement de la partie en cours. (FR10, CAP-10, AD-10)

**Acceptance Criteria:**

**Given** `Game` qui applique un tir (joueur ou ordinateur)
**When** le tir est résolu
**Then** un `Shot` horodaté est ajouté à l'historique de la partie, portant `Side` (`Human`/`Computer`), la coordonnée et le résultat

**Given** la page `Play.razor` avec plusieurs tirs déjà joués
**When** la vue se met à jour après un tour (`ShotTurnReply`, Story 1.5)
**Then** l'historique affiché liste chaque coup dans l'ordre chronologique, avec son camp, sa coordonnée et son résultat

**Given** l'historique de la partie
**When** un test de composant ou d'intégration s'exécute après plusieurs tours
**Then** il vérifie que l'ordre et le contenu de l'historique affiché correspondent aux coups réellement joués (FR8)

### Story 2.4: Statistiques de fin de partie

As a joueur,
I want voir un récapitulatif (nombre de tirs, taux de réussite, durée) pour chaque camp à la fin de la partie,
So that je peux évaluer ma performance et celle de l'ordinateur. (FR11, CAP-11, AD-10)

**Acceptance Criteria:**

**Given** une partie terminée (`Won`/`Lost`, Story 1.2) avec son historique de tirs (Story 2.3)
**When** les statistiques sont calculées
**Then** pour chaque `Side`, le nombre de tirs, le nombre de touches, le taux de réussite (touches / tirs) et la durée (entre création et fin de partie) sont corrects

**Given** la fin de partie détectée côté Blazor
**When** l'interface affiche `Summary.razor`
**Then** les statistiques des deux camps sont visibles à l'écran

**Given** le calcul des statistiques
**When** un test unitaire s'exécute avec un historique de tirs connu
**Then** il vérifie que les valeurs calculées (tirs, taux de réussite, durée) correspondent exactement aux données d'entrée (FR8)

## Epic 3: Traçabilité IA et préparation de la remise

L'équipe dispose de PROMPTS.md, docs/adr/ et REVUE-IA.md à jour et reliés à des preuves reproductibles, et d'un README.md permettant à un autre binôme de lancer et comprendre le projet. Les entrées PROMPTS/ADR/REVUE-IA se documentent au fil de l'eau dès l'Epic 1 ; cette story met en place le processus dès le début du sprint.

### Story 3.1: Mise en place du processus de traçabilité IA

As a équipe,
I want disposer dès le début du sprint des gabarits PROMPTS.md, docs/adr/ et REVUE-IA.md,
So that chaque décision et échange IA décisif peut être consigné au fil de l'eau, sans rattrapage de dernière minute. (FR9, CAP-9)

**Acceptance Criteria:**

**Given** la racine du dépôt
**When** le processus est mis en place (idéalement avant de démarrer l'Epic 1)
**Then** `PROMPTS.md` existe avec le gabarit (date/sujet, outil, contexte, prompt, réponse résumée, décision, vérification, preuve)
**And** `docs/adr/` existe avec un gabarit d'ADR (statut/date, contexte, options, décision, conséquences, vérification, références)
**And** `REVUE-IA.md` existe avec le gabarit de revue (proposition, hypothèse, expérience, observation, décision, preuves)

**Given** les gabarits en place
**When** l'équipe travaille sur l'Epic 1 et l'Epic 2
**Then** chaque échange IA décisif, chaque décision d'architecture déjà actée (ex. AD-1 à AD-11 du spine) et chaque revue de proposition IA sont consignés au fur et à mesure, avec un lien vers le commit ou le test concerné

### Story 3.2: Consolidation des livrables IA avant remise

As a équipe,
I want vérifier et compléter PROMPTS.md, docs/adr/ et REVUE-IA.md avant la remise,
So that les livrables IA respectent les exigences minimales du cours (≥ 3 revues argumentées, décisions structurantes documentées). (FR9, CAP-9)

**Acceptance Criteria:**

**Given** `REVUE-IA.md` en fin de sprint
**When** l'équipe relit le fichier
**Then** il contient au moins 3 revues, chacune avec une hypothèse vérifiable, une expérience décrite avant exécution, une observation réelle, une décision justifiée et un lien vers un commit/test reproductible

**Given** `docs/adr/`
**When** l'équipe relit les ADR
**Then** chaque décision structurante réellement prise (ex. choix du store en mémoire, découpage HTTP/gRPC, stratégie adverse) a un ADR à jour avec un statut (accepté/remplacé)

**Given** `PROMPTS.md`
**When** l'équipe relit le fichier
**Then** il couvre les échanges IA réellement décisifs du projet (pas une transcription intégrale), chacun relié à une décision et une preuve

### Story 3.3: README de remise

As a équipe,
I want un README.md complet et vérifié par un tiers,
So that un autre binôme peut lancer et comprendre le projet sans aide supplémentaire, condition de la remise. (NFR7, NFR8, NFR9)

**Acceptance Criteria:**

**Given** un binôme externe sans connaissance préalable du projet
**When** il suit uniquement les instructions du `README.md`
**Then** il parvient à restaurer les dépendances, compiler, lancer les tests, démarrer l'API et l'application, et jouer une partie complète

**Given** le `README.md`
**When** l'équipe le relit avant remise
**Then** il documente les fonctionnalités livrées, les arbitrages du backlog (ce qui a été choisi/écarté et pourquoi) et les limites connues (ex. état en mémoire uniquement, pas de multijoueur)

**Given** l'historique Git du dépôt
**When** l'équipe vérifie avant remise
**Then** les commits sont relus par le binôme et le travail est lisible (NFR7) ; le commit final est poussé avant le début du QCM du jour 5 (NFR8)
