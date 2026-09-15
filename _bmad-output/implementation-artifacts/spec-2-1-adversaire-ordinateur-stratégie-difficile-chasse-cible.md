---
title: 'Adversaire ordinateur — stratégie difficile (chasse/cible)'
type: 'feature'
created: '2026-09-15'
status: 'done'
route: 'oneshot'
review_loop_iteration: 0
context: []
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** Le jeu ne propose qu'un adversaire aléatoire simple (`EasyOpponentStrategy`) ; pour un joueur expérimenté, les parties manquent de challenge tactique et ne tirent pas parti des indices révélés par les tirs réussis.

**Approach:** Implémenter `HardOpponentStrategy` (`BattleShip.Models/Domain/Opponent/HardOpponentStrategy.cs`) implémentant `IOpponentStrategy` selon l'algorithme chasse/cible (hunt & target) :
1. **Phase de recherche (hunt)** : tire sur les cases d'une parité damier `(Row + Col) % 2 == 0` parmi les cases non encore jouées.
2. **Phase de ciblage (target)** : lorsqu'un tir touché non coulé est présent, teste l'une des 4 cases adjacentes (haut, bas, gauche, droite) valides et non encore jouées.
3. **Phase d'alignement (line)** : dès que deux tirs touchés ou plus sont alignés (même ligne ou colonne) sur un navire non coulé, poursuit dans cet axe (extrémités ou trous) jusqu'à couler le navire ou essuyer un échec, puis retourne en phase de recherche.

</frozen-after-approval>

## Implementation Notes

- Nouveau fichier : `BattleShip.Models/Domain/Opponent/HardOpponentStrategy.cs`.
- Évolution de `Grid.cs` : ajout de `GetShotOutcome(Coordinate)` et de la propriété `UnsunkHits`. Cela permet à `HardOpponentStrategy` de raisonner légitimement sur les tirs connus et les navires touchés non coulés, sans jamais lire ou fuiter les coordonnées secrètes des navires non touchés (`targetGrid.Ships`), résolvant le point différé issu de la revue de la Story 1.3.
- `HardOpponentStrategy` accepte un paramètre `Random? random = null` en constructeur pour garantir un comportement déterministe et reproductible sous test unitaire.
- Nouveaux tests ajoutés dans `BattleShip.Tests/Unit/HardOpponentStrategyTests.cs` (16 tests) et enrichissement de `BattleShip.Tests/Unit/GridTests.cs` (5 tests) :
  - Typage `IOpponentStrategy`.
  - Phase 1 (recherche) : parité damier respectée, repli sur cases restantes si tout le damier est joué.
  - Phase 2 (ciblage) : sélection des cases adjacentes non jouées, respect des limites de grille (coins).
  - Phase 3 (alignement) : poursuite axe horizontal, axe vertical, trou intérieur damier, retour en recherche après coulé ou après échec de l'axe.
  - Invariants : rejet de grille nulle (`ArgumentNullException`), grille pleine (`InvalidOperationException`), simulation d'une partie complète (aucune case rejouée), test de détection de règle violée (FR8).
- 81/81 tests unitaires et d'intégration passent avec succès, build sans aucun avertissement.
