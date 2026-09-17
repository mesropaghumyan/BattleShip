# ADR 0005 : Emplacement de la validation

## Statut et date
Accepté — 2026-09-15.

## Contexte
Le socle impose FluentValidation sur les entrées serveur. Sans emplacement fixé, la validation risquerait d'être dispersée inline dans les lambdas d'endpoints, ou le domaine ferait confiance à une entrée non vérifiée si jamais appelé directement (par exemple en test).

## Options envisagées
- Validation inline dans chaque lambda d'endpoint HTTP/gRPC : rejeté implicitement, dispersion et duplication à chaque nouveau point d'entrée.
- Validation FluentValidation uniquement, le domaine faisant confiance à l'entrée déjà validée : rejeté implicitement, rendrait le domaine non sûr si appelé directement en test.
- Validateurs FluentValidation dédiés dans `BattleShip.API`, exécutés avant tout appel à `GameService`, complétés par les propres gardes du domaine (exceptions sur invariants violés) : retenue.

## Décision
Les validateurs FluentValidation (`CreateGameRequestValidator`, `FireShotRequestValidator`) vivent dans `BattleShip.API` et s'exécutent avant tout appel à `GameService`, sur HTTP comme sur gRPC. Le domaine garde ses propres gardes (exceptions sur invariants violés), indépendamment de FluentValidation, pour rester sûr même appelé directement en test.

## Conséquences
- Double ligne de défense : FluentValidation à la frontière réseau, gardes du domaine en profondeur.
- Les tests unitaires du domaine (`BattleShip.Tests/Unit`) peuvent construire des scénarios invalides sans dépendre de FluentValidation.
- Toute nouvelle entrée réseau doit avoir son propre validateur FluentValidation ; l'omission serait un écart à cet ADR.

## Vérification et réexamen
Story 1.4 (`CreateGameRequestValidator`) et Story 1.5 (`FireShotRequestValidator`) : bornes de validation testées, y compris cas limites hors-grille. À réexaminer si une story future ajoute une nouvelle entrée réseau sans validateur dédié.

## Références
`_bmad-output/planning-artifacts/architecture/architecture-BattleShip-2026-09-15/ARCHITECTURE-SPINE.md#AD-5`
