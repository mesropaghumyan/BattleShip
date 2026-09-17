# ADR 0004 : Répartition des transports HTTP/gRPC et contrat du tour de jeu

## Statut et date
Accepté — 2026-09-15.

## Contexte
Le socle impose à la fois une API Minimal API et un échange gRPC fonctionnel. Sans répartition claire, la logique et la validation du tir risqueraient d'être dupliquées sur deux transports parallèles, avec une ambiguïté sur comment et quand l'ordinateur joue son tour.

## Options envisagées
- Exposer le tir sur HTTP et sur gRPC (les deux transports acceptent un tir) : rejeté implicitement, duplique la logique/validation du tir sur deux transports.
- Deux appels séparés pour le tir joueur puis la riposte ordinateur : alternative écartée implicitement par le choix d'une résolution en un seul appel.
- Création/consultation en HTTP, tir exclusivement en gRPC, résolu en un seul appel round-trip (tir joueur + riposte ordinateur le cas échéant) : retenue.

## Décision
Création et consultation de partie exposées en HTTP Minimal API (`POST /games`, `GET /games/{id}`). L'action de tir est exposée exclusivement via le service gRPC (`BattlefieldService.FireShot`), consommé en gRPC-Web depuis Blazor. `FireShot` résout en une seule requête le tir du joueur puis, si la partie n'est pas terminée, la riposte immédiate de l'ordinateur : `ShotTurnReply` porte les deux résultats + le statut de partie. Aucun endpoint HTTP n'accepte de tir. Les deux transports délèguent uniquement à `GameService`.

## Conséquences
- Un seul point de validation et de logique pour l'action de tir (gRPC uniquement).
- Le client n'a qu'un aller-retour réseau par tour de jeu, ordinateur inclus.
- La résolution `GameStatus`/`Winner` → `GameOutcome` a néanmoins été réimplémentée indépendamment côté HTTP (`GameViewMapper.ToOutcome`) et côté gRPC (`BattlefieldGrpcService.ToGameOutcome`) — duplication de règle constatée en rétrospective de l'Epic 1, non corrigée à ce jour (voir `deferred-work.md` / actions proposées de la rétro Epic 1).

## Vérification et réexamen
71 tests au vert dont un round-trip gRPC complet en canal in-process (Story 1.5). Comportement bout-en-bout confirmé par un utilisateur réel dans un navigateur (Story 1.6, Epic 1). À réexaminer si une évolution de règle (ex. égalité) doit un jour modifier `GameOutcome` : corriger la duplication avant, pas après, pour éviter une divergence entre HTTP et gRPC.

## Références
`_bmad-output/planning-artifacts/architecture/architecture-BattleShip-2026-09-15/ARCHITECTURE-SPINE.md#AD-4`
