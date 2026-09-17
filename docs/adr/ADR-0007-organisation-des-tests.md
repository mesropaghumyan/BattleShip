# ADR 0007 : Organisation des tests

## Statut et date
Accepté — 2026-09-15.

## Contexte
Le socle impose des tests métier et des tests d'intégration, dans les 4 projets déjà imposés (aucun 5ᵉ projet de test n'est prévu). Sans convention, il y aurait une ambiguïté récurrente sur où placer un nouveau test, ou une tentation de créer un projet de test supplémentaire non prévu.

## Options envisagées
- Créer un 5ᵉ projet de test dédié (par exemple pour l'App Blazor) : rejeté implicitement, non prévu par le socle imposé.
- Un unique projet `BattleShip.Tests` sans sous-organisation : alternative écartée implicitement, source d'ambiguïté sur le placement d'un nouveau test.
- `BattleShip.Tests` unique, structuré en deux dossiers `Unit/` (domaine pur) et `Integration/` (`WebApplicationFactory` contre l'API) : retenue.

## Décision
`BattleShip.Tests` reste un projet unique avec deux dossiers : `Unit/` (règles pures du domaine, ne référence que `BattleShip.Models`) et `Integration/` (`WebApplicationFactory<Program>` contre `BattleShip.API`, HTTP et gRPC in-process).

## Conséquences
- Aucune couverture automatisée de la couche Blazor (aucun test de composant, pas d'outillage bUnit dans le dépôt) : les bugs de rendu ou de configuration réseau côté client ne sont détectés que par la vérification manuelle — coût réel constaté (bug de profil réseau post-clôture Epic 1, commit `9525262`, détecté seulement à l'usage réel malgré 71 tests automatisés au vert).
- 101 tests au vert à la fin de l'Epic 2, tous répartis sans ambiguïté entre `Unit/` et `Integration/`.
- Toute story qui voudrait tester l'App Blazor devrait soit accepter cette limite, soit renégocier cet ADR.

## Vérification et réexamen
`dotnet test` exécuté à chaque story, aucune dérive de placement observée en rétrospective Epic 1/2. À réexaminer si le projet devait un jour couvrir automatiquement la couche Blazor (bUnit ou équivalent) — non fait à ce jour, limite acceptée et documentée.

## Références
`_bmad-output/planning-artifacts/architecture/architecture-BattleShip-2026-09-15/ARCHITECTURE-SPINE.md#AD-7`
