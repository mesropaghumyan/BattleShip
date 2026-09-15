# Règles du jeu — détails

Référencé par CAP-1, CAP-4, CAP-12 dans SPEC.md.

## Grille

- Taille : 10 x 10 (coordonnées `A1`–`J10` ou équivalent 0-indexé selon l'implémentation).
- Chaque joueur possède sa propre grille de flotte (secrète) et voit uniquement les résultats de ses tirs sur la grille adverse.

## Flotte (par joueur)

| Navire | Taille (cases) | Quantité |
|---|---|---|
| Porte-avions | 5 | 1 |
| Croiseur | 4 | 1 |
| Contre-torpilleur | 3 | 1 |
| Sous-marin | 3 | 1 |
| Torpilleur | 2 | 1 |

Total : 5 navires, 17 cases occupées.

## Règles de placement

- Orientation horizontale ou verticale uniquement (pas de diagonale).
- Aucun chevauchement entre navires.
- Aucune contiguïté requise à éviter au-delà du non-chevauchement (deux navires peuvent être adjacents case à case) — simplification volontaire par rapport à certaines variantes du jeu physique.
- Placement aléatoire pour le socle (CAP-1) ; un placement manuel par le joueur peut être une amélioration UI mais n'est pas requis.

## Règles de tir

- Un tir cible une case de la grille adverse par ses coordonnées.
- Un tir sur une case déjà jouée est rejeté sans changer l'état de la partie (ne compte pas comme un nouveau coup).
- Un tir touche s'il atteint une case occupée par un navire ; un navire est coulé quand toutes ses cases sont touchées.
- La partie se termine quand tous les navires d'un camp sont coulés ; le camp adverse gagne.
- Aucun tir n'est accepté après la fin de partie.

## Algorithme de l'adversaire (CAP-4, CAP-12)

**Mode facile** — tir aléatoire uniforme parmi les cases non encore jouées.

**Mode difficile** — chasse/cible (hunt & target) :
1. **Phase de recherche (hunt)** : tire aléatoirement, mais uniquement sur les cases d'une couleur du damier (parité `(x + y) % 2 == 0`), suffisant pour toucher tout navire de taille ≥ 2.
2. **Phase de ciblage (target)** : après un tir touché, teste les 4 cases adjacentes (haut/bas/gauche/droite) non encore jouées.
3. **Phase d'alignement** : dès qu'un deuxième tir touché confirme un axe (horizontal ou vertical), poursuit dans cet axe jusqu'à couler le navire ou essuyer un échec, puis retourne en phase de recherche.

Le mode choisi (facile/difficile) est un paramètre de la partie, sélectionné avant son lancement (CAP-12).

## Historique et statistiques (CAP-10, CAP-11)

- Chaque coup joué (par le joueur humain ou l'ordinateur) est enregistré : auteur, coordonnée, résultat (raté/touché/coulé).
- En fin de partie, les statistiques calculées par camp : nombre de tirs, nombre de touches, taux de réussite (touches / tirs), durée de la partie (entre création et fin).

## gRPC — opération exposée (CAP-6)

- L'action "jouer un coup" (tir) est exposée en gRPC (via gRPC-Web depuis le client Blazor), en plus de son équivalent HTTP pour la création/consultation de partie.
- Réponse attendue : résultat du tir (raté/touché/coulé/partie terminée).
- Erreur attendue démontrable : coordonnée hors grille, coup rejoué sur une case déjà jouée, ou tir après fin de partie.
