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
- source_spec: `_bmad-output/implementation-artifacts/spec-1-6-interface-blazor-créer-une-partie-et-jouer.md`
  summary: GameHttpClient (BattleShip.App) n'a aucun test de régression sur ses options JSON (camelCase) ; le bug corrigé cette story ne serait pas rattrapé par dotnet test.
  evidence: Signalé pré-vérifié par Verification Gap. Le correctif propre demande une ProjectReference Tests→App non prévue par AD-7 ; à revoir si un projet de test App est introduit plus tard.
- source_spec: `_bmad-output/implementation-artifacts/spec-1-6-interface-blazor-créer-une-partie-et-jouer.md`
  summary: En-têtes CORS exposés (Grpc-Status/Message/Encoding) non couverts par un test cross-origin automatisé.
  evidence: Signalé par Verification Gap. Vérifié manuellement (curl préflight) cette story ; suffisant au regard du scope AD-7 (pas de test Blazor).
- source_spec: `_bmad-output/implementation-artifacts/spec-1-6-interface-blazor-créer-une-partie-et-jouer.md`
  summary: Garde "case déjà jouée" et verrouillage de fin de partie sur BattleGrid non couverts par un test de composant (pas d'outillage bUnit dans le dépôt).
  evidence: Signalé par Verification Gap. Deux AC de cette story en dépendent ; vérifié manuellement (Playwright scratch + revue de code) mais non pinné par un test répétable.
- source_spec: `_bmad-output/implementation-artifacts/spec-1-6-interface-blazor-créer-une-partie-et-jouer.md`
  summary: URLs de l'API et origines CORS codées en dur dans Program.cs (App et API), pas de couche de configuration.
  evidence: Signalé par Blind Hunter. Hors scope (déploiement = non-goal explicite du spec) ; pertinent seulement si le projet doit tourner ailleurs qu'en local.
- source_spec: `_bmad-output/implementation-artifacts/spec-1-6-interface-blazor-créer-une-partie-et-jouer.md`
  summary: Les cellules cliquables de BattleGrid n'ont aucune accessibilité clavier (tabindex/role/onkeydown).
  evidence: Signalé par Blind Hunter. Extension optionnelle listée en backlog (accessibilité, diapo 60 du cours), pas un AC du socle.
- source_spec: `_bmad-output/implementation-artifacts/spec-1-6-interface-blazor-créer-une-partie-et-jouer.md`
  summary: Fenêtre de course théorique si un second clic passe avant que le paramètre Interactive de BattleGrid ne se mette à jour après un premier tir.
  evidence: Signalé par Edge Case Hunter (maybe-false). Dépend du scheduling interne du renderer Blazor WASM ; nécessiterait un test de stress navigateur réel pour trancher.
- source_spec: `_bmad-output/implementation-artifacts/spec-3-1-mise-en-place-du-processus-de-traçabilité-ia.md`
  summary: CLAUDE.md (mode Caveman, réponses télégraphiques, focus code C# uniquement) entre en tension avec l'exigence de l'Epic 3 de produire de la prose française argumentée (PROMPTS.md, ADR, REVUE-IA.md, README.md).
  evidence: Signalé par Blind Hunter (review Story 3.1). Réel : un assistant suivant CLAUDE.md à la lettre résisterait à produire les livrables mêmes que cet epic construit. Correction hors périmètre de cette story (édition de CLAUDE.md exclue des routes patch/HALT) ; à trancher avec l'utilisateur avant ou pendant la Story 3.2/3.3.
- source_spec: `_bmad-output/implementation-artifacts/spec-3-1-mise-en-place-du-processus-de-traçabilité-ia.md`
  summary: skills-lock.json pin les skills caveman-* par hash de contenu et nom de dépôt GitHub nu (sans tag/SHA), sans instructions d'installation/rafraîchissement documentées nulle part dans le dépôt.
  evidence: Signalé par Blind Hunter (review Story 3.1). Un tiers clonant le dépôt n'a aucun moyen documenté d'obtenir ces skills, ce qui contredit l'exigence de la Story 3.3 (« un tiers sans connaissance préalable » doit pouvoir travailler à partir du seul dépôt). À traiter dans le README de remise (Story 3.3) ou à retirer si non essentiel.
