---
id: SPEC-battleship
companions: []
sources: [docs/cours_c_asp_net_bataille_navale.md]
---

> **Canonical contract.** This SPEC and the files in `companions:` are the complete, preservation-validated contract for what to build, test, and validate. Source documents listed in frontmatter are for traceability — consult them only if you need narrative rationale or prose color this contract intentionally omits.

# Bataille Navale — TP ASP.NET Core / Blazor

## Why

Mandat pédagogique : le cours ASP.NET/C# (HTS Learning) demande à chaque binôme de livrer, en 5 jours, une bataille navale jouable du navigateur jusqu'au serveur, sur un socle technique imposé (Minimal API, Blazor WebAssembly, modèles partagés, gRPC, FluentValidation, tests). Le TP compte pour 50 % de la note finale ; le socle fonctionnel est le minimum attendu, l'ambition du périmètre complémentaire est elle aussi évaluée.

## Capabilities

- **CAP-1**
  - **intent:** Le moteur de jeu crée deux grilles, place les flottes aléatoirement et résout les tirs jusqu'à la fin de partie, indépendamment du transport (HTTP/gRPC) et de l'UI.
  - **success:** Des tests unitaires démontrent l'absence de chevauchement/débordement des navires, le secret des positions adverses non découvertes, le rejet sans effet d'un coup invalide, et la non-comptabilisation d'un tir sur une case déjà jouée.

- **CAP-2**
  - **intent:** Un contrat d'API Minimal API permet au client de créer une partie, connaître son état et la faire évoluer via des DTOs respectant les règles de visibilité du jeu.
  - **success:** Les endpoints sont démontrables via un fichier `.http`, avec des codes HTTP cohérents avec les comportements annoncés.

- **CAP-3**
  - **intent:** Une boucle de jeu fait alterner joueur et ordinateur selon les règles définies par l'équipe, produit un résultat compréhensible et interdit tout coup après la fin de partie.
  - **success:** Une partie complète est démontrable de la création jusqu'à la victoire.

- **CAP-4**
  - **intent:** Un adversaire ordinateur joue des coups valides selon la même logique de validation que le joueur humain.
  - **success:** Des tests montrent que l'adversaire respecte les règles de validité des coups (stratégie et difficulté au choix de l'équipe — voir Open Questions).

- **CAP-5**
  - **intent:** Une interface Blazor WebAssembly permet de créer une partie, afficher les deux grilles avec les informations autorisées, jouer et voir les résultats / la fin de partie.
  - **success:** Une partie est jouable de bout en bout dans le navigateur ; un incident de communication est géré sans rendre l'interface inutilisable.

- **CAP-6**
  - **intent:** Au moins un échange fonctionnel entre le front et l'API transite en gRPC, accessible depuis le navigateur via gRPC-Web.
  - **success:** Une réponse et une erreur attendue sont démontrables sur cet échange.

- **CAP-7**
  - **intent:** Les entrées serveur, HTTP comme gRPC, sont validées avec FluentValidation.
  - **success:** Une requête invalide est rejetée avec un statut/erreur cohérent, vérifié par un test explicite.

- **CAP-8**
  - **intent:** Des tests métier et d'intégration couvrent les règles du jeu et les contrats d'échange.
  - **success:** Les tests détectent une règle violée (pas seulement le cas nominal), démontré par un exemple avant/après correction.

- **CAP-9**
  - **intent:** L'équipe documente son usage de l'IA via `PROMPTS.md` (échanges décisifs), `docs/adr/` (décisions structurantes) et `REVUE-IA.md` (revues argumentées).
  - **success:** Chaque fichier existe (racine ou `docs/`), contient au moins 3 revues dans `REVUE-IA.md`, et chaque entrée est reliée à une preuve reproductible (commit, test).

## Constraints

- API imposée en ASP.NET Core Minimal API sur .NET 10 (pas de contrôleurs MVC).
- Front imposé en Blazor WebAssembly.
- `BattleShip.Models` ne dépend d'aucun autre projet : le moteur de jeu reste indépendant de HTTP, JSON et gRPC.
- FluentValidation obligatoire sur toutes les entrées serveur, HTTP et gRPC.
- Au moins un échange gRPC fonctionnel est obligatoire, accessible en gRPC-Web depuis le navigateur.
- Tests métier et tests d'intégration obligatoires, capables de détecter une règle violée.
- Historique Git exploitable requis : commits relus par le binôme, travail lisible — fait partie de la note.
- Délai fixe de 5 jours ; seul le commit poussé avant le début du QCM du jour 5 est évalué.
- Livrables obligatoires au dépôt : `README.md` (lancement, fonctionnalités, arbitrages du backlog, limites), `PROMPTS.md`, `docs/adr/`, `REVUE-IA.md`.
- Le projet doit pouvoir être lancé par un autre binôme en suivant uniquement le `README.md`.

## Non-goals

- Une reproduction fidèle du jeu de plateau physique n'est pas demandée : règles précises, taille de grille et composition de la flotte sont des choix libres de l'équipe, pas une contrainte du socle.
- Le QCM individuel du jour 5 (sans IA, sans document) est une évaluation séparée, hors périmètre de construction de ce spec.

## Success signal

Une partie complète se joue dans le navigateur, de la création à la victoire, contre l'ordinateur, avec au moins un échange gRPC-Web démontrable (réponse + erreur attendue) et des entrées validées côté serveur. Les tests détectent une règle métier violée avant correction et passent après. Le dépôt est lançable par un autre binôme à partir du seul `README.md`, avec `PROMPTS.md`, `docs/adr/` et `REVUE-IA.md` (≥ 3 revues) à jour et reliés à des preuves.

## Assumptions

- Le "socle" (CAP-1 à CAP-9 + contraintes ci-dessus) est traité comme le périmètre minimal obligatoire ; les extensions de backlog évoquées par le cours (multijoueur, adversaire élaboré, sauvegarde, historique, statistiques, personnalisation, accessibilité, déploiement) sont hors socle et relèvent de décisions d'équipe non encore prises.

## Open Questions

- Quelle taille de grille et quelle composition de flotte (nombre/tailles des navires) pour CAP-1 ?
- Quel algorithme/stratégie pour l'adversaire ordinateur (CAP-4) : tir aléatoire simple, ciblage après touche, difficulté progressive ?
- Quelle opération précise sera exposée en gRPC (CAP-6) : création de partie, envoi d'un tir, autre ?
- L'état des parties est-il conservé en mémoire uniquement, ou une persistance (fichier/BD) est-elle visée ?
- Quelles extensions du backlog l'équipe priorise-t-elle dans les 5 jours : multijoueur, sauvegarde, historique/stats, accessibilité, déploiement, autre ?
