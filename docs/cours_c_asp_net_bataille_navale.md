# COURS C# / ASP.NET

## [Diapo 2] PRÉSENTATION : LE DEAL
**Aujourd’hui :** une heure. Puis cinq jours en binôme.
Cette introduction présente la mission, les livrables, l’évaluation et le démarrage.

Vous conduisez le projet en autonomie : cherchez, testez et utilisez l’entraide.
Le référentiel commence diapo 17 ; les fichiers copiables accompagnent le PPT.
Dernière heure du jour 5 : un QCM individuel de 20 questions.

**Votre responsabilité**
Avancer vers un résultat démontrable, rendre les blocages visibles et justifier vos décisions.

**Avec l’IA**
Vous pouvez lui déléguer du travail. Vous restez responsables du code accepté et des preuves de vérification.

---

## [Diapo 3] PRÉSENTATION : POURQUOI C# ET .NET
*   **.NET 10** est la version LTS sortie en novembre 2025, supportée trois ans. C’est celle que vous installez.
*   Open source et multiplateforme depuis 2016 : Windows, macOS, Linux.
*   Un seul langage pour le web, le desktop, le mobile (MAUI), le jeu (Unity, Godot), le cloud et le machine learning.
*   **ASP.NET Core** permet de construire des API HTTP et des applications web en C#.
*   Un écosystème unifié : un SDK, une CLI, un gestionnaire de paquets (NuGet), un runtime.

**Concrètement, pour vous**
Vos connaissances de Java sont un point d’appui. Repérez les différences de langage, puis travaillez l’architecture, les tests et la qualité des échanges client–serveur.

---

## [Diapo 4] PRÉSENTATION : C# VS JAVA
Syntaxe très proche, conventions différentes. Le piège est là.
Les propriétés remplacent les getters et setters :

```csharp
// Java
private String name;
public String getName() { return name; }
public void setName(String v) { name = v; }

// C#
public string Name { get; set; }
```

**Les autres écarts qui comptent :**
*   `PascalCase` pour les méthodes et les propriétés publiques ; `camelCase` pour les paramètres et les variables locales.
*   Lambdas avec `=>` et non `->` ; asynchrone avec `async` / `await` et `Task<T>` plutôt que `CompletableFuture`.
*   `var` pour l’inférence de type : le typage reste statique et fort.
*   Héritage et interfaces partagent le même symbole : `public class A : B, IC`

---

## [Diapo 5] PRÉSENTATION : LA MISSION : BATAILLE NAVALE
**Ce que vous allez construire**
Une bataille navale jouable, en binôme, du navigateur jusqu’au serveur.
Quatre projets : une API ASP.NET Core, un front Blazor WebAssembly, une bibliothèque de modèles et un projet de tests.
Le socle permet de jouer contre l’ordinateur. Vous définissez les fonctionnalités qui enrichissent votre projet.

**Ce qui n’est pas demandé**
Une reproduction fidèle du jeu de plateau. Les règles, la taille de la grille, la composition de la flotte : ce sont vos décisions, tant que vous savez les justifier.

**Démonstration**
Une partie complète, de la création à la victoire.

---

## [Diapo 6] PRÉSENTATION : CE QUI EST IMPOSÉ, CE QUI EST LIBRE

**CONTRAINTES DU SOCLE**
*   API ASP.NET Core en Minimal API (.NET 10)
*   Front Blazor WebAssembly
*   Bibliothèque de modèles partagée
*   Partie complète contre l’ordinateur
*   FluentValidation sur les entrées serveur
*   gRPC fonctionnel sur au moins un échange
*   Tests métier et tests d’intégration
*   Livrables IA et historique Git exploitable

**VOS CHOIX, VOTRE RESPONSABILITÉ**
*   Règles, grille et composition de la flotte
*   Représentations et organisation du code
*   Stockage et algorithme de l’adversaire
*   Interface et expérience de jeu
*   Extensions et priorités du backlog

*Vous êtes attendus au-delà du socle. L’ambition, la pertinence et la qualité du périmètre livré sont évaluées, diapo 61.*

---

## [Diapo 7] PRÉSENTATION : L’IA EST OBLIGATOIRE ICI
Dans ce cours, l’IA fait partie de l’environnement de travail.
Copilot, ChatGPT, Claude, Rider AI, Cursor : choisissez un outil accessible aux deux membres du binôme. Aucun abonnement payant n’est exigé.
Testez l’accès avant le démarrage. Si un quota est atteint, changez d’outil ou avancez sur une tâche indépendante.

**Ce qui est évalué : votre capacité à cadrer, vérifier et décider.**
Vous pouvez utiliser l’IA pour comprendre, concevoir, coder, tester et relire.
Utilisez des exemples de données ; ne transmettez ni secrets ni données personnelles dans les prompts.
Vous devez expliquer le fonctionnement, les limites et les vérifications du code livré.

---

## [Diapo 8] PRÉSENTATION : TRAVAILLER EN BINÔME AVEC L’IA

**ORGANISEZ VOTRE COLLABORATION**
*   Répartissez les responsabilités et les tâches.
*   Choisissez vos points de synchronisation.
*   Partagez les décisions et les blocages.
*   Faites relire les changements intégrés.
*   Adaptez votre organisation aux résultats.

*Vous choisissez votre façon de travailler. Votre historique doit rendre le travail lisible.*

**ASSUMEZ LE RÉSULTAT À DEUX**
*   Chaque membre comprend le projet livré.
*   Chacun explique un parcours complet, du navigateur jusqu’au serveur.
*   Les choix structurants se défendent à deux.
*   Le code délégué à l’IA reste votre travail.

*La répartition des tâches vous appartient. La maîtrise du projet concerne chacun.*

---

## [Diapo 9] PRÉSENTATION : UNE DÉMARCHE À ADAPTER
1.  **Cadrer :** Définissez le besoin, les contraintes et le résultat attendu. Donnez à l’IA le contexte utile.
2.  **Explorer et réaliser :** Comparez les propositions, expérimentez et construisez une solution que vous comprenez.
3.  **Vérifier et décider :** Confrontez le résultat au besoin. Décidez ce que vous intégrez à partir de vos observations.
4.  **Adapter :** Révisez vos priorités quand les faits le justifient. Conservez les décisions utiles dans les livrables, diapo 11.

*La démarche vous appartient. La qualité du résultat et la solidité de vos décisions doivent être démontrables.*

---

## [Diapo 10] PRÉSENTATION : LE CODE GÉNÉRÉ DOIT FAIRE SES PREUVES
Vous recevrez du code plausible. À vous d’établir qu’il respecte votre besoin.
Compiler et réussir une démonstration ne prouvent pas tous les comportements.

**Votre travail d’ingénieur**
*   Identifier les hypothèses dont dépend la solution.
*   Choisir des expériences capables de mettre ces hypothèses en défaut.
*   Interpréter les résultats, décider et connaître les limites de vos vérifications.

**La preuve appartient à votre projet**
Une revue renvoie à un commit, un scénario précis et un résultat reproductible. Expliquez quelle erreur ce contrôle pourrait détecter.
Recopier une analyse du support ou demander à l’IA de se déclarer correcte ne constitue pas une vérification.

---

## [Diapo 11] PRÉSENTATION : VOS 3 LIVRABLES IA

1.  **PROMPTS.md : les échanges décisifs**
    Date, outil ou modèle si connu, contexte, prompt, réponse résumée, décision et preuve. Inutile de recopier toutes les conversations.
2.  **docs/adr/ : les décisions d’architecture**
    ADR signifie Architecture Decision Record. Pour chaque choix structurant : contexte, options, décision et conséquences.
3.  **REVUE-IA.md : trois revues argumentées minimum**
    Une proposition acceptée, adaptée ou rejetée : ce que vous avez vérifié, votre conclusion et un lien vers le diff ou le test. Une proposition acceptée doit aussi être étayée par un contrôle pertinent et reproductible.

*Les gabarits des diapos 56 à 58 sont aussi fournis en fichiers Markdown.*

---

## [Diapo 12] PRÉSENTATION : ÉVALUATION
**Deux composantes de l’évaluation**
*   **50 % :** le TP bataille navale, en binôme.
*   **50 % :** le QCM final, individuel, dernière heure du jour 5.

**Le projet**
Un résultat utilisable, un périmètre ambitieux et cohérent, des décisions justifiées et une maîtrise démontrable du code.

**Le QCM**
Les connaissances techniques de chaque étudiant sur les notions du cours.

*Les critères qualitatifs du projet sont présentés diapo 61. Les livrables sont précisés diapo 62.*

---

## [Diapo 13] PRÉSENTATION : LE QCM FINAL
*   50 % de la note. Dernière heure du jour 5.
*   20 questions, une seule bonne réponse par question, sans point négatif.
*   Individuel. Sans document, sans IA, sans binôme.

**Ce qui est testé**
Les repères du projet : propriétés et records C#, asynchrone, commandes dotnet, paramètres des Minimal APIs, injection de dépendances, Blazor, FluentValidation, xUnit, gRPC et CORS.

**Comment vous préparer**
Reliez chaque notion à un cas du projet : entrée, comportement, erreur possible et vérification.
Expliquez-vous les choix et les corrections en binôme. Chaque membre doit maîtriser les notions mobilisées dans le projet.

---

## [Diapo 14] PRÉSENTATION : VOUS PILOTEZ LE PROJET
**Une échéance, des contraintes, un projet à livrer**
*   Vous disposez de cinq jours, avant le QCM. Construisez un backlog ambitieux et cohérent à partir du socle.
*   Définissez les tâches, les priorités, la répartition du travail et vos points de contrôle.
*   Avec l’IA et les ressources disponibles, explorez les pistes du backlog et évaluez ce que vous pouvez livrer.
*   Faites évoluer votre plan à partir de vos résultats, des risques et du temps disponible.

**Des arbitrages à défendre**
Expliquez les fonctionnalités réalisées, les changements de priorité et les renoncements.
Choisir un périmètre limité engage votre responsabilité. L’ambition et la qualité du résultat font partie de l’évaluation.

---

## [Diapo 15] PRÉSENTATION : DÉMARREZ MAINTENANT
Dix minutes pour vérifier le démarrage. Installez les outils avant la séance.

```bash
dotnet --version
dotnet new sln -n BattleShip
dotnet new webapi -n BattleShip.API
dotnet new blazorwasm -n BattleShip.App
dotnet new classlib -n BattleShip.Models
dotnet new xunit -n BattleShip.Tests
dotnet sln add BattleShip.API BattleShip.App BattleShip.Models BattleShip.Tests
dotnet add BattleShip.API reference BattleShip.Models
dotnet add BattleShip.App reference BattleShip.Models
dotnet add BattleShip.Tests reference BattleShip.API
dotnet build
dotnet test
```

Vérifiez compilation et tests.
Créez le dépôt avec un `.gitignore` .NET et faites le premier commit.

---

## [Diapo 16] PRÉSENTATION : COMMENT UTILISER LA SUITE
Le référentiel présente les outils avec des exemples génériques. À vous de les appliquer au projet. 

**Les six parties**
*   **01 :** S’équiper et démarrer, diapo 18
*   **02 :** API, contrats, tests et validation, diapo 30
*   **03 :** Blazor, HTTP et CORS, diapo 41
*   **04 :** gRPC et gRPC-Web, diapo 47
*   **05 :** Utiliser l’IA et tracer les décisions, diapo 53
*   **06 :** Backlog, évaluation et remise, diapo 59

**Vous orienter**
*   Diagnostic de blocage : diapo 22. 
*   Construire vos vérifications : diapos 54–55.
*   Copiez les gabarits du dossier « Ressources Bataille Navale ».

---

## [Diapo 17] LE RÉFÉRENTIEL
À partir d’ici, le document se consulte. Il ne se projette pas.

---

# 01 - S’ÉQUIPER ET DÉMARRER
SDK, IDE, CLI, déblocage, et le C# essentiel (Diapo 18)

---

## [Diapo 19] 01 · S’ÉQUIPER ET DÉMARRER : INSTALLER SON ENVIRONNEMENT

**1. Le SDK .NET 10**
Téléchargez le SDK .NET 10 (LTS) sur dotnet.microsoft.com/download. Le SDK contient le compilateur, les bibliothèques et la CLI ; le runtime seul ne suffit pas pour développer.
```bash
dotnet --info     # tout : SDK, runtimes, architecture
dotnet --version  # 10.x depuis le dossier contenant global.json
```

**2. L’IDE**
*   JetBrains Rider : recommandé ici, multiplateforme, gratuit pour les étudiants.
*   Visual Studio 2026 18.0+ sous Windows, ou VS Code avec C# Dev Kit, conviennent aussi.

**3. Git**
Indispensable : votre historique de commits fait partie de la note.
```bash
git --version
dotnet new gitignore   # .gitignore adapté à .NET, à la racine
```

---

## [Diapo 20] 01 · S’ÉQUIPER ET DÉMARRER : RIDER : L’IDE RECOMMANDÉ
Gratuit pour les étudiants via la licence éducation JetBrains, et gratuit pour un usage non commercial.

**Pourquoi celui-là**
*   Le même IDE sous Windows, macOS et Linux : tout le groupe voit la même chose.
*   Analyse statique : elle signale certaines erreurs et aide à relire le code avant compilation.
*   Refactorings sérieux — renommer, extraire une méthode, introduire une interface : c’est l’outil qui transforme du code généré en code propre.
*   Débogueur, tests, client HTTP intégré (indispensable, voir « Tester son API »), Git.

**Raccourcis (keymap IntelliJ sous Windows/Linux ; à adapter sur macOS):**
*   `Double Maj` : chercher n'importe quoi dans la solution
*   `Alt+Entrée` : corriger / suggérer une action sur le code
*   `Ctrl+B` : aller à la définition
*   `Ctrl+Alt+L` : reformater le fichier
*   `Maj+F10` : exécuter
*   `Maj+F9` : déboguer
*   `Ctrl+Maj+T` : aller au test correspondant

*L’accès à l’assistant IA dépend du compte et de l’offre. Vérifiez vos droits ; un autre outil IA convient aussi.*

---

## [Diapo 21] 01 · S’ÉQUIPER ET DÉMARRER : LA CLI DOTNET
La CLI permet de reproduire les opérations sans dépendre de votre IDE.

```bash
dotnet new console -n MonProjet  # créer un projet
dotnet new sln -n MaSolution     # créer une solution
dotnet sln add MonProjet         # rattacher un projet
dotnet add A reference B         # référencer un projet depuis un autre
dotnet add package NomDuPackage  # ajouter une dépendance NuGet
dotnet build                     # compiler
dotnet run                       # compiler et exécuter
dotnet run --project BattleShip.API # lancer un projet précis
dotnet watch                     # relancer à chaque sauvegarde
dotnet test                      # exécuter les tests
dotnet format                    # reformater tout le code
dotnet new list                  # lister les modèles disponibles
```
« `dotnet new <modèle> --help` » liste les options réelles d’un modèle. À vérifier avant de croire l’IA sur une option.

---

## [Diapo 22] 01 · S’ÉQUIPER ET DÉMARRER : DIAGNOSTIQUER ET TRAITER UN BLOCAGE

**Un diagnostic avant une nouvelle tentative**
*   Lisez l’erreur entière et repérez le fichier, la ligne et le code d’erreur.
*   Comparez le résultat obtenu au comportement attendu.
*   Isolez le problème dans un test ou un exemple minimal.
*   Consultez la documentation de la bonne version ; donnez à l’IA les faits et les hypothèses déjà testées.
*   Dans le navigateur : console F12, onglet Réseau et logs serveur selon le cas.

**Commandes utiles**
```bash
dotnet build -v normal
dotnet restore
dotnet test --logger "console;verbosity=detailed"
```

**Quand le diagnostic n’avance plus**
Écrivez une reproduction et vos essais ; demandez une relecture à un autre binôme. Signalez les blocages d’accès ou d’organisation à contact@hts-learning.com. Avancez sur une tâche indépendante en attendant.

---

## [Diapo 23] 01 · S’ÉQUIPER ET DÉMARRER : VÉRIFIER UN COMPORTEMENT EN ISOLATION
Nouveauté .NET 10 : un fichier `.cs` s’exécute seul, sans projet, sans solution. Idéal pour vérifier une syntaxe ou un comportement avant de l’intégrer.

```csharp
// essai.cs
decimal[] prices = [12.50m, 3m, 7.25m];
var selected = prices.Where(price => price >= 5m);
Console.WriteLine(string.Join(", ", selected));
Console.WriteLine($"Total : {prices.Sum()}");
```
```bash
dotnet run --file essai.cs
```
Placez le fichier HORS du dossier de vos projets, et gardez l’option `--file` : s’il y a un `.csproj` dans le dossier courant, « `dotnet run essai.cs` » lance le projet et passe votre fichier en argument.

**Ajouter un package sans projet**
```csharp
#r "nuget: FluentValidation, 11.9.0" // exploration ; fixez ensuite la version
using FluentValidation;
```
Servez-vous-en pour confronter une affirmation de l’IA à une exécution réelle.

---

## [Diapo 24] 01 · S’ÉQUIPER ET DÉMARRER : LE C# ESSENTIEL — TYPES ET OBJETS

```csharp
// Le typage reste statique avec var.
int quantity = 3;
var name = "Cahier";
string? description = null;

public class Product
{
    public required string Name { get; init; }
    public decimal Price { get; set; }
    public bool IsFree => Price == 0;
}

// Un record positionnel fournit notamment l’égalité par valeur.
public record ProductDto(int Id, string Name, decimal Price);
```
Un record peut être mutable ; `init` ne rend pas immuable le contenu d’une collection. Choisissez vos types selon les données et les responsabilités.

---

## [Diapo 25] 01 · S’ÉQUIPER ET DÉMARRER : LE C# ESSENTIEL — COLLECTIONS ET LINQ

```csharp
var products = new List<Product>();
var byName = new Dictionary<string, Product>();
var dimensions = (Width: 12, Height: 8);

var affordable = products.Where(p => p.Price <= 20m).ToList();
var total = products.Sum(p => p.Price);
var first = products.FirstOrDefault(p => p.Name == "Cahier");
var hasFree = products.Any(p => p.IsFree);

var label = total switch
{
    0 => "vide",
    < 50 => "petit montant",
    _ => "montant élevé"
};

if (first is { Price: > 0 }) { /* produit payant trouvé */ }
```

---

## [Diapo 26] 01 · S’ÉQUIPER ET DÉMARRER : LE C# ESSENTIEL — ASYNCHRONE ET ERREURS

```csharp
public interface IProductStore
{
    Task<Product?> FindAsync(int id);
}

// Constructeur primaire et dépendance explicite.
public sealed class ProductLookup(IProductStore store)
{
    public async Task<Product> GetAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentOutOfRangeException(nameof(id));
            
        return await store.FindAsync(id)
            ?? throw new KeyNotFoundException("Produit inconnu");
    }
}
// Task<T> représente le résultat d’une opération asynchrone.
```

---

## [Diapo 27] 01 · S’ÉQUIPER ET DÉMARRER : CE QUI EST NOUVEAU EN C# 14
C# 14 : des formes facultatives, à choisir pour la lisibilité du code.

```csharp
// 'field' : accès au champ privé généré d’une propriété.
public string Label
{
    get => field;
    set => field = !string.IsNullOrWhiteSpace(value)
        ? value
        : throw new ArgumentException("Libellé requis", nameof(value));
} = "Catalogue";

// Affectation null-conditionnelle.
product?.Price = 12m;

// Membres d’extension.
public static class TextExtensions
{
    public static extension(string text)
    {
        public bool IsBlank => string.IsNullOrWhiteSpace(text);
    }
}
```

---

## [Diapo 28] 01 · S’ÉQUIPER ET DÉMARRER : CRÉER LA SOLUTION

```bash
# Copier global.json à la racine, puis vérifier dotnet --version
dotnet new sln -n BattleShip
dotnet new webapi -n BattleShip.API      # Minimal API par défaut
dotnet new blazorwasm -n BattleShip.App
dotnet new classlib -n BattleShip.Models
dotnet new xunit -n BattleShip.Tests
dotnet sln add BattleShip.API BattleShip.App BattleShip.Models BattleShip.Tests
dotnet add BattleShip.API reference BattleShip.Models
dotnet add BattleShip.App reference BattleShip.Models
dotnet add BattleShip.Tests reference BattleShip.API
dotnet build
dotnet test
```
*   Le SDK .NET 10 crée une Minimal API et une solution `.slnx`. `global.json` évite de sélectionner une autre version majeure installée.
*   HTTPS local : vérifiez le certificat avec `dotnet dev-certs https --trust`, selon votre système et votre navigateur.
*   Lancez API et front avec le profil `https` ; relevez les ports pour BaseAddress et CORS. Les ports 7001 et 7043 sont des exemples.
*   `BattleShip.Models` ne dépend d’aucun autre projet. Le moteur reste indépendant de HTTP, JSON et gRPC.

---

## [Diapo 29] 01 · S’ÉQUIPER ET DÉMARRER : LES RESPONSABILITÉS À ORGANISER

*   **Navigateur :** interface Blazor WebAssembly | échanges HTTP/JSON et gRPC-Web
*   *(vers)*
*   **Serveur :** API ASP.NET Core et règles du jeu
*   **Modèles partagés :** données échangées entre les projets
*   **Tests :** comportements métier et contrats d’intégration

**À vous de concevoir l’organisation interne**
*   Définissez les responsabilités, les dépendances et la gestion de l’état.
*   Votre logique métier doit pouvoir être vérifiée indépendamment de l’interface et des transports.
*   Expliquez les choix structurants et leurs conséquences dans vos ADR.

---

# 02 - L’API
Minimal API, injection de dépendances, spécifications, tests (Diapo 30)

---

## [Diapo 31] 02 · L’API : MINIMAL API : LES BASES

```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

ProductDto[] products = [new(1, "Stylo", 2.50m)];

app.MapGet("/products", () => products);

app.MapGet("/products/{id:int}", IResult (int id) =>
    products.FirstOrDefault(p => p.Id == id) is { } product
        ? Results.Ok(product)
        : Results.NotFound());

app.Run();

public record ProductDto(int Id, string Name, decimal Price);
```
Cet exemple expose un catalogue de démonstration en lecture seule.
`MapGet` associe une route à un traitement. Le résultat détermine le corps et le statut HTTP.
Vous concevez les opérations et les contrats de votre bataille navale.

---

## [Diapo 32] 02 · L’API : D’OÙ VIENNENT LES PARAMÈTRES
Pour les endpoints POST montrés ici, ASP.NET Core peut déduire la source des paramètres. Les attributs la rendent explicite.

```csharp
app.MapPost("/products/{id:int}/preview", (
    [FromRoute] int id,
    [FromBody] ProductInput input,
    [FromQuery] bool? verbose,
    [FromServices] ILoggerFactory logs) =>
{
    logs.CreateLogger("Preview").LogInformation("Produit {Id}", id);
    return Results.Ok(new { id, input.Name, verbose });
});
```
Dans cet exemple de liaison des paramètres :
*   `id` provient de la route ; `input` provient du corps JSON.
*   `verbose` provient de la chaîne de requête ; `logs` vient de l’injection.
*   `ProductInput` est défini dans les ressources génériques.

La liaison des paramètres et leur validation sont deux responsabilités distinctes.

---

## [Diapo 33] 02 · L’API : INJECTION DE DÉPENDANCES

```csharp
builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);
builder.Services.AddScoped<RequestContext>();
builder.Services.AddTransient<LabelFormatter>();

public sealed class RequestContext
{
    public Guid Id { get; } = Guid.NewGuid();
}

public sealed class LabelFormatter
{
    public string Format(string value) => value.Trim();
}
```
**Durées de vie côté serveur**
*   **Singleton :** une instance pour l’application.
*   **Scoped :** une instance par portée, généralement une requête HTTP.
*   **Transient :** une instance à chaque résolution.

Choisissez les durées de vie selon l’état et les dépendances de vos services.

---

## [Diapo 34] 02 · L’API : RÉPONSES ET CODES HTTP

```csharp
// ProductInput et son validateur : diapo 40 et ressources.
app.MapPost("/products/check", async Task<IResult> (
    ProductInput input, 
    IValidator<ProductInput> validator) =>
{
    var check = await validator.ValidateAsync(input);
    if (!check.IsValid)
        return TypedResults.ValidationProblem(check.ToDictionary());
        
    return TypedResults.Ok(input);
});
```
**Un statut exprime le résultat du traitement**
*   200 : succès ; 201 : ressource créée ; 204 : succès sans contenu.
*   400 : requête invalide ; 404 : ressource introuvable ; 409 : conflit avec l’état courant.

Définissez les réponses de votre API et vérifiez leur cohérence avec les comportements annoncés.

---

## [Diapo 35] 02 · L’API : VOIR ET TESTER SON API
Le modèle .NET 10 n’embarque plus Swagger UI. Il expose un document OpenAPI, et vous choisissez comment le consommer.

```csharp
builder.Services.AddOpenApi(); // avant builder.Build()
if (app.Environment.IsDevelopment()) app.MapOpenApi();
```
**Un fichier .http versionné pour les essais manuels**
```http
@api = https://localhost:7001

### Lire le catalogue de démonstration
GET {{api}}/products

### Vérifier un produit
POST {{api}}/products/check
Content-Type: application/json

{ "name": "Stylo", "price": 2.50 }
```
Le fichier `api.http` accompagne ces exemples génériques. Adaptez l’adresse au serveur lancé. Construisez les requêtes de votre projet à partir de votre propre contrat. Les essais manuels complètent vos assertions automatisées.

---

## [Diapo 36] 02 · L’API : SPÉCIFICATION 1 — LE MOTEUR DE JEU

**Un moteur de jeu vérifiable**
Les règles doivent pouvoir être exécutées et testées indépendamment des transports et de l’interface.

**Comportements attendus**
*   Créer deux grilles et placer les flottes aléatoirement.
*   Respecter les tailles et les formes choisies, sans chevauchement ni débordement.
*   Résoudre les tirs et identifier la fin de partie ainsi que le gagnant.
*   Garder secrètes les positions adverses non découvertes.
*   Un coup refusé ne modifie pas la partie ; rejouer une case ne compte pas comme un nouveau coup.

**Vos choix de conception**
Définissez les représentations, les algorithmes et les invariants. Les tests doivent établir le respect de vos règles.

---

## [Diapo 37] 02 · L’API : SPÉCIFICATION 2 — LE CONTRAT D’API

**Concevez les échanges entre le navigateur et le serveur**
Définissez les opérations nécessaires, leurs entrées, leurs réponses et leurs erreurs.
Le client doit pouvoir créer une partie, connaître son état et faire évoluer le jeu.

**Un contrat explicite**
Un DTO (Data Transfer Object) représente les données échangées à la frontière d’un service.
Choisissez les types, les informations publiques et la représentation des états possibles.
Les échanges doivent respecter les règles de visibilité des informations du jeu.

**Une décision d’équipe**
Faites évoluer ensemble le contrat, son implémentation et ses vérifications. Documentez les décisions structurantes.

---

## [Diapo 38] 02 · L’API : SPÉCIFICATION 3 — LA BOUCLE DE JEU

**Un déroulement cohérent avec vos règles**
Une action autorisée fait évoluer la partie et produit un résultat compréhensible par le joueur.
Le joueur et l’ordinateur jouent selon l’alternance définie. Aucun nouveau coup n’est joué après la fin de partie.
L’interface dispose des informations nécessaires pour représenter le résultat et l’état courant.

**Un adversaire que vous concevez**
Définissez sa stratégie et vérifiez qu’il respecte les mêmes règles de validité des coups.
La difficulté et la qualité de son comportement peuvent enrichir votre backlog.

**Une cohérence à démontrer**
Identifiez les situations qui peuvent compromettre le déroulement du jeu. Choisissez vos vérifications et justifiez ce qu’elles établissent.

---

## [Diapo 39] 02 · L’API : TESTER UN COMPORTEMENT AVEC XUNIT

```csharp
public class PriceTests
{
    [Fact]
    public void Une_taxe_de_20_pour_cent_est_appliquee()
    {
        var result = PriceMath.WithTax(20m, 0.20m);
        Assert.Equal(24m, result);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Un_prix_negatif_est_refuse(int net)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => PriceMath.WithTax(net, 0.20m));
    }
}
```
`Fact` décrit un cas ; `Theory` reçoit plusieurs jeux de données. Déduisez vos tests des règles, des contrats et des risques du jeu.

---

## [Diapo 40] 02 · L’API : FLUENTVALIDATION
FluentValidation exprime des règles de validation composées et testables. Cet exemple porte sur un produit ; concevez les règles adaptées aux entrées de votre projet.

```bash
dotnet add Catalogue.API package FluentValidation
```
```csharp
public record ProductInput(string Name, decimal Price);

public sealed class ProductInputValidator : AbstractValidator<ProductInput>
{
    public ProductInputValidator()
    {
        RuleFor(p => p.Name).NotEmpty().MaximumLength(100);
        RuleFor(p => p.Price).GreaterThanOrEqualTo(0);
    }
}

// Program.cs, avant builder.Build().
builder.Services.AddScoped<IValidator<ProductInput>, ProductInputValidator>();
// Appel explicite de ValidateAsync dans l’endpoint : diapo 34.
```

---

# 03 - LE FRONT BLAZOR
Composants, cycle de vie, appels HTTP et CORS (Diapo 41)

---

## [Diapo 42] 03 · LE FRONT BLAZOR : BLAZOR : LES BASES
Blazor WebAssembly exécute votre C# dans le navigateur. Un composant est un fichier `.razor` : du HTML, des expressions `@`, et un bloc `@code`.

```razor
@page "/selection"

<h3>Choisir un produit</h3>
@foreach (var product in products)
{
    <button @onclick="() => Select(product.Id)">@product.Name</button>
}
<p>Produit sélectionné : @selectedId</p>

@code {
    private ProductDto[] products =
        [new(1, "Stylo", 2.50m), new(2, "Cahier", 4m)];
    private int selectedId;
    
    private void Select(int id) => selectedId = id;
}
```
Un composant associe des données, un rendu et des événements. Vous choisissez le découpage et les interactions de votre interface de jeu.

---

## [Diapo 43] 03 · LE FRONT BLAZOR : BLAZOR : CYCLE DE VIE ET RENDU

```razor
@page "/catalogue"
@using System.Net.Http.Json
@inject HttpClient Http

@if (products is null)
{
    <p>Chargement…</p>
}
else
{
    <p>@products.Length produits disponibles</p>
}

@code {
    private ProductDto[]? products;
    
    protected override async Task OnInitializedAsync()
        => products = await Http.GetFromJsonAsync<ProductDto[]>("products");
}
```
`OnInitializedAsync` prépare le composant ; `OnParametersSetAsync` accompagne les changements de paramètres ; `OnAfterRenderAsync` intervient après le rendu.
Les événements du composant participent au cycle de rendu. Définissez les états de chargement, de succès et d’échec de votre interface.

---

## [Diapo 44] 03 · LE FRONT BLAZOR : BLAZOR : APPELER L’API

```csharp
// Enregistrement dans le Program.cs du client de démonstration.
builder.Services.AddScoped(_ => new HttpClient
{
    BaseAddress = new Uri("https://localhost:7001/")
});

// Dans un composant disposant de HttpClient Http et du champ message.
private async Task CheckAsync()
{
    using var response = await Http.PostAsJsonAsync("products/check",
        new ProductInput("Stylo", 2.50m));
        
    message = response.IsSuccessStatusCode
        ? "Produit valide"
        : $"Vérification refusée : HTTP {(int)response.StatusCode}";
}
```
`System.Net.Http.Json` fournit les extensions de sérialisation JSON. L’adresse du serveur dépend de l’environnement.
Choisissez où votre client effectue les appels et conserve son état. Gérez les réponses métier et les échecs de communication selon l’expérience attendue.

---

## [Diapo 45] 03 · LE FRONT BLAZOR : CORS : LES ÉCHANGES ENTRE ORIGINES

**Le navigateur applique une politique d’origine**
Une origine associe un schéma, un hôte et un port. Deux applications servies sur des ports différents ont des origines distinctes.
CORS (Cross-Origin Resource Sharing) permet au serveur d’autoriser certains échanges entre origines.

**Une politique à définir**
Les origines, les méthodes et les en-têtes autorisés dépendent des échanges prévus.
ASP.NET Core propose des services et un middleware pour appliquer cette politique.

**Votre configuration**
Établissez les besoins du client et du serveur, puis configurez et vérifiez les échanges dans le navigateur.
CORS ne remplace pas l’authentification ni les autorisations d’accès. Consultez la documentation ASP.NET Core pour les options de configuration.

---

## [Diapo 46] 03 · LE FRONT BLAZOR : SPÉCIFICATION 4 — L’INTERFACE

**Une partie utilisable depuis le navigateur**
*   Créer une partie et afficher les informations nécessaires au joueur.
*   Présenter les deux grilles avec les informations que le joueur est autorisé à connaître.
*   Permettre de jouer, rendre les résultats visibles et annoncer la fin de partie.

**Une expérience cohérente**
Le joueur comprend l’état courant, les actions possibles et les éventuels refus.
Un incident de communication est pris en charge sans rendre l’interface inutilisable.

**Vos décisions d’interface**
Choisissez les composants, la gestion de l’état, les interactions et la présentation visuelle. Expliquez comment ils servent l’expérience de jeu.

---

# 04 - GRPC
Contrat binaire entre l’API et le front (Diapo 47)

---

## [Diapo 48] 04 · GRPC : GRPC : POURQUOI ET COMMENT

**Un contrat partagé entre client et serveur**
gRPC échange des messages définis dans un fichier `.proto`. Des outils génèrent les types et les services.
Les caractéristiques du format et du transport s’apprécient selon les besoins et les mesures du projet.
Depuis le navigateur, utilisez gRPC-Web pour accéder au service.

**Ce qui est exigé**
Au moins un échange fonctionnel entre le front et l’API, avec une réponse et une erreur attendue démontrables.

**Votre choix d’architecture**
Choisissez l’opération concernée et concevez son contrat. Expliquez l’articulation avec les autres échanges du projet.
Votre ADR présente les options examinées, votre décision et ses conséquences.

---

## [Diapo 49] 04 · GRPC : LE CONTRAT .PROTO

```protobuf
syntax = "proto3";
option csharp_namespace = "Catalogue.Grpc";
package catalogue;

service Catalogue {
  rpc FindProduct (ProductQuery) returns (ProductReply);
}

message ProductQuery {
  int32 id = 1;
}

message ProductReply {
  int32 id = 1;
  string name = 2;
}
```
Exemple générique fourni dans `Exemples/catalogue.proto`. Les numéros identifient les champs du contrat ; conservez leur compatibilité lors des évolutions.

---

## [Diapo 50] 04 · GRPC : GRPC : VALIDER ET RÉPONDRE

```csharp
public sealed class CatalogueGrpcService(IValidator<ProductQuery> validator)
    : global::Catalogue.Grpc.Catalogue.CatalogueBase
{
    public override async Task<ProductReply> FindProduct(
        ProductQuery request, ServerCallContext context)
    {
        var check = await validator.ValidateAsync(request, context.CancellationToken);
        if (!check.IsValid)
            throw new RpcException(new Status(StatusCode.InvalidArgument, check.ToString()));
            
        if (request.Id != 1)
            throw new RpcException(new Status(StatusCode.NotFound, "Produit inconnu"));
            
        return new ProductReply { Id = 1, Name = "Stylo" };
    }
}

public sealed class ProductQueryValidator : AbstractValidator<ProductQuery>
{
    public ProductQueryValidator() => RuleFor(p => p.Id).GreaterThan(0);
}
```
Exemple de catalogue fixe : un produit disponible. Usings et enregistrement du validateur dans les ressources.
Définissez les validations et les réponses adaptées à votre propre contrat.

---

## [Diapo 51] 04 · GRPC : GRPC : CONFIGURER LES DEUX PROJETS
Extraits pour `Catalogue.API` et `Catalogue.App` ; contrat commun dans `Protos/catalogue.proto`.

```bash
dotnet add Catalogue.API package Grpc.AspNetCore
dotnet add Catalogue.API package Grpc.AspNetCore.Web
dotnet add Catalogue.App package Grpc.Net.Client
dotnet add Catalogue.App package Grpc.Net.Client.Web
dotnet add Catalogue.App package Google.Protobuf
dotnet add Catalogue.App package Grpc.Tools
```

```xml
<!-- Dans API.csproj : Server ; dans App.csproj : Client -->
<ItemGroup>
  <Protobuf Include="../Protos/catalogue.proto" GrpcServices="Server" />
</ItemGroup>
<!-- Grpc.Tools est un outil de build : PrivateAssets="all". -->
```

```csharp
// Enregistrements avant builder.Build().
builder.Services.AddGrpc();
builder.Services.AddScoped<IValidator<ProductQuery>, ProductQueryValidator>();

// Après builder.Build().
app.UseGrpcWeb();
app.MapGrpcService<CatalogueGrpcService>().EnableGrpcWeb();
```
Complétez la configuration selon votre hébergement et vos origines. La documentation gRPC-Web est liée à cette diapo.

---

## [Diapo 52] 04 · GRPC : GRPC : CRÉER LE CLIENT

```csharp
// Exemple d’appel autonome ; packages de la diapo 51.
using var channel = GrpcChannel.ForAddress("https://localhost:7001",
    new GrpcChannelOptions
    {
        HttpHandler = new GrpcWebHandler(new HttpClientHandler())
    });

var client = new global::Catalogue.Grpc.Catalogue.CatalogueClient(channel);
var reply = await client.FindProductAsync(new ProductQuery { Id = 1 });
Console.WriteLine(reply.Name);
```
**Intégrer le client dans votre application**
Le client généré expose les opérations du contrat. Organisez son utilisation et la durée de vie de ses dépendances.
Définissez le comportement de l’interface quand l’appel réussit ou échoue.
Le fonctionnement dans le navigateur fait partie de vos vérifications. Consultez la documentation gRPC-Web pour la configuration complète.

---

# 05 - L’IA EN PRATIQUE
Démarche de vérification et structures des livrables (Diapo 53)

---

## [Diapo 54] 05 · L’IA EN PRATIQUE : VÉRIFIER UNE PROPOSITION TECHNIQUE

**Votre contexte de travail**
Identifiez la version réellement utilisée, le style d’API imposé et les conventions du dépôt.
Confrontez la proposition à ces contraintes avant de l’intégrer.

**Une référence vérifiable**
Pour une API ou une dépendance proposée, retrouvez sa documentation et reproduisez le comportement sur lequel vous vous appuyez.
Une référence ne prouve pas à elle seule que l’API est bien employée dans votre contexte.

**Votre décision**
Comparez les options utiles. Expliquez votre choix au regard du besoin, de la lisibilité et des contraintes du projet.
La nouveauté d’une syntaxe et le ton assuré du modèle ne démontrent ni la pertinence ni la correction de la solution.

---

## [Diapo 55] 05 · L’IA EN PRATIQUE : CONSTRUIRE VOS PROPRES VÉRIFICATIONS

**Choisir un risque réel de votre projet**
À partir de vos règles et de votre architecture, cherchez ce qui pourrait être faux malgré une démonstration réussie.
Décrivez le résultat attendu avant d’exécuter le scénario choisi.

**Concevoir une expérience qui peut échouer**
Votre vérification doit pouvoir distinguer une implémentation correcte d’une implémentation fautive. Expliquez comment.
Réduisez le scénario pour isoler le comportement et permettre sa reproduction.

**Conclure à partir des observations**
Distinguez ce que vous avez constaté, ce que vous supposez et ce qui reste à vérifier.
Si vous corrigez le code, vérifiez que votre contrôle détectait le défaut avant correction et réussit après.
Une revue se défend avec les faits du projet. Le nombre de tests ou la longueur du journal ne suffisent pas.

---

## [Diapo 56] 05 · L’IA EN PRATIQUE : GABARIT — PROMPTS.md
À la racine du dépôt. Une entrée par prompt qui a compté — pas tous, les décisifs.

```markdown
## Date et sujet de l’échange
**Outil / modèle** : celui réellement utilisé, si connu.
**Contexte** : besoin, contraintes et code concerné.
**Prompt** : votre demande effective.
**Réponse résumée** : proposition et hypothèses de l’IA.
**Décision** : acceptée, adaptée ou rejetée. Expliquez pourquoi cette décision répond à votre besoin.
**Vérification** : scénario ou commande, résultat attendu, résultat observé et portée du contrôle.
**Preuve** : lien vers le commit et les éléments reproductibles.
```
Consignez vos échanges réels ; le gabarit ne fournit pas l’analyse. Chaque entrée éclaire une décision réelle et les éléments qui l’étayent.

---

## [Diapo 57] 05 · L’IA EN PRATIQUE : GABARIT — ADR
Un ADR par choix structurant ; conservez l’historique et actualisez son statut.

```markdown
# ADR numéro : intitulé de la décision

## Statut et date
Proposé, accepté ou remplacé ; référence de l’ADR suivant si nécessaire.

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
```

---

## [Diapo 58] 05 · L’IA EN PRATIQUE : GABARIT — REVUE-IA.md
Trois revues minimum : propositions acceptées, adaptées ou rejetées, avec des preuves réelles.

```markdown
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
```
Après correction, comparez le résultat avant et après le changement.

---

# 06 - RENDU & ÉVALUATION
Ambition du projet, évaluation et remise (Diapo 59)

---

## [Diapo 60] 06 · RENDU & ÉVALUATION : BACKLOG : VOUS FIXEZ L’AMBITION

**Le socle constitue votre point de départ**
Avec l’IA et les ressources disponibles, explorez les pistes du backlog et construisez un projet ambitieux.

**Des pistes ouvertes**
Multijoueur, adversaire plus élaboré, sauvegarde, historique, statistiques, personnalisation, accessibilité, déploiement… Proposez aussi vos propres idées.

**Un périmètre à défendre**
Choisissez vos priorités. Expliquez les fonctionnalités réalisées, celles écartées et les raisons de ces arbitrages.
L’ambition, la pertinence et la qualité du résultat sont évaluées. Choisir de s’en tenir au minimum engage votre responsabilité et sera apprécié comme tel.
Chaque fonctionnalité livrée doit être intégrée, vérifiée et comprise par le binôme.

---

## [Diapo 61] 06 · RENDU & ÉVALUATION : CE QUI SERA ÉVALUÉ

**Les critères du projet**
*   **Fonctionnement :** partie complète et respect des contraintes techniques.
*   **Qualité du code :** responsabilités, lisibilité, cohérence et gestion des erreurs.
*   **Maîtrise de l’IA :** analyse critique, décisions justifiées et vérifications probantes.
*   **Tests et collaboration :** contrôles pertinents, relectures et historique Git exploitable.
*   **Extensions :** ambition et pertinence du périmètre, intégration et finition.

**Des choix dont vous répondez**
Le socle fonctionnel constitue la base attendue. Le périmètre complémentaire choisi et réalisé fait partie de l’appréciation du projet.
Défendez vos priorités et vos renoncements avec les faits du projet. Chaque membre doit comprendre le résultat livré et ses limites.

---

## [Diapo 62] 06 · RENDU & ÉVALUATION : LIVRABLES & REMISE

**Un dépôt GitHub par binôme**
*   Les quatre projets, les éventuels projets supplémentaires et les tests
*   `README.md` : noms, lancement, fonctionnalités, arbitrages du backlog et limites
*   `PROMPTS.md` : les échanges décisifs et les preuves de vérification
*   `docs/adr/` : les décisions d’architecture
*   `REVUE-IA.md` : trois revues argumentées minimum
*   Un historique Git avec des changements relus par le binôme

**Remise : avant le début du QCM, jour 5**
Envoyez le lien du dépôt et le hash du commit à contact@hts-learning.com. Donnez l’accès au correcteur si le dépôt est privé. Déposez également le `README.md` sur votre espace étudiant pour marquer la date de remise.
Seul ce commit, poussé avant le début du QCM, est évalué.
Faites lancer le projet par un autre binôme en suivant uniquement le `README`.

---

## [Diapo 63] 06 · RENDU & ÉVALUATION : CHECKLIST AVANT DE RENDRE

*   [ ] Le README suffit à lancer le projet avec les prérequis indiqués
*   [ ] Une partie complète se joue, y compris la fin et la création d’une autre partie
*   [ ] Un échange gRPC-Web et une erreur attendue sont démontrables
*   [ ] Les entrées HTTP et gRPC sont validées ; les règles sont vérifiées côté serveur
*   [ ] Les tests passent et détectent des règles violées, pas seulement un succès nominal
*   [ ] PROMPTS.md, ADR et trois revues IA sont à jour et reliés aux preuves
*   [ ] Les changements ont été relus ; les commits identifient le travail effectué
*   [ ] Chaque membre explique le fonctionnement, le périmètre choisi et les limites du projet
*   [ ] Le code généré automatiquement est identifié et son rôle est compris

Vous êtes responsables du résultat livré, quel que soit l’outil utilisé.

---

## [Diapo 64] 06 · RENDU & ÉVALUATION : OÙ CHERCHER DE L’AIDE

**La documentation de référence**
*   **.NET :** langage, bibliothèques et commandes CLI
*   **ASP.NET Core :** API, Blazor et gRPC ; sélectionner la version .NET 10
*   **NuGet :** packages, versions et dépendances
*   **ASP.NET Core sur GitHub :** code source et problèmes connus

**Les ressources jointes**
« Ressources Bataille Navale » : référentiel texte, `global.json`, exemples génériques et gabarits Markdown.

**Pour vérifier une proposition**
Confrontez l’API citée à la documentation, puis à un exemple exécuté. Conservez le test lorsqu’il protège une règle utile.

Organisation ou accès bloqué : contact@hts-learning.com. Joignez un diagnostic ; utilisez d’abord le protocole diapo 22.