# REVUE-IA.md

Revues argumentées de propositions IA du projet BattleShip. Au moins 3 revues sont attendues avant la remise, chacune avec une hypothèse vérifiable, une expérience décrite avant exécution, une observation réelle et une décision justifiée.

Pour ajouter une revue : dupliquez le bloc « Gabarit » ci-dessous, insérez la copie **après** ce bloc (le gabarit reste en tête, inchangé, comme référence), et remplissez-la avec un sujet précis du projet et des preuves réelles (commits, tests).

---

## Gabarit (à dupliquer pour chaque nouvelle revue — ne pas supprimer ce bloc)

## Revue : un sujet précis de votre projet

**Proposition examinée**
Code ou choix concerné, avec sa référence dans le dépôt.

**Hypothèse à vérifier**
Ce qui doit être vrai pour que cette proposition soit acceptable.

**Expérience**
Scénario, données ou commande ; résultat attendu avant exécution. Quelle erreur ce contrôle serait-il capable de détecter ?

**Observation**
Résultat réellement obtenu et éléments permettant de le reproduire.

**Décision et justification**
Acceptée, adaptée ou rejetée ; raisons fondées sur les observations.

**Preuves et limites**
Liens vers les commits et les tests ; ce qui reste non vérifié.

---

## Revue : duplication des options JSON serveur/client (`JsonSerializerOptions`)

**Proposition examinée**
Les options de sérialisation JSON (camelCase, enums en chaîne) sont déclarées indépendamment des deux côtés du réseau : côté serveur dans `BattleShip.API/Program.cs` (`ConfigureHttpJsonOptions`, Story 1.4) et côté client dans `BattleShip.App/Services/GameHttpClient.cs` (Story 1.6).

**Hypothèse à vérifier**
Les deux déclarations indépendantes resteraient synchronisées sans contrat partagé qui les force à évoluer ensemble.

**Expérience**
Lancer l'App contre l'API réelle après l'écriture initiale du client (Story 1.6, avant vérification manuelle) et observer si la désérialisation de `GameStateDto` réussit sur le premier appel. Un défaut attendu détectable : une `JsonException` au premier appel si les options ne matchent pas.

**Observation**
L'hypothèse était fausse : le client par défaut ne matchait pas le format serveur (options par défaut différentes) et levait effectivement une `JsonException` sur le premier appel réel. Le défaut a été trouvé pendant la vérification manuelle de la Story 1.6, pas par un test automatisé — `dotnet test` passait intégralement (71/71) au moment où le bug existait en pratique.

**Décision et justification**
Corrigée : les options du client ont été alignées sur celles du serveur (Story 1.6). Risque résiduel accepté et tracé plutôt que réellement éliminé : aucun test de régression ne protège cette synchronisation, donc une régression future sur ce point ne serait pas rattrapée par la suite automatisée.

**Preuves et limites**
Correctif : commit `f4d4e4b` (Story 1.6, `git log` vérifié). Bug et correctif détaillés dans les Implementation Notes de `_bmad-output/implementation-artifacts/spec-1-6-interface-blazor-créer-une-partie-et-jouer.md` ; risque résiduel tracé dans `_bmad-output/implementation-artifacts/deferred-work.md` (entrée Story 1.6, « GameHttpClient n'a aucun test de régression sur ses options JSON ») et confirmé toujours ouvert en rétrospectives Epic 1 et Epic 2. Limite : pas de test automatisé reproductible pour ce cas précis à ce jour — seul le commit atteste la correction.

---

## Revue : statut périmé dans le repli local de `Play.razor`

**Proposition examinée**
`ApplyLocalShotFallback` dans `BattleShip.App/Pages/Play.razor` (ajouté en Story 1.6 pour corriger un autre symptôme : les cases redevenant cliquables après un échec réseau).

**Hypothèse à vérifier**
Le repli local recalculait correctement tous les champs de l'état affiché (dont `Status`) à partir de la réponse du tour déjà reçue, pas seulement les champs visés par le correctif d'origine.

**Expérience**
Jouer un tir qui termine la partie (victoire ou défaite) pendant que le rafraîchissement réseau de l'état complet échoue, puis observer si l'interface affiche la fin de partie et verrouille la grille adverse. Résultat attendu si le code est correct : statut `Won`/`Lost` affiché immédiatement, grille verrouillée.

**Observation**
L'hypothèse était fausse : `ApplyLocalShotFallback` réutilisait `state.Status` (l'ancien statut) au lieu de mapper `turn.Status` (le nouveau, déjà disponible sur la réponse gRPC). Si le tir qui échoue à se rafraîchir terminait la partie, l'UI restait bloquée sur « en cours » et la grille adverse restait cliquable — contredisant directement l'AC de la Story 1.6. Trouvé indépendamment par 3 lenses de revue (adversarial, edge-case-hunter, verification-gap) lors de la rétrospective de l'Epic 1.

**Décision et justification**
Corrigée : ajout de `ToGameOutcome(turn.Status)` pour mapper le statut réel du tour plutôt que l'ancien statut mémorisé. Acceptée sans réserve, le comportement corrigé correspond exactement à l'AC de la story.

**Preuves et limites**
Commit `13e84d9` (`fix: corriger mapping GameOutcome dans Play.razor`, confirmé présent dans l'historique Git). Limite : aucun test de composant Blazor ne pin ce comportement (pas d'outillage bUnit dans le dépôt, AD-7) — une régression future sur ce même champ ne serait détectée que par relecture ou usage manuel.

---

## Revue : `IGameStore.WithGame<TResult> : class` force des contournements

**Proposition examinée**
La contrainte générique `where TResult : class` sur `IGameStore.WithGame<TResult>` (Story 1.4, AD-3).

**Hypothèse à vérifier**
La contrainte à un type référence ne gênerait aucun appelant, tous les usages prévus retournant naturellement un objet.

**Expérience**
Relire tous les sites d'appel de `WithGame<TResult>` dans `BattleShip.Tests` et `BattleShip.API` et vérifier si un appelant a besoin de retourner un type valeur (par exemple `Coordinate`) directement.

**Observation**
L'hypothèse était fausse : plusieurs sites d'appel voulant une valeur de type valeur ont dû contourner la contrainte. `BattlefieldGrpcServiceTests.cs` enveloppe le résultat dans un tableau à un élément avec un commentaire explicite (« WithGame requires a reference-typed result, hence... ») ; le test `LeaveOnlyOneUnplayedCellOnHumanGrid` retourne `game` lui-même comme valeur sentinelle sans rapport avec l'opération. Le contournement se répète à chaque nouveau site d'appel plutôt que d'être résolu à la source.

**Décision et justification**
Reportée, non corrigée cette story. Le contournement fonctionne et n'affecte aucun comportement observable pour l'utilisateur ; corriger la contrainte générique demanderait de retoucher une abstraction déjà largement utilisée, jugé disproportionné dans le temps disponible du TP.

**Preuves et limites**
Constat détaillé dans `_bmad-output/implementation-artifacts/epic-1-retro-2026-09-15.md` (section Findings, « Pattern divergent — récurrent, pas dans une seule story »), repris comme action proposée n°5 de la même rétrospective (non réalisée à ce jour). Limite : reportée sans échéance fixée ; à traiter seulement si un nouveau site d'appel rend le contournement trop coûteux.

---

## Revue : `IOpponentStrategy.ChooseShot(Grid)` expose toute la grille

**Proposition examinée**
La signature `IOpponentStrategy.ChooseShot(Grid)` (Story 1.3), qui reçoit la grille complète (y compris les navires non découverts) plutôt que l'historique des tirs seul.

**Hypothèse à vérifier**
Une implémentation future pourrait « tricher » en lisant directement `Grid.Ships` pour choisir son tir, puisque rien au niveau du type ne l'empêche.

**Expérience**
À l'ouverture de la Story 2.1 (`HardOpponentStrategy`, stratégie chasse/cible), relire le code de l'implémentation et vérifier si elle accède à `Grid.Ships` ou se limite à l'historique des tirs déjà connus.

**Observation**
Vérifié : `HardOpponentStrategy` n'accède jamais à `Grid.Ships`. Elle raisonne exclusivement via `Grid.GetShotOutcome(Coordinate)` et `Grid.UnsunkHits`, deux méthodes ajoutées spécifiquement en Story 2.1 pour exposer l'historique des tirs sans exposer les positions non découvertes.

**Décision et justification**
Risque concrétisé sainement en pratique (l'implémentation réelle ne triche pas), mais la garde de type reste absente : rien n'empêche techniquement une future implémentation de lire `Grid.Ships` directement. Reportée — pas de garde de type ajoutée à ce jour.

**Preuves et limites**
Commit `ebb2e3b` (Story 2.1, `HardOpponentStrategy`, `git log` vérifié). Signalé dès la revue de la Story 1.3 (`_bmad-output/implementation-artifacts/deferred-work.md`, entrée « [RÉSOLU dans Story 2.1] »), vérifié en rétrospective de l'Epic 1 (« Déjà tracé, risque atténué en pratique »). Code de référence : `BattleShip.Models/Domain/Opponent/HardOpponentStrategy.cs`, `BattleShip.Models/Domain/Grid.cs`. Limite : aucun test ne peut détecter mécaniquement un futur accès à `Grid.Ships` par une nouvelle implémentation — seule une relecture de code le vérifierait.

---

## Revue : écart Story 2.4 — `Summary.razor` (AC) vs affichage réel dans `Play.razor`

**Proposition examinée**
L'affichage des statistiques de fin de partie (Story 2.4) : l'AC de la story mentionne un composant `Summary.razor`, mais l'implémentation affiche le tableau de synthèse directement dans `Play.razor`, au moment où le statut devient `Won` ou `Lost`.

**Hypothèse à vérifier**
L'écart de découpage de composant (pas de fichier `Summary.razor` séparé) constitue un écart bloquant par rapport à l'AC de la story.

**Expérience**
Jouer une partie jusqu'à sa fin et vérifier si les statistiques des deux camps (tirs, touches, taux, durée) sont visibles à l'utilisateur, indépendamment du fichier `.razor` qui les affiche.

**Observation**
Le comportement utilisateur attendu par le cours (visibilité du récapitulatif après la fin de partie) est présent et conforme : `Play.razor` affiche le tableau de synthèse dès que `Statistics` n'est plus `null` dans `GameStateDto`. Le seul écart est le découpage de composant, pas le comportement observable.

**Décision et justification**
Acceptée : écart documenté plutôt que refactoré sans nécessité. Le cours évalue la visibilité du résultat pour l'utilisateur (diapo 46, 61 du support), pas le nom exact du fichier `.razor` qui le produit ; introduire un composant `Summary.razor` séparé maintenant serait un refactoring sans valeur fonctionnelle ajoutée, à ce stade du projet.

**Preuves et limites**
Commit `fc9e8dc` (Story 2.4, `git log` vérifié). Contrat `Statistics` testé par `BattleShip.Tests/Unit/GameTests.cs` et `BattleShip.Tests/Integration/GameEndpointsTests.cs` (101/101 tests au vert). Constat et décision documentés dans `_bmad-output/implementation-artifacts/epic-2-retro-2026-09-16.md` (Findings, « Écart d'acceptance criteria — non bloquant » ; Questions ouvertes, « Le cours exige-t-il littéralement une page Summary.razor »). Code de référence : `BattleShip.App/Pages/Play.razor`. Limite : aucun test de composant Blazor ne vérifie que le tableau apparaît réellement dans le navigateur (AD-7) — seule la compilation Razor et les tests HTTP du contrat `Statistics` sont couverts par `dotnet test`.
