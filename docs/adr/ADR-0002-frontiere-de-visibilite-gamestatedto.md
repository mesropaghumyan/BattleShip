# ADR 0002 : Frontière de visibilité et véhicule des deux vues (`GameStateDto`)

## Statut et date
Accepté — 2026-09-15.

## Contexte
Les endpoints HTTP et le service gRPC doivent exposer l'état de partie sans jamais faire fuiter les positions de navires adverses non découvertes. Sans mapping dédié ni contrat fixé, deux implémentations pourraient chacune inventer leur propre forme de réponse, avec un risque de fuite d'information ou d'incohérence entre elles.

## Options envisagées
- Sérialiser l'entité domaine `Grid` brute directement dans la réponse HTTP/gRPC : rejeté implicitement, fait fuiter les navires non découverts.
- Deux endpoints séparés (un pour sa propre grille, un pour la grille adverse) : alternative écartée implicitement par le choix d'un DTO composite unique.
- Un DTO composite unique (`GameStateDto`) portant `OwnGrid` + `OpponentGrid`, produit par un mapping dédié (`GameViewMapper`) : retenue.

## Décision
Les endpoints HTTP et le service gRPC ne sérialisent jamais l'entité domaine `Grid` brute. `GameViewMapper` est le seul point de conversion Domaine → DTO. `GET /games/{id}` (HTTP) renvoie un `GameStateDto` composite unique portant `OwnGrid` (navires du joueur) + `OpponentGrid` (touché/raté/coulé uniquement) — pas deux endpoints séparés. La réponse gRPC `FireShot` (`ShotTurnReply`) ne porte pas les grilles complètes, seulement les résultats du tour ; le client rappelle `GET /games/{id}` s'il a besoin de l'état complet.

## Conséquences
- Un seul point de mapping à maintenir et à tester pour la frontière de visibilité.
- Le client doit faire un aller-retour HTTP supplémentaire après un tir gRPC s'il veut l'état complet des grilles (compromis accepté, cf. AD-4).
- Permet d'ajouter des champs composites (historique, statistiques — Epic 2) sans changer la forme de contrat.

## Vérification et réexamen
Un test vérifie qu'aucune coordonnée de navire non coulé n'apparaît dans `OpponentGrid` (Story 1.4). À réexaminer si un besoin de flux temps réel (push serveur→client) apparaissait, ce qui n'est pas dans le périmètre du cours.

## Références
`_bmad-output/planning-artifacts/architecture/architecture-BattleShip-2026-09-15/ARCHITECTURE-SPINE.md#AD-2`
