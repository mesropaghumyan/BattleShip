- source_spec: `_bmad-output/implementation-artifacts/spec-1-3-adversaire-ordinateur-stratégie-aléatoire-facile.md`
  summary: IOpponentStrategy.ChooseShot(Grid) expose toute la grille (dont les positions de navires réelles), pas seulement l'historique des tirs — un futur HardOpponentStrategy (Story 2.1) pourrait "tricher" en lisant Ships au lieu de raisonner uniquement sur ShotsPlayed.
  evidence: Signalé en revue Blind Hunter de la Story 1.3. Pas corrigé maintenant car la vraie forme de l'interface dépend des besoins réels de la Story 2.1 (hunt & target a aussi besoin de savoir quels tirs étaient des touches, pas seulement lesquelles cases sont jouées — Grid.ShotsPlayed ne distingue pas hit/miss aujourd'hui). À trancher explicitement à l'ouverture de la Story 2.1 plutôt que de deviner la forme maintenant.
- source_spec: `_bmad-output/implementation-artifacts/spec-1-4-api-créer-une-partie-et-consulter-son-état.md`
  summary: InMemoryGameStore (et son dictionnaire _locks) n'a aucune éviction/TTL — croissance non bornée pour la durée du process.
  evidence: Signalé par Blind Hunter et Verification Gap (review Story 1.4). Non spécifié par AD-3 (état en mémoire, mais pas de politique de cycle de vie). Pas un problème pour une démo courte ; à revoir si le projet doit tourner longtemps ou avec beaucoup de parties.
- source_spec: `_bmad-output/implementation-artifacts/spec-1-4-api-créer-une-partie-et-consulter-son-état.md`
  summary: Aucun test de concurrence réel n'exerce le verrou par partie de IGameStore (lecture + écriture simultanées sur la même partie).
  evidence: Signalé par Blind Hunter. Écrire un test de concurrence fiable (non flaky) demande un effort disproportionné pour ce projet ; la correction du verrou repose sur une relecture du `lock` plutôt que sur un test de stress.
- source_spec: `_bmad-output/implementation-artifacts/spec-1-4-api-créer-une-partie-et-consulter-son-état.md`
  summary: Métadonnées OpenAPI absentes sur POST /games et GET /games/{id} (WithName, Produces<T>, ProducesProblem).
  evidence: Signalé par Blind Hunter. Purement documentaire, aucun AC ni test ne l'exige ; à revoir si la Story 1.6 ou la remise a besoin d'une doc OpenAPI plus riche.
