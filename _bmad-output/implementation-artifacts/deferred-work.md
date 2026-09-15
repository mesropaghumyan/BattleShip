- source_spec: `_bmad-output/implementation-artifacts/spec-1-3-adversaire-ordinateur-stratégie-aléatoire-facile.md`
  summary: [RÉSOLU dans Story 2.1] IOpponentStrategy.ChooseShot(Grid) expose toute la grille (dont les positions de navires réelles), pas seulement l'historique des tirs.
  evidence: Résolu dans la Story 2.1 via l'ajout de `Grid.GetShotOutcome(Coordinate)` et `Grid.UnsunkHits`. `HardOpponentStrategy` ne lit jamais `Grid.Ships` et raisonne exclusivement sur les tirs connus et les touches sur navires non coulés, éliminant tout risque de triche ou de fuite d'information.
- source_spec: `_bmad-output/implementation-artifacts/spec-1-4-api-créer-une-partie-et-consulter-son-état.md`
  summary: InMemoryGameStore (et son dictionnaire _locks) n'a aucune éviction/TTL — croissance non bornée pour la durée du process.
  evidence: Signalé par Blind Hunter et Verification Gap (review Story 1.4). Non spécifié par AD-3 (état en mémoire, mais pas de politique de cycle de vie). Pas un problème pour une démo courte ; à revoir si le projet doit tourner longtemps ou avec beaucoup de parties.
- source_spec: `_bmad-output/implementation-artifacts/spec-1-4-api-créer-une-partie-et-consulter-son-état.md`
  summary: Aucun test de concurrence réel n'exerce le verrou par partie de IGameStore (lecture + écriture simultanées sur la même partie).
  evidence: Signalé par Blind Hunter. Écrire un test de concurrence fiable (non flaky) demande un effort disproportionné pour ce projet ; la correction du verrou repose sur une relecture du `lock` plutôt que sur un test de stress.
- source_spec: `_bmad-output/implementation-artifacts/spec-1-4-api-créer-une-partie-et-consulter-son-état.md`
  summary: Métadonnées OpenAPI absentes sur POST /games et GET /games/{id} (WithName, Produces<T>, ProducesProblem).
  evidence: Signalé par Blind Hunter. Purement documentaire, aucun AC ni test ne l'exige ; à revoir si la Story 1.6 ou la remise a besoin d'une doc OpenAPI plus riche.
- source_spec: `_bmad-output/implementation-artifacts/spec-1-5-api-jouer-un-coup-via-grpc.md`
  summary: Incohérence dans battlefield.proto — FireShotRequest utilise row/col bruts, ShotResult les enveloppe dans un message Coordinate.
  evidence: Signalé par Blind Hunter. Corriger casserait le contrat déjà implémenté/testé (validator, service, 71 tests). À revoir si des auteurs clients (Story 1.6) trouvent l'incohérence gênante en pratique.
- source_spec: `_bmad-output/implementation-artifacts/spec-1-5-api-jouer-un-coup-via-grpc.md`
  summary: <NoWarn>CS0436</NoWarn> sur BattleShip.Tests.csproj masque toute future collision de types sans rapport avec la génération protobuf.
  evidence: Signalé par Blind Hunter. Compromis déjà documenté dans le fichier (la génération double proto server+client est la cause attendue) ; pas de meilleure option simple identifiée pour l'instant.
