# ADR 0010 : Identité des camps (`enum Side`)

## Statut et date
Accepté — 2026-09-15.

## Contexte
Le socle ne demande pas de multijoueur (non-goal explicite). Une notion d'identité joueur individuelle (par exemple un `Guid` de joueur généré côté client ou serveur) ne serait pas justifiée par ce périmètre et risquerait d'être implémentée de façon incompatible selon l'endroit (client vs serveur).

## Options envisagées
- Une identité joueur individuelle (`PlayerId`, `Guid` généré côté client ou serveur) : écartée en revue d'architecture (« *PlayerId* orphelin remplacé par `enum Side` », voir `PROMPTS.md` entrée 2) — non justifiée par le non-goal multijoueur, risque d'implémentation incompatible.
- `enum Side { Human, Computer }` pour identifier l'auteur d'un tir et le camp dans les statistiques, `GameId` (`Guid`) restant le seul identifiant du domaine : retenue.

## Décision
`enum Side { Human, Computer }` identifie l'auteur d'un tir (`Shot.Side`, historique) et le camp dans les statistiques. Pas d'identité joueur individuelle. `GameId` (`Guid`) reste le seul identifiant du domaine.

## Conséquences
- Simplifie le modèle : deux camps fixes, pas de gestion de session joueur ni d'authentification.
- L'historique (Story 2.3) et les statistiques (Story 2.4) s'appuient directement sur `Side` sans traduction supplémentaire.
- Si un besoin multijoueur émergeait un jour (hors périmètre actuel), cet ADR devrait être remplacé.

## Vérification et réexamen
Utilisé sans friction dans `Shot.Side` (Story 2.3) et `GameStatisticsCalculator` par `Side` (Story 2.4) — 101 tests au vert à la fin de l'Epic 2. À réexaminer uniquement si le non-goal « pas de multijoueur » était renégocié.

## Références
`_bmad-output/planning-artifacts/architecture/architecture-BattleShip-2026-09-15/ARCHITECTURE-SPINE.md#AD-10`
