---
title: 'Mise en place du processus de traçabilité IA'
type: 'chore' # feature | bugfix | refactor | chore
created: '2026-09-17'
status: 'done' # draft | ready-for-dev | in-progress | in-review | done
route: 'oneshot' # oneshot | dispatch — set by step-02's route gate after design
review_loop_iteration: 0 # incremented by step-04 before each review loopback
context: ['{project-root}/docs/cours_c_asp_net_bataille_navale.md'] # gabarits officiels PROMPTS.md / ADR / REVUE-IA (diapos 56-58)
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** L'équipe n'a pas encore de gabarits pour consigner ses échanges IA décisifs, ses décisions d'architecture (ADR) et ses revues IA, alors que ces trois livrables (`PROMPTS.md`, `docs/adr/`, `REVUE-IA.md`) sont une condition de recevabilité de la remise (FR9/CAP-9, ≥ 3 revues argumentées attendues).

**Approach:** Créer à la racine du dépôt `PROMPTS.md` et `REVUE-IA.md`, et le dossier `docs/adr/` avec un gabarit `_TEMPLATE.md`, en reprenant fidèlement les gabarits officiels du cours (diapos 56-58 de `docs/cours_c_asp_net_bataille_navale.md`), chacun précédé d'une courte note d'usage rappelant qu'il s'agit d'un gabarit à dupliquer/remplir au fil de l'eau — pas de contenu rétroactif des Epics 1-2 dans cette story (ce sera fait en Story 3.2).

</frozen-after-approval>

## Implementation Notes

- Créé `PROMPTS.md` et `REVUE-IA.md` à la racine du dépôt, reprenant mot pour mot les gabarits officiels des diapos 56 et 58 (`docs/cours_c_asp_net_bataille_navale.md`), précédés d'une note d'usage expliquant le rôle du fichier et comment ajouter une entrée (dupliquer le gabarit, l'insérer après le bloc de référence).
- Créé `docs/adr/_TEMPLATE.md` reprenant le gabarit ADR officiel (diapo 57), avec une note d'usage indiquant de copier ce fichier vers `docs/adr/ADR-{numéro}-{intitulé-kebab-case}.md` pour chaque nouvel ADR, et un pointeur vers `ARCHITECTURE-SPINE.md` (AD-1..AD-11) comme matière première pour la Story 3.2 — pas de dossier vide sans fichier, le template lui-même sert de gabarit versionné.
- **Correction post-revue (Blind Hunter) :** `docs/Prompt.md` existait déjà de facto (17 entrées réelles couvrant les Epics 1-2, créé au fil de l'eau pendant ces epics), en collision avec le `PROMPTS.md` vide créé initialement à la racine. Consolidation effectuée : les 17 entrées ont été déplacées vers `PROMPTS.md` (racine, emplacement du gabarit officiel), précédées du gabarit de référence ; `docs/Prompt.md` supprimé (`git rm`). Une 18e entrée documente cette consolidation elle-même.
- Aucun contenu ADR/REVUE-IA rétroactif des Epics 1-2 ajouté ici : conformément à l'AC de la story et à `epic-3-context.md`, ce remplissage relève de la Story 3.2 (consolidation). Seul le journal des prompts préexistant a été relocalisé, pas recréé.
- **Vérification :** les trois gabarits ont été comparés mot pour mot aux diapos 56-58 du support de cours ; `git status`/`git diff` relus après la fusion de `docs/Prompt.md` pour confirmer qu'aucune des 17 entrées existantes n'a été perdue ou altérée.

## Review Triage Log

- **high (patch appliqué)** — `PROMPTS.md` (racine, vide) entrait en collision avec `docs/Prompt.md` (préexistant, 17 entrées réelles). Confirmé par lecture de `docs/Prompt.md` (294 lignes) et `git log`. Corrigé par consolidation (voir Implementation Notes).
- **medium (patch appliqué)** — `docs/adr/_TEMPLATE.md` ne pointait pas vers `ARCHITECTURE-SPINE.md` comme source des décisions déjà actées. Ajout d'une phrase de renvoi.
- **low (patch appliqué)** — `PROMPTS.md`/`REVUE-IA.md` ne précisaient pas où insérer une nouvelle entrée par rapport au gabarit. Précisé : « insérer après ce bloc, ne pas le supprimer ».
- **low (patch appliqué)** — Implementation Notes ne contenaient pas de déclaration de vérification, contrairement aux autres specs du dépôt. Ajouté.
- **defer** — `CLAUDE.md` (mode Caveman) entre en tension avec l'exigence de prose française argumentée de l'Epic 3. Réel, mais correction hors périmètre de cette story (édition de CLAUDE.md exclue des routes patch/HALT). Ajouté à `deferred-work.md`.
- **defer** — `skills-lock.json` (skills `caveman-*`) sans instructions d'installation documentées, ce qui gênerait un tiers suivant uniquement le futur `README.md`. Réel mais relève de la Story 3.3 (README de remise), pas de cette story. Ajouté à `deferred-work.md`.
- **false** — Le `.gitignore` modifié (ordre alphabétique rompu par `.agents/`, pas de retour à la ligne final) est antérieur à cette story (déjà présent en `M .gitignore` avant tout travail sur l'Epic 3, confirmé par `git diff` isolé sur ce fichier) : pas causé par ce changement, hors périmètre.
- **maybe-false, rejeté (low si vrai)** — `sprint-status.yaml` : les 7 actions ouvertes de la rétrospective de l'Epic 1 ne sont pas croisées avec le démarrage de l'Epic 3. Cosmétique/organisationnel, pas un défaut de cette story ; laissé tel quel.

