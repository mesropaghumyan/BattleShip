# ADR 0011 : Mapping des codes d'erreur gRPC

## Statut et date
Accepté — 2026-09-15.

## Contexte
Le service gRPC `FireShot` doit signaler plusieurs cas d'erreur distincts (coordonnée invalide, case déjà jouée, partie inconnue, tir après fin de partie). Sans mapping fixé à l'avance, le client Blazor pourrait interpréter différemment la même erreur selon qui a implémenté le service gRPC, au fil des stories.

## Options envisagées
- Laisser chaque implémentation choisir librement son code d'erreur gRPC au cas par cas : rejeté implicitement, cause l'interprétation incohérente que la règle interdit.
- Fixer un mapping unique par nature d'erreur avant l'implémentation : retenue.

## Décision
Coordonnée hors grille → `InvalidArgument`. Case déjà jouée → `FailedPrecondition`. `GameId` inconnu → `NotFound`. Tir après fin de partie → `FailedPrecondition`.

## Conséquences
- Le client Blazor peut distinguer les cas d'erreur de façon fiable et cohérente dans le temps.
- Le service gRPC `FireShot` n'a cependant pas de filet de sécurité générique en miroir du pipeline HTTP (`AddProblemDetails`) : seules 3 exceptions domaine précises sont attrapées et mappées ; toute autre exception non anticipée tombe en `Internal`/`Unknown` sans détail structuré (signalé en rétrospective Epic 1, action proposée non réalisée).

## Vérification et réexamen
Testé par cas en Story 1.5 (bornes hors-grille, rejeu de case, partie inconnue, tir après fin de partie) — 71 tests au vert. À réexaminer si un filet `catch (Exception)` générique est ajouté au service gRPC (action proposée en rétrospective Epic 1, non encore réalisée).

## Références
`_bmad-output/planning-artifacts/architecture/architecture-BattleShip-2026-09-15/ARCHITECTURE-SPINE.md#AD-11`
