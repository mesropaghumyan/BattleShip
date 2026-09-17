---
title: 'Consolidation des livrables IA avant remise'
type: 'chore' # feature | bugfix | refactor | chore
created: '2026-09-17'
status: 'done' # draft | ready-for-dev | in-progress | in-review | done
route: 'dispatch' # oneshot | dispatch — set by step-02's route gate after design
review_loop_iteration: 0 # incremented by step-04 before each review loopback
baseline_commit: '02be9ea5942b7624f21ada1902e13ae4fa1cd929'
context: ['{project-root}/docs/cours_c_asp_net_bataille_navale.md'] # gabarits officiels (diapos 56-58) + rubrique d'évaluation
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** `PROMPTS.md` et `REVUE-IA.md` (racine) et `docs/adr/` (Story 3.1) n'ont que leurs gabarits vides ou incomplets : `docs/adr/` n'a aucun ADR réel malgré 11 décisions d'architecture déjà actées (`ARCHITECTURE-SPINE.md`, AD-1..AD-11), et `REVUE-IA.md` n'a aucune des ≥3 revues argumentées exigées, alors que la matière première (findings réels, preuves, commits) existe déjà dans les rétrospectives des Epics 1 et 2.

**Approach:** Consolider les trois livrables à partir des preuves déjà produites, sans en inventer : rédiger un ADR par décision d'architecture déjà actée dans `ARCHITECTURE-SPINE.md`, extraire au moins 3 revues argumentées réelles des rétrospectives Epic 1/2 et de `deferred-work.md` vers `REVUE-IA.md`, et auditer `PROMPTS.md` pour vérifier qu'il couvre les échanges réellement décisifs.

</frozen-after-approval>

## Code Map

- `_bmad-output/planning-artifacts/architecture/architecture-BattleShip-2026-09-15/ARCHITECTURE-SPINE.md` -- source des 11 ADR (section "Invariants & Rules", AD-1 à AD-11 ; chaque AD a déjà Contexte/Prevents(≈ options écartées)/Rule(≈ décision)/Binds).
- `docs/adr/_TEMPLATE.md` -- gabarit à copier vers `docs/adr/ADR-{numéro}-{intitulé}.md` pour chaque ADR (créé en Story 3.1, déjà pointeur vers le spine).
- `REVUE-IA.md` -- gabarit en place (Story 3.1) ; ajouter les revues **après** le bloc gabarit, sans le supprimer.
- `_bmad-output/implementation-artifacts/epic-1-retro-2026-09-15.md`, `epic-2-retro-2026-09-16.md`, `deferred-work.md` -- source des revues : findings déjà vérifiés avec preuve (commit/test), à reformuler au format REVUE-IA (hypothèse → expérience → observation → décision), pas à copier tel quel.
- `PROMPTS.md` -- 18 entrées déjà en place (Story 3.1) ; auditer, ne pas réécrire sauf lacune réelle trouvée.

## Tasks & Acceptance

**Execution:**
- [x] `docs/adr/ADR-0001-...md` .. `ADR-0011-...md` -- un ADR par AD du spine, statut "Accepté", en copiant `_TEMPLATE.md` -- voir mapping en Design Notes
- [x] `REVUE-IA.md` -- ajouter les 5 revues listées en Design Notes, sous le bloc gabarit, chacune avec hypothèse/expérience/observation/décision/preuve réelle
- [x] `PROMPTS.md` -- relire les 18 entrées ; si une décision réellement structurante de cette story (ex. choix des revues, numérotation des ADR) le justifie, ajouter une entrée ; sinon ne rien changer

**Acceptance Criteria:**
- Given `REVUE-IA.md` en fin de story, when on le relit, then il contient au moins 3 revues avec hypothèse vérifiable, expérience décrite avant exécution, observation réelle, décision justifiée et lien vers un commit/test reproductible
- Given `docs/adr/`, when on relit les ADR, then chaque décision structurante déjà actée (AD-1..AD-11) a un ADR à jour avec un statut (accepté/remplacé)
- Given `PROMPTS.md`, when on le relit, then il couvre les échanges réellement décisifs, chacun relié à une décision et une preuve

## Implementation Notes

- Implémentation déléguée à un sous-agent sans contexte préalable, sur la seule base de cette spec (voir handoff au step-03).
- 11 ADR créés (`docs/adr/ADR-0001-...md` à `ADR-0011-...md`), un par AD du spine, tous statut "Accepté" — mapping et structure conformes au Design Notes ci-dessous, `_TEMPLATE.md` non modifié.
- 5 revues ajoutées à `REVUE-IA.md` après le bloc gabarit (conservé intact), reformulées (pas copiées) au format hypothèse/expérience/observation/décision/preuve.
- `PROMPTS.md` audité (18 entrées existantes couvrent déjà les échanges décisifs) ; une 19ᵉ entrée ajoutée pour documenter l'exécution de cette story elle-même (seule décision structurante propre à 3.2 : numérotation directe AD-N → ADR-N à 4 chiffres, choix des 5 revues).
- **Vérification indépendante (orchestrateur, contre le diff, pas le seul rapport du sous-agent) :** diff lu intégralement depuis `baseline_commit`. Commits effectivement cités dans le nouveau contenu (`13e84d9` dans `REVUE-IA.md`, `9525262` dans `ADR-0007`) confirmés présents via `git log` ; `3dc8c44`/`48f33b3` existent aussi dans l'historique mais ne sont cités dans aucun nouveau fichier de cette story (corrigé après revue, voir Review Triage Log). Contenu des 11 ADR lu en entier, conforme au mapping. Gabarits `REVUE-IA.md`/`PROMPTS.md` confirmés intacts (aucune entrée existante perdue ou altérée).
- Rien d'inachevé au regard du périmètre de cette story ; les limites de code documentées dans les ADR/revues (contrainte `WithGame<TResult>`, absence de filet gRPC générique, etc.) restent volontairement non corrigées — c'est le rôle de code, pas de documentation, hors périmètre de 3.2.

## Design Notes

**Mapping ADR (AD du spine → fichier) :**
1. Placement du noyau de domaine · 2. Frontière de visibilité (`GameStateDto`) · 3. Propriété/concurrence de l'état (`IGameStore`) · 4. Répartition transports HTTP/gRPC · 5. Emplacement de la validation · 6. Stratégie adverse (`IOpponentStrategy`) · 7. Organisation des tests · 8. Frontière App/API (pas de `ProjectReference`) · 9. Source/partage du contrat gRPC · 10. Identité des camps (`enum Side`) · 11. Mapping des codes d'erreur gRPC.
Chaque ADR : Contexte = le "Prevents" du spine ; Options envisagées = l'alternative que le "Prevents" écarte implicitement ; Décision = le "Rule" du spine ; Conséquences = ce que l'AD rend possible/impossible pour les stories suivantes ; Références = `ARCHITECTURE-SPINE.md#AD-N`.

**5 revues candidates pour `REVUE-IA.md`** (sourcées, pas inventées) :
1. Duplication des options JSON serveur/client (Story 1.4/1.6) : hypothèse que les deux camelCase resteraient synchronisés sans contrat partagé → bug réel constaté à l'usage → corrigé, risque résiduel tracé (`deferred-work.md`).
2. Statut périmé dans le repli local de `Play.razor` (rétro Epic 1) : trouvé indépendamment par 3 lenses de revue → confirmé et corrigé (`ToGameOutcome(turn.Status)`, commit `13e84d9`).
3. `IGameStore.WithGame<TResult> : class` force des contournements (rétro Epic 1) : hypothèse que la contrainte ne gênerait aucun appelant → contournements répétés constatés dans les tests → décision : reporté, pas corrigé cette story.
4. `IOpponentStrategy.ChooseShot(Grid)` expose toute la grille (Story 1.3, vérifié en Story 2.1) : hypothèse qu'une implémentation future pourrait "tricher" → vérifié que `HardOpponentStrategy` n'accède jamais à `Grid.Ships` → risque concrétisé sainement, garde de type toujours absente (reporté).
5. Écart Story 2.4 : AC mentionne `Summary.razor`, l'affichage réel est dans `Play.razor` (rétro Epic 2) : hypothèse d'un écart bloquant → comportement utilisateur conforme malgré le découpage différent → décision : accepté, écart documenté plutôt que refactoré sans nécessité.

## Verification

**Manual checks (no CLI — docs-only story):**
- Chaque ADR comparé au AD correspondant du spine (aucune décision non couverte, aucune inventée).
- Chaque revue de `REVUE-IA.md` reliée à un commit ou un test réel existant (pas de preuve fabriquée).
- `git status`/`git diff` relus avant commit pour confirmer qu'aucun contenu existant (`PROMPTS.md`, gabarits Story 3.1) n'a été perdu.

## Review Triage Log

- **medium (patch appliqué)** — L'AC exige ≥3 revues avec « lien vers un commit/test reproductible » ; seules les revues 2 (commit `13e84d9`) et 3 (tests nommés) en avaient un direct. Les revues 1, 4 et 5 ne citaient que des documents secondaires (specs/rétros), sans hash de commit ni test nommé. Corrigé : ajout du commit `f4d4e4b` (revue 1, Story 1.6), `ebb2e3b` (revue 4, Story 2.1) — les deux vérifiés via `git log` — et `fc9e8dc` + les tests existants réels `BattleShip.Tests/Unit/GameTests.cs`/`BattleShip.Tests/Integration/GameEndpointsTests.cs` (revue 5, Story 2.4 ; vérifié qu'aucun fichier `GameStatisticsCalculatorTests` dédié n'existe, corrigé pour citer les fichiers réels). *(Edge Case Hunter, confirmé)*
- **low (patch appliqué)** — Implementation Notes affirmait que « 4 commits cités dans les revues » étaient vérifiés (`13e84d9`, `9525262`, `3dc8c44`, `48f33b3`), mais `3dc8c44`/`48f33b3` n'apparaissent nulle part dans le nouveau contenu (`docs/adr/`, `REVUE-IA.md`) — vérifié par `grep`, absents. La phrase surclaimait la portée de la vérification. Corrigée pour ne citer que ce qui est réellement présent dans le diff. *(Blind Hunter + Edge Case Hunter, confirmé)*
- **low (patch appliqué)** — `sprint-status.yaml` indiquait `3-2-...: in-progress` alors que le frontmatter de la spec est passé à `in-review` au début de cette étape — désynchronisation entre les deux fichiers de suivi. Corrigée : `sprint-status.yaml` aligné sur `review`. *(Blind Hunter + Edge Case Hunter, confirmé)*
- **low (patch appliqué)** — `PROMPTS.md` entrée 19 : « numérotation directe AD-N → ADR-000N » est imprécis pour N≥10 (AD-10/AD-11 donnent `ADR-0010`/`ADR-0011`, pas littéralement « ADR-000 » + N). Reformulé. *(Blind Hunter, confirmé)*
- **low (patch appliqué)** — `ADR-0003`, section Vérification : phrase ambiguë (« la correction du verrou repose sur une relecture directe du lock plutôt que sur un test de stress » se lit comme si un correctif était en cours). Reformulée pour clarifier qu'il s'agit d'une vérification par relecture de code, pas d'un correctif en attente. *(Blind Hunter, confirmé)*
- **false** — `.gitignore`/`CLAUDE.md`/`skills-lock.json` apparaissent dans le diff mais ne sont pas un scope creep de cette story : ces trois fichiers sont datés du 15 septembre (`ls -la`), antérieurs à toute la branche `feature/epic-3`, et étaient déjà dans cet état exact (`M .gitignore`, `?? CLAUDE.md`, `?? skills-lock.json`) au tout début de cette session, avant même la Story 3.1. Ils apparaissent dans `{diff_file}` uniquement parce que sa génération (`git add -A -N .` puis `git diff {baseline_commit}`) inclut tout changement non committé du répertoire de travail, pas seulement celui de cette story — un artefact de la commande de diff de revue, pas un défaut de l'implémentation. *(Blind Hunter, réfuté)*
- **false** — `ADR-0011` n'omet pas la limite connue (absence de filet `catch(Exception)` générique) : elle est documentée dans « Conséquences », section du gabarit dédiée précisément aux effets et limites d'une décision, distincte de « Décision » qui rapporte fidèlement la règle du spine. Rien n'est caché ; c'est la séparation voulue par le gabarit ADR. *(Blind Hunter, réfuté)*
- **low, rejeté** — Audit de `PROMPTS.md` présenté comme une assertion narrative sans tableau de correspondance décision↔entrée. Réel, mais l'AC de cette story porte sur le contenu des entrées elles-mêmes (déjà satisfait), pas sur la forme de l'audit ; produire un tableau exhaustif pour 18 entrées serait disproportionné pour un gain cosmétique. *(Blind Hunter)*
- **low, rejeté** — Convention de renvoi inter-ADR incohérente (certains citent `AD-N`, aucun ne cite les nouveaux identifiants `ADR-000N` entre eux). Réel mais cosmétique ; uniformiser toucherait les 11 fichiers pour un bénéfice de navigation mineur, pas une correction directe. *(Blind Hunter)*
- **low, rejeté** — Les 5 revues ne déclarent pas toutes explicitement leur méthode de découverte (seule la revue 2 nomme « 3 lenses »). Réel mais chaque revue décrit déjà sa méthode dans son champ Expérience ; le gabarit REVUE-IA n'exige pas de label de méthode. *(Blind Hunter)*
- **defer, déjà tracé** — `skills-lock.json` pin des skills externes sans documentation d'installation. Déjà signalé et déféré en Story 3.1 (`deferred-work.md`) ; pas de nouvelle entrée. *(Blind Hunter, confirme le tracking existant)*
