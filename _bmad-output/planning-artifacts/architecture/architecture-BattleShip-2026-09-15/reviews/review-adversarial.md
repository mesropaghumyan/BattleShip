---
name: 'Revue adversariale — ARCHITECTURE-SPINE Bataille Navale'
type: review
target: '_bmad-output/planning-artifacts/architecture/architecture-BattleShip-2026-09-15/ARCHITECTURE-SPINE.md'
sources:
  - '_bmad-output/specs/spec-battleship/SPEC.md'
  - '_bmad-output/specs/spec-battleship/game-design.md'
created: '2026-09-15'
---

# Revue adversariale — ARCHITECTURE-SPINE

## Méthode

Pour chaque scénario ci-dessous, deux binômes fictifs (ou deux devs d'un même binôme, l'un côté `BattleShip.API`, l'autre côté `BattleShip.App`) respectent **à la lettre** les 8 AD et les conventions du spine, sans jamais violer une seule règle écrite — et produisent pourtant des implémentations incompatibles à l'intégration. Chaque scénario indique : les deux implémentations divergentes, pourquoi elles sont incompatibles, et quel AD resserrer ou quel AD manquant ajouter.

Verdict global : le spine est solide sur la séparation de couches et la frontière de visibilité (AD-1, AD-2, AD-8 sont bien fermés), mais **laisse plusieurs points de couture réseau sous-spécifiés** — précisément là où deux personnes qui ne se parlent pas doivent s'accorder sur un octet exact. Les trouvailles les plus graves concernent la boucle de jeu humain/IA sur gRPC et la forme exacte des DTO d'état, qui sont mentionnés mais jamais fixés au niveau champ-par-champ.

---

## F1 — [CRITICAL] Résolution du tour de l'IA : incluse dans `FireShot` ou mécanisme séparé non défini

**Implémentation A (côté API) :** `BattlefieldGrpcService.FireShot` traite le tir humain, puis **dans le même appel**, invoque immédiatement `IOpponentStrategy` pour jouer le tir de l'ordinateur, et retourne un `FireShotReply` contenant les deux résultats (`PlayerShotResult` + `OpponentShotResult`) en une seule réponse synchrone.

**Implémentation B (côté App/Blazor) :** Le dev Blazor lit AD-4 littéralement : « l'action de tir est exposée exclusivement via gRPC ». Il suppose donc que `FireShot` ne couvre que **le tir du joueur humain** (un tir = un appel), et qu'il doit ensuite faire un second appel (ou un polling `GET /games/{id}`) pour découvrir le coup de l'ordinateur, puisque rien dans le Structural Seed ne montre de méthode « tir de l'IA » distincte — il code donc `Play.razor` pour rafraîchir l'état via HTTP après chaque tir gRPC.

**Pourquoi incompatible :** Le client B attend un état d'IA mis à jour via HTTP après le tir gRPC ; le serveur A a déjà résolu et consommé le tour de l'IA à l'intérieur du `FireShotReply`, sans jamais l'exposer via `GET /games/{id}` de façon distincte (ou l'expose deux fois si le mapper le remet dans l'état global, créant une confusion sur « qui a tiré en dernier »). Selon comment le serveur choisit de raconter le tour de l'IA, le client peut soit ne jamais voir le coup adverse, soit le voir en double, soit désynchroniser l'historique (CAP-10) et les statistiques (CAP-11).

**AD à resserrer :** AD-4 doit préciser explicitement **le contrat du tour de jeu**, pas seulement quel transport porte l'action : est-ce que `FireShotReply` contient toujours *les deux* résultats (tir joueur + riposte IA) dans un seul aller-retour ? Si oui, le dire noir sur blanc et l'ajouter au Structural Seed (`FireShotReply { PlayerShot, OpponentShot, GameStatus }`). C'est le point le plus sous-spécifié de tout le spine car c'est le cœur de CAP-3 (boucle de jeu) exposé sur un transport dont le détail des messages est explicitement « Deferred ».

---

## F2 — [CRITICAL] Forme et nombre des DTO d'état retournés par `GET /games/{id}` non fixés

**Implémentation A :** `GameStateDto` est un objet unique combiné retourné par `GET /games/{id}`, contenant à la fois la grille propriétaire (`OwnGrid: CellDto[]`) et la vue adverse imbriquée (`OpponentGrid: OpponentGridDto`), plus le statut et l'historique. Une seule requête HTTP suffit à peupler `Play.razor`.

**Implémentation B :** En lisant le Structural Seed, qui liste `GameStateDto` et `OpponentGridDto` comme **deux fichiers de contrat distincts** dans `Contracts/`, le dev API conclut que ce sont deux réponses distinctes (ou deux endpoints, ou un DTO retourné par le HTTP et l'autre uniquement en sortie du mapper interne, jamais exposé tel quel). Le dev Blazor, lui, attend un seul appel `GET /games/{id}` qui retourne tout ; s'il ne reçoit que `GameStateDto` sans la vue adverse imbriquée, `Play.razor` ne peut pas afficher la grille adverse (touché/raté) après reconnection/rafraîchissement de page.

**Pourquoi incompatible :** AD-2 fixe qu'il existe deux *vues* distinctes produites par le mapper (règle de non-fuite), mais ne dit rien sur leur **véhicule de sérialisation** — un seul DTO composite vs deux DTO séparés vs deux endpoints. Les deux lectures sont également compatibles avec le texte de l'AD.

**AD à resserrer :** Étendre AD-2 (ou ajouter un AD-2bis) pour fixer explicitement la forme de la réponse de `GET /games/{id}` : un seul DTO racine avec deux sous-propriétés nommées, ou deux endpoints nommés. Ajouter cette forme au Structural Seed avec les champs exacts, au même niveau de détail que pour l'IGameStore.

---

## F3 — [HIGH] `PlayerId` : identifiant mentionné dans les conventions mais jamais défini nulle part ailleurs

**Constat :** La table *Consistency Conventions* dit « `GameId` et `PlayerId` en `Guid` » — mais **aucun AD, aucun DTO du Structural Seed (`CreateGameRequest`, `FireShotRequest`, `GameStateDto`...), aucun endpoint** ne mentionne `PlayerId`. Le jeu est strictement solo (1 humain vs 1 IA, pas de multijoueur — cf. Non-goals du SPEC), donc son rôle n'est pas évident.

**Implémentation A :** Le dev API introduit `PlayerId` comme jeton de session généré à la création de partie (`CreateGameRequest` répond avec un `PlayerId` que le client doit renvoyer dans chaque `FireShotRequest` pour prouver que c'est bien « son » tour), utile si plusieurs onglets/parties sont ouverts.

**Implémentation B :** Le dev Blazor ne voit `PlayerId` nulle part dans le Structural Seed concret (seulement dans une ligne de convention générale), considère que c'est un vestige, et n'envoie jamais ce champ — `FireShotRequest` côté client n'a que `GameId` + `Coordinate`.

**Pourquoi incompatible :** Si A valide côté serveur la présence/cohérence d'un `PlayerId` (potentiellement via `FireShotRequestValidator`, cf. AD-5), toutes les requêtes de B échouent en validation (rejet FluentValidation systématique) — un cas non couvert par CAP-7 qui suppose que rejet = entrée réellement invalide, pas un champ oublié par accord tacite.

**AD à resserrer :** Soit retirer `PlayerId` de la table des conventions (le jeu n'a qu'un joueur humain implicite, `GameId` suffit), soit lui donner un rôle explicite dans un AD (par ex. étendre AD-3 : « le store est aussi keyed/vérifié par `PlayerId` pour X raison ») et le faire apparaître dans les DTO du Structural Seed. Un identifiant cité une fois sans usage documenté est une invitation à deux interprétations incompatibles.

---

## F4 — [HIGH] Concurrence au niveau de l'agrégat `Game`, non couverte par AD-3 (qui ne protège que le dictionnaire)

**Implémentation A :** `InMemoryGameStore` utilise `ConcurrentDictionary<Guid, Game>` (conforme AD-3) et, en plus, encapsule chaque mutation dans `GameService` par un `lock` (ou `SemaphoreSlim`) par `Game`, pour sérialiser les appels concurrents à `ApplyShot` sur une même partie.

**Implémentation B :** Un autre dev lit AD-3 à la lettre — elle ne parle que de la structure de stockage (« `ConcurrentDictionary<Guid, Game>` enregistrée en Singleton ») — et ne met **aucun verrou** autour des mutations de l'objet `Game` lui-même, en toute bonne foi puisque « l'AD est respectée : un seul store, une seule instance, pas de copie parallèle ».

**Pourquoi incompatible :** `ConcurrentDictionary` protège uniquement les opérations `Add/Get/Remove` sur le dictionnaire, pas l'objet `Game` mutable qu'il contient. Sans verrou explicite, deux appels `FireShot` concurrents sur le même `GameId` (double-clic, retry réseau gRPC) peuvent tous deux lire « case non jouée », tous deux appliquer le tir, et soit compter deux fois le même tir dans les statistiques (violant le critère de succès CAP-1 : « non-comptabilisation d'un tir sur une case déjà jouée »), soit lever une exception de collection modifiée pendant qu'un `GET /games/{id}` concurrent énumère l'historique. Les deux implémentations sont « conformes AD-3 » ; une seule est correcte sous charge concurrente — et rien dans le texte ne permet de trancher à l'avance.

**AD à resserrer :** Étendre AD-3 pour couvrir explicitement la synchronisation **au niveau agrégat**, pas seulement au niveau de la table d'index : « toute mutation d'un `Game` via `IGameStore` doit être atomique vis-à-vis des appels concurrents sur le même `GameId` (verrou par partie, ou méthode `Update` de type `AddOrUpdate`/compare-and-swap exposée par `IGameStore`) ». Idem pour les lectures (`GameViewMapper` doit lire un état cohérent, pas une référence mutable en cours de modification).

---

## F5 — [HIGH] Mapping code gRPC ↔ cas d'erreur non pinné (« selon le cas » laisse le choix à chaque dev)

**Implémentation A :** Pour « tir sur une case déjà jouée », le dev API choisit `StatusCode.FailedPrecondition` (l'état du jeu ne permet pas cette action). Pour « tir après fin de partie », il choisit aussi `FailedPrecondition`. `InvalidArgument` est réservé aux erreurs de validation FluentValidation pures (coordonnée hors grille).

**Implémentation B :** Un autre dev, en implémentant en parallèle le client Blazor (`ShotGrpcClient`), suppose — en lisant CAP-6 (« coordonnée hors grille, coup déjà joué, partie terminée » listés ensemble comme la même famille d'erreurs attendues) — que les trois cas remontent en `InvalidArgument`, et code son `try/catch` sur `RpcException.StatusCode == StatusCode.InvalidArgument` pour afficher un message unique « coup invalide » pour les trois cas.

**Pourquoi incompatible :** Le client B ne capte jamais le cas « déjà joué » / « partie terminée » (il tombe dans un `catch` générique ou reste silencieux), alors que CAP-6 exige que cette erreur soit « démontrable ». La convention du spine (« `InvalidArgument`/`NotFound`/`FailedPrecondition` selon le cas ») liste les trois codes possibles sans dire lequel va avec quel cas précis — chaque dev peut légitimement choisir une association différente.

**AD à resserrer :** Étendre AD-5 (ou la ligne « Forme des erreurs » des Consistency Conventions, en la promouvant en AD si elle porte une règle de compatibilité contractuelle) avec une table explicite : coordonnée hors grille → `InvalidArgument` ; `GameId` inconnu → `NotFound` ; case déjà jouée → `FailedPrecondition` ; tir après fin de partie → `FailedPrecondition`. Sans ce tableau, CAP-6 et CAP-7 ne sont testables de façon interopérable qu'après coordination hors-document entre les deux devs.

---

## F6 — [MEDIUM] Type validé par FluentValidation côté gRPC : contrat partagé `Models` ou classe protobuf générée ?

**Implémentation A :** `FireShotRequestValidator : AbstractValidator<FireShotRequest>` où `FireShotRequest` est le type défini dans `BattleShip.Models/Contracts/FireShotRequest.cs` (partagé HTTP/gRPC). Le service gRPC mappe d'abord le message protobuf généré (`FireShotRequestProto`) vers ce type avant de valider.

**Implémentation B :** Un autre dev, pressé, écrit `FireShotRequestValidator : AbstractValidator<FireShotRequestProto>` directement sur la classe générée par `Grpc.Tools` à partir du `.proto`, pour éviter une étape de mapping — toujours conforme à AD-5 (« les validateurs vivent dans `BattleShip.API` et s'exécutent avant tout appel au domaine, sur HTTP comme sur gRPC »), qui ne précise pas contre **quel type** ils doivent être écrits.

**Pourquoi incompatible :** Cela crée deux définitions divergentes de « ce qu'est une requête de tir valide » (deux classes, potentiellement deux jeux de règles qui dérivent avec le temps), et casse la promesse implicite du Structural Seed que `BattleShip.Models/Contracts` est la source unique des DTO d'échange. Si un troisième dev veut réutiliser `FireShotRequestValidator` dans un test unitaire de `BattleShip.Models`-only (AD-7, dossier `Unit/`), il ne peut pas si le validateur cible le type protobuf (qui vit dans `BattleShip.API`, généré depuis `.proto`), cassant l'hypothèse de AD-7 sur ce qui est testable en isolation.

**AD à resserrer :** Ajouter à AD-5 une phrase fixant que les validateurs ciblent toujours les types de `BattleShip.Models/Contracts`, et que le mapping proto → contrat partagé se fait *avant* validation, jamais l'inverse.

---

## F7 — [MEDIUM] Propriété et synchronisation du fichier `.proto` : AD-8 interdit le `ProjectReference`, mais rien ne fixe où vit la source de vérité

**Implémentation A :** Le `.proto` canonique vit dans `BattleShip.API/Grpc/battlefield.proto` (conforme au Structural Seed). Le dev Blazor, pour générer le client gRPC-Web sans `ProjectReference` vers `API` (interdit par AD-8), **copie manuellement** le fichier `.proto` dans `BattleShip.App/Protos/battlefield.proto` et le référence dans son `.csproj` via `<Protobuf Include="Protos/battlefield.proto" GrpcServices="Client" />`.

**Implémentation B :** Le dev API modifie plus tard un champ de `FireShotReply` (par ex. pour F1, ajoute `OpponentShotResult`) dans sa copie du `.proto`, mais personne n'a de processus pour propager le changement vers la copie du côté `App` — les deux `.proto` divergent silencieusement, sans erreur de compilation (protobuf est tolérant aux champs manquants par design).

**Pourquoi incompatible :** Les deux devs respectent AD-8 à la lettre (aucun `ProjectReference` App→API), mais AD-8 ne dit rien sur **comment le `.proto` est partagé sans référence de projet** (fichier lié via `<Link>` MSBuild vers un chemin relatif commun ? script de synchronisation ? dossier partagé hors des deux projets ?). Le Structural Seed ne liste `battlefield.proto` que sous `BattleShip.API/Grpc/`, jamais sous `BattleShip.App`, ce qui laisse deviner la mécanique de partage.

**AD à resserrer :** Étendre AD-8 avec la mécanique concrète de partage du `.proto` (par ex. : fichier physique unique référencé par les deux `.csproj` via un chemin relatif commun — `<Protobuf Include="../BattleShip.API/Grpc/battlefield.proto" />` — sans que cela constitue un `ProjectReference`), et l'ajouter au Structural Seed.

---

## F8 — [MEDIUM] Représentation de `Coordinate` sur le fil gRPC non explicitement pinnée par un AD (seulement une convention)

**Implémentation A :** Le `.proto` définit `message Coordinate { int32 row = 1; int32 col = 2; }`, conforme à la convention « `record Coordinate(int Row, int Col)` 0-indexé côté domaine/API ».

**Implémentation B :** Le dev Blazor, en lisant que « la conversion en notation lettre+chiffre (A1..J10) [est] uniquement à l'affichage », comprend cela comme : le domaine/API travaille en interne avec des lettres+chiffres et ne les convertit qu'à l'affichage terminal/log — et code son client pour envoyer une `string Cell = "A1"` dans le message gRPC, avec conversion en `Row/Col` uniquement côté serveur si besoin.

**Pourquoi incompatible :** Le champ n'apparaît dans aucun AD normatif — seulement dans la table *Consistency Conventions*, qui n'a pas le même statut contraignant que les AD numérotées (le document distingue explicitement les deux catégories). Une divergence de type sur le message `Coordinate` casse la compilation du client généré dès que les deux `.proto` (cf. F7) ne s'accordent pas sur le schéma.

**AD à resserrer :** Faire remonter la définition du type `Coordinate` (0-indexé, deux entiers) dans le Structural Seed du `.proto` lui-même, ou l'ajouter comme clause normative d'AD-4/AD-6, pas seulement comme ligne de convention informelle.

---

## F9 — [LOW/MEDIUM] Calcul de la durée/statistiques (CAP-11) : aucun champ DTO ne porte l'horodatage, risque de double calcul divergent

**Implémentation A :** `Game` porte `CreatedAt`/`EndedAt` (domaine, AD-1), et un service de statistiques dans `BattleShip.Models` calcule la durée à la fin de partie ; ce résultat est exposé dans un DTO de fin de partie renvoyé par le serveur (ex. étend `GameStateDto` avec un objet `Stats`).

**Implémentation B :** Aucun DTO du Structural Seed (`GameStateDto`, `OpponentGridDto`, `ShotResultDto`) ne mentionne explicitement de champ temporel ou de statistiques. Le dev Blazor, ne voyant pas de champ serveur pour la durée, calcule lui-même la durée côté client à partir de l'horodatage de première réponse HTTP `POST /games` jusqu'à l'horodatage de détection de fin de partie dans `Play.razor`.

**Pourquoi incompatible :** Les deux valeurs de durée divergent (latence réseau, décalage horloge client/serveur, et surtout si le joueur laisse l'onglet inactif — le timer client dérive du vrai temps de jeu serveur). CAP-11 exige que « les valeurs affichées correspondent aux données de la partie jouée, vérifié par un test unitaire sur le calcul des statistiques » — un test unitaire ne peut porter que sur un calcul serveur déterministe, pas sur un chronométrage client, donc l'implémentation B ne peut structurellement pas satisfaire le critère de succès de CAP-11 tel qu'écrit, alors qu'elle ne viole aucun AD numéroté.

**AD à resserrer :** Ajouter au Structural Seed les champs exacts du DTO de fin de partie (`CreatedAt`, `EndedAt` ou `DurationSeconds`, `ShotsCount`, `HitsCount`, `SuccessRate`, par camp), et fixer dans un AD (extension d'AD-1 ou nouvel AD) que ce calcul est **exclusivement** server-side, jamais recalculé côté Blazor.

---

## Synthèse des AD à amender

| AD | Amendement recommandé |
| --- | --- |
| AD-2 | Fixer la forme exacte de la réponse `GET /games/{id}` (DTO composite unique vs multiple) |
| AD-3 | Étendre la garantie de concurrence au niveau de l'agrégat `Game`, pas seulement du dictionnaire d'index |
| AD-4 | Fixer si `FireShotReply` porte le tir humain ET la riposte IA dans le même aller-retour |
| AD-5 | Fixer le type cible des validateurs FluentValidation côté gRPC (contrat `Models`, pas le type protobuf généré) et une table code-erreur ↔ cas métier |
| AD-8 | Fixer le mécanisme de partage physique du `.proto` sans `ProjectReference` |
| Nouveau | Fixer le rôle (ou l'absence de rôle) de `PlayerId`, actuellement un identifiant orphelin dans les conventions |
| Nouveau | Fixer les champs exacts du DTO de fin de partie portant les statistiques (CAP-11), calculés uniquement côté serveur |
