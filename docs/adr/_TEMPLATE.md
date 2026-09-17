# Gabarit ADR (Architecture Decision Record)

Un ADR par choix structurant réellement pris (ex. store en mémoire, découpage HTTP/gRPC, stratégie adverse). Conservez l'historique : ne supprimez pas un ADR remplacé, actualisez son statut.

Pour créer un ADR : copiez ce fichier vers `docs/adr/ADR-{numéro}-{intitulé-kebab-case}.md` et remplissez-le.

Les décisions d'architecture déjà actées pour ce projet (AD-1 à AD-11 : placement du domaine, frontière HTTP/gRPC, propriété/concurrence de l'état, stratégie adverse, mapping des codes d'erreur gRPC, etc.) sont la matière première attendue pour les premiers ADR — voir `_bmad-output/planning-artifacts/architecture/architecture-BattleShip-2026-09-15/ARCHITECTURE-SPINE.md` et son dossier `reviews/`. Ce travail de rétro-transcription est couvert par la Story 3.2, pas par cette story.

---

# ADR numéro : intitulé de la décision

## Statut et date
Proposé, accepté ou remplacé ; référence de l'ADR suivant si nécessaire.

## Contexte
Besoin à satisfaire, contraintes et enjeu de la décision.

## Options envisagées
Alternatives crédibles, avantages, limites et critères de comparaison.

## Décision
Option retenue et raisons du choix dans votre contexte.

## Conséquences
Effets attendus, compromis, risques et travail induit.

## Vérification et réexamen
Éléments qui étayent le choix ; conditions qui conduiraient à le revoir.

## Références
Liens utiles : documentation, issue, expérience ou commit.
