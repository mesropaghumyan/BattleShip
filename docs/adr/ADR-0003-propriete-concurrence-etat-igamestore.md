# ADR 0003 : Propriétaire, accès et concurrence de l'état (`IGameStore`)

## Statut et date
Accepté — 2026-09-15.

## Contexte
L'état de chaque partie doit rester cohérent sous appels concurrents (HTTP et gRPC pouvant arriver en parallèle) et n'avoir qu'un seul point d'accès autorisé, sous peine de collections d'état divergentes, de double application d'un tir ou de lecture partielle (torn read).

## Options envisagées
- Deux collections d'état distinctes (une par transport) : rejeté implicitement, cause la divergence que la règle interdit.
- Endpoints et service gRPC accédant chacun directement à `IGameStore` avec Get/Set brut : rejeté implicitement, ambiguïté sur qui synchronise l'accès et risque de double application d'un tir.
- Une unique abstraction `IGameStore` en Singleton, avec mutation atomique par partie, et un seul appelant autorisé (`GameService`) : retenue.

## Décision
Une unique abstraction `IGameStore` (implémentation en mémoire, `ConcurrentDictionary<Guid, Game>`) est enregistrée en Singleton. `IGameStore` n'expose pas de Get/Set brut mais une méthode atomique `Mutate(Guid gameId, Action<Game> mutation)` qui synchronise l'accès à l'agrégat (verrou par partie). `GameService` (couche application) est le seul appelant autorisé de `IGameStore` ; les Endpoints HTTP et le service gRPC n'accèdent jamais directement à `IGameStore`, ils passent tous deux par `GameService`.

## Conséquences
- Toute mutation de `Game`/`Grid` passe par un point unique, testable et auditable.
- Le code de test peut légitimement accéder à `IGameStore` directement pour construire des états déterministes, sans violer l'invariant qui porte sur le code de production (clarifié en rétrospective de l'Epic 1).
- `IGameStore.WithGame<TResult> : class` force des contournements pour les appelants voulant une valeur (type valeur), constatés dans les tests — reporté, voir `REVUE-IA.md`.
- `InMemoryGameStore` (`_games` et `_locks`) croît sans borne — accepté pour une démonstration courte, tracé dans `deferred-work.md`.

## Vérification et réexamen
La correction du verrou n'a pas été validée par un test de concurrence (jugé disproportionné pour ce projet, `deferred-work.md`), mais par relecture directe du code du `lock` dans `InMemoryGameStore`. À réexaminer si le store doit un jour persister ou tourner en environnement multi-instance.

## Références
`_bmad-output/planning-artifacts/architecture/architecture-BattleShip-2026-09-15/ARCHITECTURE-SPINE.md#AD-3`
