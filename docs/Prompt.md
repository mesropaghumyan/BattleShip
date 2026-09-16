# Journal des échanges IA (PROMPTS)

Ce document consigne les échanges décisifs réalisés avec l'assistant IA au cours du développement du projet **BattleShip**, conformément au gabarit d'évaluation (Diapo 56 du cours).

---

## 1. Distillation du support de cours en spec (Epic 1)

- **Date** : 15 septembre 2026
- **Outil / modèle** : Claude Code / Sonnet 5
- **Contexte** : Juste après l'initialisation BMAD, avant toute architecture ou tout code. Le support de cours (`docs/cours_c_asp_net_bataille_navale.md`) mélange contraintes imposées (socle) et choix libres (règles, flotte, adversaire, backlog) ; il fallait le condenser en un contrat exploitable pour piloter le reste du projet.
- **Prompt** :
  > `bmad-spec`, puis sur les 5 questions ouvertes restantes : « Je te laisse carte blanche sur les questions ouvertes. Le but c'est de faire le TP du prof dans les délais impartis. Il ne faut pas faire une solution trop simpliste car ça équivaudrait à une note minimale, il faut bien respecter les exigences énoncées par le support du cours. Il ne faut pas non plus faire de reverse engineering pour un petit projet comme celle-ci, il faut rester simple mais propre. »
- **Réponse résumée** :
  Distillation du socle imposé (Minimal API, Blazor WebAssembly, FluentValidation, gRPC, tests) en 9 capacités (CAP-1 à CAP-9) + contraintes + non-goals dans `SPEC.md`. Résolution des 5 questions ouvertes : grille 10x10, flotte classique à 5 navires, adversaire en mode chasse/cible pour la difficulté élevée, action de tir exposée en gRPC, état en mémoire uniquement, backlog limité à historique/statistiques/difficulté réglable (multijoueur et persistance explicitement écartés). Ajout de 3 capacités supplémentaires (CAP-10 à CAP-12) et d'un companion `game-design.md` pour le détail flotte/grille/algorithme.
- **Décision** : Acceptée. Auto-validation en deux passes (cohérence + préservation) au vert.
- **Vérification** : Relecture croisée avec les diapositives 5-6 du support de cours (contraintes du socle) pour s'assurer qu'aucune exigence n'était perdue.
- **Preuve** : `_bmad-output/specs/spec-battleship/SPEC.md`, `game-design.md`, `.memlog.md`.

---

## 2. Architecture technique (Epic 1)

- **Date** : 15 septembre 2026
- **Outil / modèle** : Claude Code / Sonnet 5
- **Contexte** : Traduction de la spec en décisions d'architecture engageantes (paradigme, frontières, gestion d'état) avant le découpage en stories.
- **Prompt** :
  > `lance l'architecture` — mode « Fast path » choisi, usage « substrat de build interne uniquement, pas de support de présentation ».
- **Réponse résumée** :
  Paradigme « noyau de domaine isolé + adaptateurs minces » (`BattleShip.Models` sans dépendance externe). 11 décisions d'architecture rédigées : placement du domaine, frontière de visibilité HTTP/gRPC, propriété/concurrence de l'état, répartition des transports, validation, stratégie adverse, organisation des tests, frontière App/API, source du contrat gRPC, identité des camps, mapping des codes d'erreur. Revue à 3 couches (vérification des versions + adversariale) : 2 findings critiques corrigés (round-trip du tour de jeu non fixé, véhicule des deux vues non fixé) et 3 *high* (concurrence sur l'état, codes d'erreur, `PlayerId` orphelin remplacé par `enum Side`).
- **Décision** : Acceptée après corrections.
- **Vérification** : Versions des paquets NuGet vérifiées sur le web (FluentValidation 12.1.1, Grpc.* 2.80.0/2.83.0) ; lint déterministe du spine.
- **Preuve** : `_bmad-output/planning-artifacts/architecture/architecture-BattleShip-2026-09-15/ARCHITECTURE-SPINE.md` et `reviews/`.

---

## 3. Découpage en epics/stories et planification de sprint (Epic 1)

- **Date** : 15 septembre 2026
- **Outil / modèle** : Claude Code / Sonnet 5
- **Contexte** : Découpage de la spec et de l'architecture en epics et stories livrables, puis génération du suivi de sprint, avant tout code.
- **Prompt** :
  > `lance les epics et stories`, puis `lance le Sprint Planning`.
- **Réponse résumée** :
  3 epics (socle jouable de bout en bout, extensions, traçabilité IA) / 13 stories avec critères d'acceptation Given/When/Then, mappées aux capacités de la spec et aux décisions d'architecture. Chevauchement de fichiers entre Epic 1 et Epic 2 jugé justifié : l'Epic 1 livre déjà une valeur autonome complète (partie jouable conforme au socle). Génération de `sprint-status.yaml` via le script déterministe du skill.
- **Décision** : Acceptée.
- **Vérification** : Audit de couverture (chaque exigence fonctionnelle mappée à un epic, aucune dépendance en avant entre stories).
- **Preuve** : `_bmad-output/planning-artifacts/epics.md`, `_bmad-output/implementation-artifacts/sprint-status.yaml`.

---

## 4. Story 1.1 — Moteur de jeu : grilles et placement des flottes

- **Date** : 15 septembre 2026
- **Outil / modèle** : Claude Code / Sonnet 5
- **Contexte** : Première story de l'Epic 1, fondation du moteur de jeu.
- **Prompt** :
  > `lance la Story 1.1`
- **Réponse résumée** :
  `Grid`, `Ship`, `GameEngine` dans `BattleShip.Models/Domain` : placement aléatoire d'une flotte de 5 navires par tirage-rejet, sans chevauchement ni débordement, zéro dépendance transport. Revue automatisée (« Blind Hunter ») : 6 findings corrigés (référence de projet `Tests → Models` manquante, validation de géométrie des navires, boucle de tirage-rejet non bornée, tests manquants sur `Ship`/`Coordinate`). Walkthrough humain ensuite : un bug supplémentaire trouvé et corrigé (`Ship.Cells` gardait la référence de la liste appelante sans copie défensive).
- **Décision** : Acceptée en walkthrough.
- **Vérification** : `dotnet build`/`dotnet test` → 19 tests au vert.
- **Preuve** : commits `3dc8c44`, `948d0f6`.

---

## 5. Story 1.2 — Moteur de jeu : résolution des tirs et fin de partie

- **Date** : 15 septembre 2026
- **Outil / modèle** : Claude Code / Sonnet 5
- **Contexte** : Résolution des tirs et détection de fin de partie, sur la base du moteur de la Story 1.1.
- **Prompt** :
  > `lance la Story 1.2`
- **Réponse résumée** :
  Agrégat `Game` (deux `Grid`, statut, vainqueur), `Grid.ResolveShot` (Miss/Hit/Sunk, rejet d'un coup déjà joué sans effet de bord), `enum Side`. Revue : 4 findings corrigés (test à deux navires manquant pour distinguer `All`/`Any` sur `IsFleetSunk`, bornes hors-grille incomplètes, valeur `Side` invalide acceptée silencieusement, constructeur `Game` sans garde).
- **Décision** : Acceptée.
- **Vérification** : 40 tests au vert.
- **Preuve** : commit `6ee0c42`.

---

## 6. Story 1.3 — Adversaire ordinateur (stratégie facile)

- **Date** : 15 septembre 2026
- **Outil / modèle** : Claude Code / Sonnet 5
- **Contexte** : Adversaire ordinateur en mode facile, dernier morceau du moteur avant l'exposition réseau.
- **Prompt** :
  > `lance la Story 1.3`
- **Réponse résumée** :
  `IOpponentStrategy` + `EasyOpponentStrategy` (tir aléatoire uniforme sur les cases non jouées). Revue : garde `null` ajoutée, `Grid.RemainingCells()` extrait pour réutilisation future, déterminisme du `Random` injecté verrouillé par test. 1 finding reporté : l'interface expose toute la grille (pas seulement l'historique des tirs) — à trancher à l'ouverture de la Story 2.1.
- **Décision** : Acceptée.
- **Vérification** : 45 tests au vert.
- **Preuve** : commit `a06ab56`, `_bmad-output/implementation-artifacts/deferred-work.md`.

---

## 7. Story 1.4 — API : créer une partie et consulter son état

- **Date** : 15 septembre 2026
- **Outil / modèle** : Claude Code / Sonnet 5
- **Contexte** : Première exposition réseau (HTTP), à partir d'un scaffold encore en configuration de démonstration.
- **Prompt** :
  > `lance la Story 1.4` — spec en mode « dispatch », implémentation déléguée à un sous-agent sans contexte préalable à partir de la seule spec.
- **Réponse résumée** :
  `POST /games` / `GET /games/{id}`, `IGameStore` en mémoire (singleton, accès atomique par partie), `GameService`, `GameViewMapper` (frontière de visibilité), `CreateGameRequestValidator`. Revue à 3 couches : 10 findings corrigés (gestion d'exceptions globale via `ProblemDetails`, README/`.http` obsolètes, test mal nommé, 404 structuré, garde sur `Winner`, contrat « enums en toutes lettres sur le fil » verrouillé par test).
- **Décision** : Acceptée.
- **Vérification** : 58 tests au vert ; vérification indépendante par `curl` (création, lecture, formes de réponse).
- **Preuve** : commit `3293e62`.

---

## 8. Story 1.5 — API : jouer un coup via gRPC

- **Date** : 15 septembre 2026
- **Outil / modèle** : Claude Code / Sonnet 5
- **Contexte** : Exigence gRPC obligatoire du socle du cours.
- **Prompt** :
  > `lance la Story 1.5`
- **Réponse résumée** :
  Service gRPC `Battlefield.FireShot` (`Protos/battlefield.proto` à la racine du dépôt, référencé par chemin relatif, sans `ProjectReference`) : un seul appel résout le tir du joueur puis, si la partie continue, la riposte immédiate de l'ordinateur. Mapping des codes d'erreur gRPC (`InvalidArgument`/`FailedPrecondition`/`NotFound`). Revue : test manquant sur la branche « défaite » (`Lost`) corrigé, bornes de validation complétées, documentation XML enrichie.
- **Décision** : Acceptée.
- **Vérification** : 71 tests au vert (dont un round-trip gRPC complet en canal in-process).
- **Preuve** : commit `48f33b3`.

---

## 9. Story 1.6 — Interface Blazor : créer une partie et jouer

- **Date** : 15 septembre 2026
- **Outil / modèle** : Claude Code / Sonnet 5
- **Contexte** : Interface Blazor WebAssembly — dernière story du socle, rend le jeu jouable dans un navigateur.
- **Prompt** :
  > `lance la Story 1.6`
- **Réponse résumée** :
  `NewGame.razor` / `Play.razor`, composant réutilisable `BattleGrid`, `GameHttpClient` / `ShotGrpcClient`, politique CORS nommée côté API limitée à l'origine de l'App. Vérification manuelle réelle (navigateur + script Playwright de contrôle) ayant trouvé et corrigé un bug de désérialisation JSON (camelCase côté client). Revue à 3 couches : 7 findings corrigés (état périmé après un rafraîchissement réseau en échec, garde de nullité manquante, duplication de `Grid.Size`, etc.).
- **Décision** : Acceptée par l'utilisateur après test manuel en local (« Super ça marche »).
- **Vérification** : 71 tests au vert ; partie complète jouée dans le navigateur.
- **Preuve** : commit `f4d4e4b`.

---

## 10. Correction post-clôture : port HTTPS jamais démarré par défaut

- **Date** : 15 septembre 2026
- **Outil / modèle** : Claude Code / Sonnet 5
- **Contexte** : Premier lancement réel par l'utilisateur après la clôture de l'Epic 1 : `POST /games` échouait systématiquement (« Impossible de créer la partie » + « An unhandled error has occurred »).
- **Prompt** :
  > « Lors[que] je clique sur nouvelle partie j'ai le message d'erreur... » (capture d'écran Chrome DevTools à l'appui).
- **Réponse résumée** :
  Diagnostic par inspection des ports en écoute (`lsof`) : `dotnet run` sans `--launch-profile` utilise le profil `http` par défaut, mais l'adresse de l'API côté client Blazor était codée en dur sur le port `https` (7164), jamais démarré — avec en plus `UseHttpsRedirection()` qui redirigeait vers ce port muet. Bascule de l'App et de l'API sur `http` en local, retrait de la redirection HTTPS (démo locale uniquement).
- **Décision** : Acceptée, confirmée fonctionnelle par l'utilisateur.
- **Vérification** : `curl` (plus de redirection, CORS/en-têtes gRPC-Web intacts) puis test réel par l'utilisateur dans le navigateur.
- **Preuve** : commit `9525262`.

---

## 11. Rétrospective de l'Epic 1

- **Date** : 15-16 septembre 2026
- **Outil / modèle** : Claude Code / Sonnet 5
- **Contexte** : Clôture de l'Epic 1 (6/6 stories `done`), rétrospective demandée avant de démarrer l'Epic 2.
- **Prompt** :
  > `Réalise la retrospective de l'epic 1`
- **Réponse résumée** :
  Rétrospective structurée (skill `bmad-retrospective`) sur la plage `df1c1f0..3959ea6` : vues d'ensemble (aucun god-class détecté, protections AD-1/AD-8 vivantes via tests de régression dédiés, duplication réelle des options JSON entre serveur et client, contournements répétés de la contrainte `IGameStore.WithGame<TResult> : class`) puis revue à 3 lenses (adversarial, edge-case, verification-gap) via le skill `bmad-review`. Un bug réel supplémentaire trouvé indépendamment par les 3 lenses (statut resté figé sur « en cours » dans le repli local de `Play.razor` après un rafraîchissement réseau en échec) — corrigé en parallèle par une autre session avant la clôture de cette rétrospective (commit `13e84d9`).
- **Décision** : En cours au moment de la rédaction de cette entrée.
- **Vérification** : 3 lenses de revue indépendantes + vues d'ensemble dérivées de `git_evidence.py`.
- **Preuve** : `_bmad-output/implementation-artifacts/epic-1-retro-2026-09-15.md`.

---

## 12. Analyse d'indépendance des stories de l'Epic 2

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

## 13. Développement de la Story 2.1 et de ses tests unitaires

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

## 14. Mise en place du journal des prompts

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

## 15. Développement de la Story 2.2 : sélection de la difficulté

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
