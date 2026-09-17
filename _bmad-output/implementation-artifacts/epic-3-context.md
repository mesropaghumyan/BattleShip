# Epic 3 Context: Traçabilité IA et préparation de la remise

<!-- Compiled from planning artifacts. Edit freely. Regenerate with compile-epic-context if planning docs change. -->

## Goal

Cet epic garantit que l'usage de l'IA pendant le projet est documenté de façon vérifiable et que le dépôt est livrable en autonomie. Concrètement : mettre en place dès le début du sprint les gabarits de traçabilité IA (`PROMPTS.md`, `docs/adr/`, `REVUE-IA.md`), les alimenter au fil de l'eau pendant les Epics 1 et 2, puis les auditer et les compléter avant la remise, et produire un `README.md` permettant à un autre binôme de lancer et comprendre le projet sans aide supplémentaire. C'est une condition de recevabilité de la remise (exigences minimales du cours), pas une fonctionnalité du jeu.

## Stories

- Story 3.1: Mise en place du processus de traçabilité IA
- Story 3.2: Consolidation des livrables IA avant remise
- Story 3.3: README de remise

## Requirements & Constraints

- L'équipe doit documenter son usage de l'IA via trois fichiers dédiés, avec au moins 3 revues argumentées au total.
- Chaque entrée de revue IA doit comporter : une hypothèse vérifiable, une expérience décrite avant exécution, une observation réelle, une décision justifiée, et un lien vers un commit ou un test reproductible.
- Chaque décision d'architecture structurante réellement prise (choix du store en mémoire, découpage HTTP/gRPC, stratégie adverse, etc.) doit avoir un ADR à jour avec un statut explicite (accepté/remplacé).
- `PROMPTS.md` ne doit couvrir que les échanges IA réellement décisifs (pas une transcription intégrale), chacun relié à une décision et à une preuve.
- L'historique Git doit être exploitable et lisible par le binôme (relecture des commits attendue).
- Contrainte de délai stricte : seul le commit poussé avant le début du QCM du jour 5 est évalué — le calendrier de livraison n'est pas négociable.
- Le `README.md` doit permettre à un tiers sans connaissance préalable de restaurer les dépendances, compiler, lancer les tests, démarrer l'API et l'application, et jouer une partie complète, en suivant uniquement ce fichier.
- Le `README.md` doit aussi documenter les fonctionnalités livrées, les arbitrages du backlog (ce qui a été choisi/écarté et pourquoi) et les limites connues (ex. état en mémoire uniquement, pas de multijoueur).

## Technical Decisions

- Ce périmètre est explicitement hors architecture logicielle : c'est un processus documentaire, pas un invariant technique. Le format exact des gabarits ADR/PROMPTS/REVUE-IA est laissé à l'implémentation.
- Convention de langue déjà actée pour le projet : le code reste en anglais (PascalCase/camelCase), mais l'UI, le `README.md`, les ADR, `PROMPTS.md` et `REVUE-IA.md` s'écrivent en français.
- Gabarit `PROMPTS.md` attendu : date/sujet, outil, contexte, prompt, réponse résumée, décision, vérification, preuve.
- Gabarit ADR attendu (dans `docs/adr/`) : statut/date, contexte, options, décision, conséquences, vérification, références.
- Gabarit `REVUE-IA.md` attendu : proposition, hypothèse, expérience, observation, décision, preuves.
- Les décisions d'architecture déjà actées dans le spine (couvrant notamment le placement du noyau de domaine, la frontière de visibilité des vues, la propriété/concurrence de l'état, la répartition HTTP/gRPC, la validation, la stratégie adverse, l'organisation des tests, la frontière de référence entre projets, le partage du contrat gRPC, l'identité des camps, et le mapping des codes d'erreur gRPC) sont la matière première attendue pour les ADR de ce dépôt — elles doivent être retranscrites en ADR au fil de l'eau plutôt que rattrapées en fin de sprint.

## Cross-Story Dependencies

- Story 3.1 doit idéalement être posée avant même le démarrage de l'Epic 1, car les Stories 3.2 et 3.3 supposent que la documentation s'est construite au fil de l'eau pendant les Epics 1 et 2, sans rattrapage de dernière minute.
- Story 3.2 dépend du contenu réellement produit pendant les Epics 1 et 2 (décisions d'architecture actées, échanges IA décisifs, propositions revues) : elle consolide, elle ne crée pas la matière depuis zéro.
- Story 3.3 dépend d'un projet fonctionnellement complet (API, tests, application Blazor, échange gRPC) puisque le README doit permettre de réellement compiler, lancer et jouer une partie complète.
- La contrainte de délai (commit final avant le QCM du jour 5) s'applique à l'ensemble du dépôt et conditionne la clôture de toutes les stories de cet epic.
