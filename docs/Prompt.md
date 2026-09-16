# Journal des échanges IA (PROMPTS)

Ce document consigne les échanges décisifs réalisés avec l'assistant IA au cours du développement du projet **BattleShip**, conformément au gabarit d'évaluation (Diapo 56 du cours).

---

## 1. Analyse d'indépendance des stories de l'Epic 2

- **Date** : 15 septembre 2026
- **Outil / modèle** : Antigravity CLI / Gemini 3.8 Flash (High)
- **Contexte** : Planification du sprint et organisation du travail en binôme. L'Epic 1 était encore en cours (Story 1.5 gRPC et Story 1.6 Blazor non terminées), et l'objectif était d'identifier quelle tâche de l'Epic 2 pouvait être démarrée immédiatement en parallèle sans blocage technique ni dépendance réseau/UI.
- **Prompt** :
  > `bmad-help quel story de l'epics 2 est la plus indépendante de l'epics 1`
- **Réponse résumée** :
  L'assistant a analysé les 4 stories de l'Epic 2 et a identifié la **Story 2.1 (Adversaire ordinateur — stratégie difficile chasse/cible)** comme étant la plus indépendante de l'Epic 1.
  - **Raisons** : Logique métier pure (Domain Kernel) située exclusivement dans `BattleShip.Models`, aucune dépendance sur les transports HTTP/gRPC (Stories 1.4/1.5) ni sur l'UI Blazor (Story 1.6), et testabilité unitaire à 100% dans `BattleShip.Tests/Unit`.
  - Comparaison avec les autres stories qui dépendent toutes de la stack complète (Story 2.2 nécessite l'API et Blazor, Story 2.3 nécessite gRPC et Blazor, Story 2.4 dépend de l'historique et de l'UI).
- **Décision** : Acceptée. Décision de démarrer immédiatement l'implémentation de la Story 2.1 en isolation sur la branche dédiée `fra_epic_2`.
- **Vérification** : Examen croisé du document d'architecture (`ARCHITECTURE-SPINE.md`, AD-1, AD-6) et du découpage des stories (`epics.md`).
- **Preuve** : Documentation d'architecture `_bmad-output/planning-artifacts/architecture/architecture-BattleShip-2026-09-15/ARCHITECTURE-SPINE.md` et backlog `_bmad-output/planning-artifacts/epics.md`.

---

## 2. Développement de la Story 2.1 et de ses tests unitaires

- **Date** : 15 septembre 2026
- **Outil / modèle** : Antigravity CLI / Gemini 3.8 Flash (High)
- **Contexte** : Implémentation de la stratégie difficile (CAP-4, CAP-12, FR4, FR8) sur la branche `fra_epic_2`. Prise en compte du retour de revue différé issu de la Story 1.3 consigné dans `deferred-work.md` (éviter que la stratégie n'accède directement à `Grid.Ships` pour "tricher").
- **Prompt** :
  > `Sur la branche fra_epic_2, développe la story 2.1 (les tests associés, également, comme lors de la réalisation de l'epic 1)`
- **Réponse résumée** :
  1. **Évolution du domaine (`Grid.cs`)** :
     - Ajout de `GetShotOutcome(Coordinate)` : retourne le résultat d'un tir déjà joué (raté/touché/coulé) sans jamais révéler les navires non découverts.
     - Ajout de `UnsunkHits` : expose les coordonnées des tirs ayant touché un navire non encore entièrement coulé. Cela résout le point différé de la Story 1.3 sans faire fuiter d'information.
  2. **Implémentation de `HardOpponentStrategy.cs`** :
     - Implémentation de `IOpponentStrategy` avec constructeur acceptant un `Random?` optionnel pour assurer le déterminisme des tests.
     - **Phase 1 (Recherche / Hunt)** : tirs sur les cases du damier de parité `(Row + Col) % 2 == 0` parmi les cases non encore jouées, avec repli sur cases impaires si tout le damier est joué.
     - **Phase 2 (Ciblage / Target)** : ciblage de l'une des 4 cases adjacentes valides (en grille et non jouées) autour d'un tir touché non coulé.
     - **Phase 3 (Alignement / Line)** : identification de l'axe (horizontal ou vertical) dès que deux touches sont alignées, tir aux extrémités de l'axe ou comblement de trou intérieur, et retour en recherche si l'axe échoue ou dès que le navire est coulé.
  3. **Tests unitaires exhaustifs (`HardOpponentStrategyTests.cs` et `GridTests.cs`)** :
     - 16 tests pour `HardOpponentStrategy` couvrant les 3 phases, les cas limites (coins de grille, repli damier, détection de grille pleine, grille nulle, déterminisme).
     - Test de simulation d'une partie complète jusqu'à destruction de la flotte (garantie du non-rejeu d'une case déjà jouée).
     - Test de détection de règle violée (FR8).
     - 5 tests unitaires sur `GridTests.cs` pour `GetShotOutcome` et `UnsunkHits`.
  4. **Artefacts de traçabilité BMAD** :
     - Création de `spec-2-1-adversaire-ordinateur-stratégie-difficile-chasse-cible.md`.
     - Mise à jour de `sprint-status.yaml` (`epic-2: in-progress`, `2-1-...: done`).
     - Clôture du point différé dans `deferred-work.md`.
- **Décision** : Acceptée. L'implémentation est purement algorithmique, découplée de l'infrastructure, déterministe sous test, et respecte l'interdiction de fuite d'information sur la flotte adverse.
- **Vérification** :
  - Compilation générale : `dotnet build` -> 0 avertissement, 0 erreur.
  - Exécution des tests : `dotnet test` -> 81 tests réussis sur 81 (58 initiaux + 21 nouveaux), 0 échec, durée 526 ms.
- **Preuve** :
  - Code source : `BattleShip.Models/Domain/Opponent/HardOpponentStrategy.cs` et `BattleShip.Models/Domain/Grid.cs`.
  - Tests : `BattleShip.Tests/Unit/HardOpponentStrategyTests.cs` et `BattleShip.Tests/Unit/GridTests.cs`.
  - Spécification : `_bmad-output/implementation-artifacts/spec-2-1-adversaire-ordinateur-stratégie-difficile-chasse-cible.md`.
  - État Git : commit `ebb2e3b` (`feat(models): adversaire ordinateur en mode difficile chasse/cible (Story 2.1)`) sur la branche `fra_epic_2`.

---

## 3. Mise en place du journal des prompts

- **Date** : 15 septembre 2026
- **Outil / modèle** : Antigravity CLI / Gemini 3.8 Flash (High)
- **Contexte** : Traçabilité et justification des contributions IA pour la remise du projet (exigence CAP-9 / FR9 / Story 3.1).
- **Prompt** :
  > `Dans docs/ créee prompt.md qui listera nos echanges`
- **Réponse résumée** :
  Création de ce document `docs/prompt.md` structuré selon les rubriques requises par le cours (Date/sujet, Outil/modèle, Contexte, Prompt, Réponse résumée, Décision, Vérification, Preuve) pour consigner les échanges décisifs.
- **Décision** : Acceptée.
- **Vérification** : Fichier créé dans `docs/prompt.md` avec toutes les rubriques du gabarit d'évaluation.
- **Preuve** : `docs/prompt.md`.

---

## 4. Développement de la Story 2.2 : sélection de la difficulté

- **Date** : 16 septembre 2026
- **Outil / modèle** : GitHub Copilot
- **Contexte** : La Story 2.1 étant validée sur `fra_epic_2`, la Story 2.2 devait rendre les stratégies facile et difficile sélectionnables à la création d'une partie, sans modifier la boucle de jeu.
- **Prompt** :
  > `Poursuit le travail de ce prompt. Maintenant que la story 2.1 a été validée et testée, sur la branche fra_epic_2, développe la story 2.2 (les tests associés également, comme lors de la réalisation de la story 2.1) et alimente docs/Prompt.md.`
- **Réponse résumée** :
  1. La difficulté est conservée dans `Game` et exposée dans `GameStateDto`.
  2. `Game.ResolveStrategy` associe `Easy` à `EasyOpponentStrategy` et `Hard` à `HardOpponentStrategy`; `GameService` conserve cette stratégie avec la partie.
  3. La validation accepte les deux valeurs connues et rejette toujours une valeur absente ou invalide.
  4. `NewGame.razor` propose un sélecteur facile/difficile et transmet le choix via `GameHttpClient` à `POST /games`.
  5. Les tests d'intégration vérifient qu'une création `Hard` renvoie 201 et enregistre bien `HardOpponentStrategy`; les anciens tests hérités de la Story 1.4 ont été adaptés à ce nouveau contrat.
- **Décision** : Acceptée. La sélection repose sur la difficulté persistée au niveau de la partie et reste compatible avec l'architecture `IOpponentStrategy`.
- **Vérification** : `dotnet test --no-restore` -> 96 tests réussis sur 96, 0 échec.
- **Preuve** : `BattleShip.Models/Domain/Game.cs`, `BattleShip.API/Services/GameService.cs`, `BattleShip.App/Pages/NewGame.razor`, `BattleShip.App/Services/GameHttpClient.cs`, `BattleShip.Tests/Integration/GameEndpointsTests.cs` et `BattleShip.Tests/Unit/GameTests.cs`.
