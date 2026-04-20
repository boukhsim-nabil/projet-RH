# 🎓 Guide de Soutenance — Concepts C# illustrés par le projet SUPMTI RH

> Pour chaque concept : l'emplacement dans le projet, le code exact, et le discours à tenir face au professeur.

---

## Concept 1 — La méthode `Main` (Point d'entrée du serveur Web)

### 📍 Emplacement
```
Program.cs — lignes 61 à 136
```

### 💻 Code
```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<ApplicationUser>(...)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=AfterLogin}/{id?}");

app.Run(); // ← démarre le serveur HTTP, boucle infinie
```

### 🎙️ Discours
> "En C# classique, le point d'entrée d'un programme est la méthode `static void Main()`.
> Dans notre projet ASP.NET Core, ce rôle est joué par `Program.cs`.
> Depuis C# 9, on peut écrire directement du code de niveau supérieur sans déclarer `Main` explicitement — le compilateur le génère pour nous.
>
> Ce fichier se décompose en trois phases :
> D'abord, la **phase de configuration** : on enregistre tous les services dont l'application a besoin — la base de données, le système d'authentification Identity, les contrôleurs MVC, SignalR pour les notifications temps réel.
> Ensuite, la **phase de construction** avec `builder.Build()` qui assemble tout.
> Enfin, la **phase de démarrage** : on branche les middlewares dans l'ordre — les fichiers statiques, le routage, l'authentification, l'autorisation — puis `app.Run()` lance le serveur HTTP.
>
> La différence fondamentale avec une application console : `app.Run()` ne se termine jamais. Le programme reste en vie en permanence, en attente de requêtes HTTP entrantes. C'est la boucle infinie du serveur Web."

---

## Concept 2 — Variables, Types de données et Conversions

### 📍 Emplacement
```
Areas/Identity/Data/ApplicationUser.cs — lignes 13 à 43
Controllers/PayRollController.cs       — lignes 33 à 37
Controllers/AttendanceController.cs    — lignes 452 à 453
```

### 💻 Code

**Le modèle de données — diversité des types :**
```csharp
// ApplicationUser.cs
public class ApplicationUser : IdentityUser
{
    // Types valeur entiers — nullable avec le "?"
    public int?      empid        { get; set; }  // peut être null
    public int?      salary       { get; set; }  // salaire mensuel

    // Types référence — chaînes de caractères
    public string?   name         { get; set; }  // nom complet
    public string?   status       { get; set; }  // "Active" ou "Inactive"
    public string?   cnic         { get; set; }  // numéro de carte d'identité

    // Type structuré date
    public DateTime? joining_date { get; set; }  // date d'embauche
    public DateTime? cnic_expiry  { get; set; }  // expiration du CNIC

    // Type collection
    public List<EmpDocModel>? empDocs { get; set; }  // documents joints
}
```

**Conversion de types dans le contrôleur :**
```csharp
// PayRollController.cs — l'utilisateur envoie "2025-01" depuis le formulaire HTML
string monthyear = "2025-01";  // string reçue du formulaire

// string → DateTime (conversion explicite avec format imposé)
DateTime date = DateTime.ParseExact(monthyear, "yyyy-MM", CultureInfo.InvariantCulture);

// DateTime → string (formatage)
string monthName = date.ToString("MMMM", CultureInfo.InvariantCulture); // → "January"
string year      = date.ToString("yyyy", CultureInfo.InvariantCulture);  // → "2025"

// Concaténation + conversion en majuscules
string result = (monthName + year).ToUpper(); // → "JANUARY2025"
```

**Cast explicite pour éviter la division entière :**
```csharp
// AttendanceController.cs — calcul du pourcentage de présence
double todaypercentage = ((double)todaytime.Hours / 9) * 100;
//                        ↑ sans ce cast : 8/9 = 0 (division entière en C#)

int todayintValue = (int)todaypercentage; // troncature : 87.5 → 87
```

### 🎙️ Discours
> "C# est un langage fortement typé : chaque variable a un type précis, connu dès la compilation.
>
> Dans notre modèle `ApplicationUser`, on voit la coexistence de plusieurs familles de types.
> Les **types valeur** comme `int` et `DateTime` stockent directement la donnée en mémoire.
> Les **types référence** comme `string` et `List<T>` stockent une adresse vers la donnée.
> Le point d'interrogation — `int?`, `string?`, `DateTime?` — crée un type **nullable** : la propriété peut valoir `null` si l'information n'est pas encore renseignée. C'est essentiel dans notre contexte RH car un employé peut ne pas encore avoir de date de résignation, par exemple.
>
> Dans le contrôleur de paie, on voit une conversion chaînée : la chaîne `'2025-01'` reçue du formulaire HTML est convertie en `DateTime` via `ParseExact` — si le format est incorrect, une `FormatException` est lancée. Puis ce `DateTime` est reformaté en `'JANUARY2025'` pour correspondre au format stocké en base de données.
>
> Le cast `(double)` avant la division est un exemple classique de piège C# : sans lui, la division de deux entiers donne un entier — `8 / 9` vaut `0`, pas `0.88`. Ce cast force une division en virgule flottante."

---

## Concept 3 — Tableaux et Collections (avec LINQ)

### 📍 Emplacement
```
Program.cs                      — ligne 100
Controllers/PayRollController.cs — lignes 39 à 47
Controllers/LeaveApplyController.cs — lignes 69 à 76
Controllers/EmployeeController.cs — lignes 354 à 368
```

### 💻 Code

**Tableau `string[]` dans un `foreach` :**
```csharp
// Program.cs — créer les 5 rôles au démarrage si absents
foreach (var roleName in new[] { "admin", "HR", "HOD", "Employee", "Viewer" })
{
    if (!await roleManager.RoleExistsAsync(roleName))
        await roleManager.CreateAsync(new IdentityRole(roleName));
}
// new[] { ... } = tableau de strings inféré par le compilateur
```

**`List<T>` avec LINQ chaîné :**
```csharp
// PayRollController.cs — récupérer la liste de paie d'un mois
List<PayRollModel> payroll = await _context.payRolls
    .Include(c => c.employee)                              // JOIN employé
    .Where(c => c.monthYear == result                      // filtre mois
             && c.employee.company.Id == company)          // filtre entreprise
    .OrderBy(c => c.employee.departId)                     // tri 1
    .ThenBy(c => c.employee.joining_date)                  // tri 2
    .ToListAsync();                                        // exécution → List<>
```

**LINQ avec `Sum()` et `Contains()` :**
```csharp
// LeaveApplyController.cs — jours de congés restants
ViewBag.RL =
    // Quota total des 3 types de congés
    (_context.leaveTypes
        .Where(c => new[] { "Annual Leave", "Sick Leave", "Casual Leave" }
                        .Contains(c.type)   // équivalent SQL : WHERE type IN (...)
                 && c.companyId == user.companyId)
        .Sum(c => c.days))
    -
    // Jours déjà consommés et approuvés
    (_context.leaveApplies
        .Where(c => c.empId == user.Id
                 && c.status   == "Approve"
                 && c.hrstatus == "Approve")
        .Sum(c => c.days));
```

**`Dictionary<string, string>` — table de correspondance :**
```csharp
// EmployeeController.cs — types MIME pour le téléchargement de fichiers
private Dictionary<string, string> GetMimeTypes()
{
    return new Dictionary<string, string>
    {
        { ".pdf",  "application/pdf"  },
        { ".docx", "application/vnd.ms-word" },
        { ".png",  "image/png" },
        { ".jpg",  "image/jpeg" },
        { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" }
    };
    // Accès O(1) : types[".pdf"] → "application/pdf"
}
```

### 🎙️ Discours
> "En C#, les collections sont au cœur du développement professionnel. Le projet illustre quatre types distincts.
>
> Le **tableau `string[]`** est la structure la plus basique : taille fixe, accès par index. On l'utilise ici pour lister les cinq rôles à créer au démarrage — une liste courte et immuable.
>
> La **`List<T>`** est dynamique : elle peut grandir ou rétrécir. Couplée à **LINQ** — Language Integrated Query — elle permet d'écrire des requêtes de type SQL directement dans le code C#. Ce que vous voyez dans le contrôleur de paie, c'est une requête LINQ qui est traduite par Entity Framework Core en SQL et envoyée à SQL Server. On n'écrit plus de SQL à la main — on manipule des objets C#.
>
> `Contains()` sur un tableau inline reproduit la clause SQL `IN (...)`. `Sum()` est l'agrégation, équivalent de `SUM()` en SQL. Ces opérations LINQ sont paresseuses — elles ne s'exécutent qu'au moment du `ToListAsync()`, ce qui permet au framework d'optimiser la requête finale.
>
> Le **`Dictionary<K, V>`** est une table de hachage : accès en temps constant O(1) par clé. On l'utilise pour associer une extension de fichier à son type MIME — c'est plus efficace qu'une série de `if/else`."

---

## Concept 4 — Entrées / Sorties (I/O)

### 📍 Emplacement
```
Controllers/LeaveApplyController.cs          — lignes 147 à 177
Controllers/EmployeeController.cs            — lignes 327 à 342
Areas/Identity/Pages/Account/Login.cshtml.cs — ligne 132
```

### 💻 Code

**Lire un formulaire HTTP POST (équivalent de `Console.ReadLine()`) :**
```csharp
// LeaveApplyController.cs
[HttpPost]                    // répond uniquement aux requêtes POST
[ValidateAntiForgeryToken]    // protection contre les attaques CSRF
public async Task<IActionResult> Create(
    [Bind("Id,from,days,status,hrstatus,to,leaveId,reason,empId,companyId")]
    LeaveApplyModel leaveApplyModel)
//  ↑ ASP.NET Core lit automatiquement le corps POST
//    et remplit cet objet (c'est le "Model Binding")
{
    // leaveApplyModel.from   ← vient de <input name="from"> dans le HTML
    // leaveApplyModel.days   ← vient de <input name="days">
    // leaveApplyModel.reason ← vient de <input name="reason">

    if (ModelState.IsValid && leavesdays >= sumOfDays + leaveApplyModel.days)
    {
        _context.Add(leaveApplyModel);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index)); // réponse = redirection HTTP 302
    }
    return RedirectToAction(nameof(Index), new { exceddays = "..." });
}
```

**Lire et envoyer un fichier (I/O disque → HTTP) :**
```csharp
// EmployeeController.cs — téléchargement d'un document employé
public async Task<IActionResult> Download(string filename)
{
    // Construction du chemin physique sur le disque serveur
    var path = Path.Combine(_webHostEnvironment.WebRootPath, "dist/files/", filename);

    var memory = new MemoryStream();
    using (var stream = new FileStream(path, FileMode.Open)) // ouverture fichier
    {
        await stream.CopyToAsync(memory); // lecture asynchrone en mémoire
    }
    memory.Position = 0;

    // Écriture de la réponse HTTP : envoie les octets au navigateur
    return File(memory, GetContentType(path), Path.GetFileName(path));
}
```

**Logger serveur (équivalent de `Console.WriteLine()`) :**
```csharp
// Login.cshtml.cs — après connexion réussie
_logger.LogInformation("User logged in.");
// → écrit dans les logs serveur, jamais visible par l'utilisateur

// Niveaux disponibles :
_logger.LogDebug("...");     // développement uniquement
_logger.LogInformation("..."); // événement normal
_logger.LogWarning("...");   // situation anormale
_logger.LogError("...");     // erreur à corriger
_logger.LogCritical("...");  // arrêt imminent du service
```

### 🎙️ Discours
> "En application console, les entrées/sorties sont simples : `Console.ReadLine()` lit une ligne, `Console.WriteLine()` en affiche une. Dans une application Web, le paradigme est radicalement différent.
>
> L'**entrée**, c'est une requête HTTP POST : quand l'employé soumet son formulaire de congé depuis le navigateur, les données voyagent dans le corps de la requête HTTP jusqu'au serveur. ASP.NET Core intercepte cette requête, et grâce au mécanisme de **Model Binding**, il décode automatiquement les champs du formulaire et les injecte dans notre objet `LeaveApplyModel`. Il n'y a pas de lecture manuelle — le framework fait le travail.
>
> La **sortie**, c'est la réponse HTTP : `return RedirectToAction()` envoie un code 302, `return View(model)` génère du HTML, `return File()` envoie des octets binaires. Le navigateur interprète cette réponse et réagit en conséquence.
>
> Pour les **logs**, l'équivalent de `Console.WriteLine()` côté serveur est `_logger.LogInformation()`. Ces messages ne sont jamais visibles par l'utilisateur — ils sont destinés à l'administrateur système pour surveiller l'application, détecter des erreurs, ou tracer les connexions."

---

## Concept 5 — Énumérations et Constantes

### 📍 Emplacement
```
Controllers/LeaveApplyController.cs — lignes 82 à 100 et 159 à 162
Areas/Identity/Pages/Account/Login.cshtml.cs — ligne 124
Program.cs — ligne 100
```

### 💻 Code

**Statuts d'approbation — strings répétés dans le projet actuel :**
```csharp
// LeaveApplyController.cs — les 3 états possibles d'une demande de congé
// HOD → modifie "status"
.Where(c => c.status == "Pending") // en attente HOD
.Where(c => c.status == "Approve") // approuvé HOD
.Where(c => c.status == "Reject")  // rejeté HOD

// RH → modifie "hrstatus"
.Where(c => c.hrstatus == "pending") // ← minuscule ! incohérence dans le code
.Where(c => c.hrstatus == "Approve")
.Where(c => c.hrstatus == "Reject")

// Statut du compte employé
if (user.status == "Active")   { /* connexion autorisée */ }
if (user.status == "Inactive") { /* connexion bloquée  */ }
```

**Ce que serait une vraie `enum` C# (bonne pratique) :**
```csharp
// Version améliorée — aucune faute de frappe possible
public enum ApprovalStatus
{
    Pending = 0,
    Approve = 1,
    Reject  = 2
}

public enum EmployeeStatus
{
    Active   = 0,
    Inactive = 1
}

// Utilisation sécurisée :
leaveApply.status = ApprovalStatus.Pending.ToString(); // → "Pending"
//                  ↑ le compilateur vérifie que "Pending" existe dans l'enum

// Faute de frappe détectée à la compilation :
leaveApply.status = ApprovalStatus.Approv; // ERREUR : 'Approv' n'existe pas
// vs le code actuel :
leaveApply.status = "Approv"; // ← compiles mais bug silencieux à l'exécution !
```

**Constantes pour les noms de rôles :**
```csharp
// Program.cs — les rôles sont actuellement des strings littéraux
new[] { "admin", "HR", "HOD", "Employee", "Viewer" }

// Bonne pratique : classe de constantes
public static class Roles
{
    public const string Admin    = "admin";
    public const string HR       = "HR";
    public const string HOD      = "HOD";
    public const string Employee = "Employee";
    public const string Viewer   = "Viewer";
}

// Utilisation dans les attributs :
[Authorize(Roles = Roles.Admin + "," + Roles.HR)]
// au lieu de :
[Authorize(Roles = "admin,HR")] // risque de faute de frappe non détectée
```

### 🎙️ Discours
> "Une énumération en C# est un type qui restreint une variable à un ensemble fini de valeurs nommées. C'est le moyen le plus sûr de représenter un état qui ne peut prendre que des valeurs connues à l'avance.
>
> Dans notre projet, on gère trois workflows d'approbation — congés, prêts, corrections de temps — qui partagent tous le même cycle : `Pending` → `Approve` ou `Reject`. Ces valeurs sont actuellement des **strings littéraux** répétés dans tout le code, ce qui présente un risque réel : une faute de frappe comme `'Approv'` au lieu de `'Approve'` passe à la compilation mais crée un bug silencieux à l'exécution.
>
> J'ai identifié une **incohérence existante** : le champ `status` utilise `'Pending'` avec un P majuscule, mais le champ `hrstatus` utilise `'pending'` avec un p minuscule. C'est exactement le type de bug qu'une `enum` aurait empêché dès le départ.
>
> La solution correcte serait de définir une `enum ApprovalStatus` avec les trois valeurs, et d'appeler `.ToString()` pour stocker la chaîne en base de données. De même, une classe statique de constantes pour les noms de rôles éviterait toute divergence entre les contrôleurs. C'est une dette technique que j'ai identifiée et documentée dans le projet."

---

## Concept 6 — Gestion des Exceptions

### 📍 Emplacement
```
Controllers/EmployeeController.cs — lignes 300 à 325
Program.cs                        — ligne 62 et lignes 124 à 127
```

### 💻 Code

**`try/catch` sur une opération critique de base de données :**
```csharp
// EmployeeController.cs — suppression d'un document employé
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Delete(int id, string empid)
{
    try
    {
        // Opération risquée : accès base de données
        var empDocModel = await _context.empDocs.FindAsync(id);

        if (empDocModel != null)
        {
            _context.empDocs.Remove(empDocModel);
            await _context.SaveChangesAsync(); // ← peut lancer DbUpdateException
            //                                    (contrainte FK, timeout, verrou)
            return RedirectToAction(nameof(Edit), new { id = empid });
        }
        else
        {
            return NotFound(); // HTTP 404 : ID inexistant en base
        }
    }
    catch (DbUpdateException ex)
    // ↑ Exception spécifique EF Core — violation de contrainte, problème réseau
    {
        // Ici on pourrait : logger l'erreur, afficher un message à l'utilisateur
        throw; // relance l'exception originale avec sa stack trace intacte
               // DIFFÉRENT de "throw ex" qui réinitialise la stack trace
    }
}
```

**Exception au démarrage — "fail fast" (`Program.cs` ligne 62) :**
```csharp
// Si la connection string est absente de appsettings.json,
// on préfère crasher immédiatement avec un message clair
var connectionString = builder.Configuration
    .GetConnectionString("ApplicationDbContextConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'ApplicationDbContextConnection' not found.");
// L'opérateur "?? throw" : si null → exception lancée sur-le-champ
// Mieux vaut échouer au démarrage que produire des erreurs cryptiques plus tard
```

**Gestionnaire global d'exceptions (`Program.cs` lignes 124–127) :**
```csharp
// En production : toutes les exceptions non gérées sont interceptées
// et redirigées vers une page d'erreur propre
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error"); // page d'erreur générique sans détails
}
// En développement : la page d'exception détaillée s'affiche (stack trace complète)
```

### 🎙️ Discours
> "La gestion des exceptions est le mécanisme qui permet à un programme de réagir proprement à une situation imprévue, sans planter brutalement.
>
> Dans notre projet, l'exemple le plus concret est la suppression d'un document employé. L'opération `SaveChangesAsync()` peut échouer pour plusieurs raisons : le serveur SQL est indisponible, une contrainte de clé étrangère est violée, un verrou concurrent bloque l'opération. Sans `try/catch`, l'exception remonterait jusqu'au navigateur sous forme d'une page d'erreur technique — inacceptable en production.
>
> Je veux attirer votre attention sur la différence entre `throw` et `throw ex`. Écrire `throw` seul relance l'exception originale en préservant toute sa trace d'appels — on sait exactement où l'erreur s'est produite. Écrire `throw ex` recrée une nouvelle exception depuis ce point, effaçant la trace originale — c'est un piège classique qui complique le débogage.
>
> Il y a également un pattern 'fail fast' dans `Program.cs` : si la chaîne de connexion est absente du fichier de configuration, on lance immédiatement une `InvalidOperationException` au démarrage. L'idée est qu'il vaut mieux une erreur explicite et immédiate qu'un serveur qui démarre mais s'effondre mystérieusement à la première requête de base de données.
>
> Enfin, le middleware `UseExceptionHandler` joue le rôle de filet de sécurité global : toute exception qui n'a pas été interceptée localement est capturée ici et redirige l'utilisateur vers une page d'erreur propre, sans révéler les détails techniques."

---

## Concept 7 — Le modèle Singleton (Injection de Dépendances)

### 📍 Emplacement
```
Program.cs                           — lignes 63 à 77
Controllers/LeaveApplyController.cs  — lignes 18 à 29
```

### 💻 Code

**Enregistrement des services dans le conteneur IoC (`Program.cs`) :**
```csharp
// ── SCOPED : une instance créée par requête HTTP ──────────────────────────
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
// ApplicationDbContext est Scoped par défaut dans EF Core :
// → chaque requête HTTP reçoit son propre contexte de base de données
// → évite les conflits de données entre deux utilisateurs simultanés

builder.Services.AddDefaultIdentity<ApplicationUser>(...)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddUserManager<ApplicationUserManager>();
// UserManager, RoleManager, SignInManager → tous Scoped

// ── SCOPED (service personnalisé) ─────────────────────────────────────────
builder.Services.AddScoped<ITimeService, TimeService>();
// ITimeService = l'interface (le contrat)
// TimeService  = l'implémentation concrète
// Le framework crée un TimeService à chaque requête et l'injecte
// dans n'importe quel contrôleur qui le demande dans son constructeur
```

**Réception des dépendances par injection de constructeur :**
```csharp
// LeaveApplyController.cs — le contrôleur NE crée RIEN lui-même
public class LeaveApplyController : Controller
{
    // Dépendances déclarées readonly : immuables après construction
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    public IWebHostEnvironment _webHostEnvironment;

    // Le framework lit ce constructeur, résout les types,
    // et injecte les instances appropriées automatiquement
    public LeaveApplyController(
        ApplicationDbContext context,
        IWebHostEnvironment webHostEnvironment,
        UserManager<ApplicationUser> userManager)
    {
        _context            = context;
        _webHostEnvironment = webHostEnvironment;
        _userManager        = userManager;
    }

    // Utilisation dans les actions :
    public async Task<IActionResult> Index(...)
    {
        var user = await _userManager.GetUserAsync(User); // UserManager injecté
        var list = await _context.leaveApplies            // DbContext injecté
            .Where(c => c.empId == user.Id)
            .ToListAsync();
    }
}
```

**Les trois durées de vie (cycle de vie des objets) :**
```csharp
// SINGLETON  : 1 instance pour toute la durée de vie du serveur
builder.Services.AddSingleton<IMonService, MonService>();

// SCOPED     : 1 instance par requête HTTP  ← utilisé dans ce projet
builder.Services.AddScoped<ITimeService, TimeService>();
builder.Services.AddDbContext<ApplicationDbContext>(...); // Scoped par défaut

// TRANSIENT  : 1 instance à chaque injection (le plus "frais", le plus coûteux)
builder.Services.AddTransient<IMonService, MonService>();
```

### 🎙️ Discours
> "L'injection de dépendances est un pattern de conception fondamental en C# moderne. Il répond à un problème simple : comment un objet peut-il obtenir les outils dont il a besoin sans les fabriquer lui-même ?
>
> Sans injection de dépendances, le contrôleur devrait écrire `new ApplicationDbContext(...)` — il serait directement couplé à l'implémentation concrète. Cela rend le code difficile à tester, difficile à maintenir, et impossible à reconfigurer sans modifier le code source.
>
> Avec l'injection de dépendances, on **inverse le contrôle** : c'est le framework ASP.NET Core qui décide quand et comment créer les objets. Le contrôleur déclare simplement ce dont il a besoin dans son constructeur, et le framework le lui fournit. C'est ce qu'on appelle le principe IoC — Inversion of Control.
>
> Le **pattern Singleton** est une des trois durées de vie disponibles. Un Singleton est créé une seule fois et partagé par toutes les requêtes, toute la durée de vie du serveur. Dans notre projet, le DbContext est au contraire en **mode Scoped** — une instance par requête HTTP — ce qui est crucial : si deux utilisateurs modifient des données simultanément, ils ont chacun leur propre contexte EF Core et ne se marchent pas dessus.
>
> Une règle d'or à connaître : on ne doit jamais injecter un service Scoped dans un Singleton. Le service Scoped serait 'capturé' et vivrait au-delà de sa requête — ASP.NET Core détecte cette erreur au démarrage et lance une exception. C'est un des avantages du système : il nous protège contre nos propres erreurs de conception."

---

## 📋 Tableau de référence rapide

| Concept | Fichier clé | Ligne | Mot-clé à mentionner |
|---------|------------|-------|----------------------|
| Méthode Main | `Program.cs` | 61–136 | Top-Level Statements, `app.Run()`, pipeline HTTP |
| Variables & Types | `ApplicationUser.cs` | 13–43 | Nullable `?`, value type, reference type |
| Conversions | `PayRollController.cs` | 33–37 | `ParseExact`, `ToString`, cast explicite `(double)` |
| Collections & LINQ | `PayRollController.cs` | 39–43 | `List<T>`, `Where`, `OrderBy`, `Sum`, `Contains` |
| I/O Web | `LeaveApplyController.cs` | 147–177 | Model Binding, HTTP POST, `_logger`, `return File()` |
| Enum / Constantes | `LeaveApplyController.cs` | 82–100 | `enum`, `const`, dette technique, string littéral |
| Exceptions | `EmployeeController.cs` | 300–325 | `try/catch`, `throw` vs `throw ex`, fail fast |
| Injection / Singleton | `Program.cs` + `LeaveApplyController.cs` | 63–77 / 18–29 | IoC, Scoped, Singleton, Transient, couplage |

---

*Document généré le 2026-04-20 — SUPMTI RH — Guide de soutenance*
