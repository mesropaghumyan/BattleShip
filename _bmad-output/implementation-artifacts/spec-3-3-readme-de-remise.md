---
title: 'README de remise'
type: 'chore' # feature | bugfix | refactor | chore
created: '2026-09-17'
status: 'done' # draft | ready-for-dev | in-progress | in-review | done
route: 'oneshot' # oneshot | dispatch — set by step-02's route gate after design
review_loop_iteration: 0 # incremented by step-04 before each review loopback
baseline_commit: '61791ea3b48129302d92161dd680ea65cdf77bc2'
context: ['{project-root}/_bmad-output/specs/spec-battleship/SPEC.md', '{project-root}/_bmad-output/implementation-artifacts/deferred-work.md', '{project-root}/_bmad-output/implementation-artifacts/epic-1-retro-2026-09-15.md', '{project-root}/_bmad-output/implementation-artifacts/epic-2-retro-2026-09-16.md']
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** Le `README.md` actuel est le stub de scaffold initial : il ne documente ni les profils de lancement corrects (bug réel déjà rencontré, commit `9525262`, action ouverte n°3 de la rétro Epic 1), ni les fonctionnalités livrées (CAP-1..12), ni les arbitrages de backlog, ni les limites connues — alors que NFR9/SPEC.md exigent qu'un tiers sans connaissance préalable puisse restaurer, compiler, tester, lancer et jouer une partie complète en suivant uniquement ce fichier.

**Approach:** Réécrire `README.md` à partir des preuves déjà produites (SPEC.md, ARCHITECTURE-SPINE.md, ADR, rétrospectives, `deferred-work.md`) : prérequis, commandes exactes de restauration/compilation/tests/lancement (avec les bons profils et ports pour éviter le bug de contenu mixte déjà rencontré), un scénario pas-à-pas pour jouer une partie complète, les fonctionnalités livrées, les arbitrages du backlog (choisi/écarté et pourquoi) et les limites connues sourcées des ADR/`deferred-work.md`. Clôturer l'action n°3 de la rétro Epic 1 (documentation du couple profil App/API) dans `sprint-status.yaml`, puisqu'elle est résolue par ce changement. Ne pas corriger de code : les limites listées restent telles que documentées.

</frozen-after-approval>

## Implementation Notes

- `README.md` réécrit intégralement : fonctionnalités livrées (CAP-1..12), prérequis, commandes restaurer/compiler/tester/lancer (bons profils/ports), scénario pas-à-pas pour jouer une partie complète, arbitrages du backlog (choisi/écarté et pourquoi), limites connues sourcées de `deferred-work.md`/ADR, pointeur vers `PROMPTS.md`/`docs/adr/`/`REVUE-IA.md`, structure du dépôt.
- Avertissement explicite ajouté sur le profil de lancement de l'API (http obligatoire, port 5268) — clôture l'action ouverte n°3 de la rétrospective Epic 1 (`sprint-status.yaml`, `epic-1-retro-item-3`, passée à `done`).
- Aucune correction de code : les limites listées (contrainte `WithGame<TResult>`, absence de filet gRPC générique, pas de test de régression JSON client, pas de test de composant Blazor, incohérence `row/col` vs `Coordinate` du proto) restent documentées telles quelles, conformément à l'Intent.
- **Vérification réelle exécutée (pas seulement lue) :** `dotnet restore BattleShip.slnx` (à jour), `dotnet build BattleShip.slnx` (0 avertissement/erreur), `dotnet test BattleShip.slnx --no-build` (101/101 réussis, exactement le chiffre annoncé dans le README). API démarrée sans `--launch-profile` (confirmé : profil `http` par défaut, port 5268) ; `POST /games` et `GET /games/{id}` testés par `curl` en conditions réelles, réponses conformes au contrat documenté. Instance API arrêtée après vérification.
- Non re-rejoué : le parcours navigateur complet (App Blazor, gRPC-Web, historique, statistiques) — déjà vérifié manuellement en Epic 1/2 (commits `f4d4e4b`, `9525262`) et aucun code n'a changé depuis (Epic 3 est docs-only) ; pas de nouvelle preuve navigateur produite par cette story.
- **Hors périmètre, action humaine restante :** NFR7 (relecture des commits par le binôme) et NFR8 (commit final poussé avant le début du QCM du jour 5) sont des actions procédurales que l'équipe doit accomplir elle-même au moment de la remise — aucun contenu de fichier ne peut les satisfaire à la place de l'équipe. Signalé à l'utilisateur en fin de story, pas bloquant pour l'approbation de cette spec.
- **Résidu de vérification :** un processus `BattleShip.API` lancé pendant la vérification manuelle (avant la revue) n'a pas pu être arrêté automatiquement (permission refusée par le classifieur auto mode) — signalé à l'utilisateur en fin de story, à arrêter manuellement (`Ctrl+C` dans le terminal correspondant, ou PID 73326/73344).
- **Changement demandé en cours de story (hors Intent initial, appliqué) :** l'utilisateur a demandé de déplacer `PROMPTS.md` et `REVUE-IA.md` de la racine vers `docs/`. `git mv` effectué pour les deux fichiers ; liens mis à jour dans `README.md` ; entrée 20 ajoutée à `docs/PROMPTS.md` documentant ce changement. Les mentions sans chemin dans `docs/adr/ADR-0003/0006/0008/0010` restent valides. Documents de planification figés (`SPEC.md`, `epics.md`, `ARCHITECTURE-SPINE.md`) et specs déjà `done` (3.1, 3.2) volontairement non modifiés — enregistrements historiques, pas des pointeurs à maintenir.

## Review Triage Log

- **high (patch appliqué)** — L'avertissement sur le profil de lancement de l'API affirmait que le profil `https` « n'écoute pas sur ce port » (5268). Faux : `BattleShip.API/Properties/launchSettings.json` montre que le profil `https` a `applicationUrl: "https://localhost:7164;http://localhost:5268"` — il bind aussi le port 5268. Aucun `UseHttpsRedirection` dans `Program.cs` qui interférerait. Corrigé : avertissement retiré côté API (aucun risque réel identifié), reformulé côté App. *(Blind Hunter, confirmé par lecture directe de `launchSettings.json`)*
- **high (patch appliqué)** — À l'inverse, le README affirmait que le profil `https` de l'App « fonctionne aussi si besoin » — faux : une App servie en HTTPS (`7297`) appelant l'API en HTTP (`5268`) déclenche un blocage navigateur de contenu mixte, risque explicitement identifié et non résolu dans la rétrospective de l'Epic 1 (« Risque de contenu mixte introduit par le correctif post-clôture »). Corrigé : avertissement déplacé et reformulé sur l'App, recommandation du profil `http` par défaut des deux côtés. *(Blind Hunter, confirmé contre `epic-1-retro-2026-09-15.md`)*
- **low (patch appliqué)** — « Structure du dépôt » omettait `docs/` et `_bmad-output/`, pourtant liés depuis plusieurs sections du README. Ajoutés. *(Blind Hunter, confirmé)*
- **low (patch appliqué)** — La description Blazor de la frontière de visibilité (« règles de visibilité respectées ») ne précisait pas ce que cela signifie pour un lecteur externe. Reformulée pour l'expliciter. *(Blind Hunter, confirmé)*
- **low (patch appliqué)** — Aucune indication pour arrêter les process ou libérer un port déjà occupé. Ajout d'une section courte « En cas de problème ». *(Blind Hunter, confirmé)*
- **low, rejeté** — « 101 tests » codé en dur sans garde-fou (CI/badge) qui deviendrait obsolète si des tests sont ajoutés/retirés. Réel mais hors périmètre : mettre en place une CI n'est pas demandé par cette story ni par les AC. *(Blind Hunter)*
- **low, rejeté** — Absence d'instructions pour faire confiance au certificat de développement HTTPS. Devenu sans objet : le README ne recommande plus le profil `https` de l'App (corrigé ci-dessus), donc ce besoin disparaît. *(Blind Hunter)*
- **low, rejeté** — Les « Limites connues » ne renvoient pas aux identifiants exacts des `action_items` de `sprint-status.yaml` (ex. `epic-1-retro-item-4`). Ce fichier de suivi interne BMAD n'est pas destiné à un lecteur externe/correcteur ; le renvoi déjà présent vers `docs/adr/`, `REVUE-IA.md` et `deferred-work.md` suffit à l'AC. *(Blind Hunter)*
- **low, rejeté** — Pas de marqueur explicite reliant le changement de statut dans `sprint-status.yaml` au README dans le diff. Résolu par le message de commit de cette story, pas par le contenu des fichiers eux-mêmes. *(Blind Hunter)*
- **false** — `sprint-status.yaml` à `in-progress` pendant que le README « lit comme terminé » : conforme au processus déjà suivi en Story 3.1 (oneshot) — le statut ne passe à `review` qu'à l'étape Finalize Spec, après la revue, pas avant. *(Blind Hunter, réfuté par le workflow)*
