# SUPMTI RH — Système de Gestion des Ressources Humaines

> Documentation technique et fonctionnelle — Base pour la rédaction du cahier des charges

---

## Table des matières

1. [Présentation du projet](#1-présentation-du-projet)
2. [Stack technique](#2-stack-technique)
3. [Architecture du projet](#3-architecture-du-projet)
4. [Rôles et permissions](#4-rôles-et-permissions)
5. [Workflow d'authentification](#5-workflow-dauthentification)
6. [Workflow de création d'un employé](#6-workflow-de-création-dun-employé)
7. [Workflow Congés](#7-workflow-congés)
8. [Workflow Présence (Attendance)](#8-workflow-présence-attendance)
9. [Workflow Correction de Temps (COAT)](#9-workflow-correction-de-temps-coat)
10. [Workflow Prêts (Loans)](#10-workflow-prêts-loans)
11. [Workflow Actions Disciplinaires](#11-workflow-actions-disciplinaires)
12. [Workflow Paie (Payroll)](#12-workflow-paie-payroll)
13. [Vues et rapports SQL](#13-vues-et-rapports-sql)
14. [Installation et démarrage](#14-installation-et-démarrage)

---

## 1. Présentation du projet

**SUPMTI RH** est un système complet de gestion des ressources humaines développé en ASP.NET Core MVC. Il couvre l'ensemble du cycle de vie RH : enregistrement des employés, suivi de présence par machine biométrique, gestion des congés, des prêts, de la paie, et des actions disciplinaires.

Le système applique un modèle d'approbation à **deux niveaux** (HOD → RH) pour toutes les demandes des employés, garantissant une chaîne de validation hiérarchique.

---

## 2. Stack technique

| Composant | Technologie |
|-----------|------------|
| Framework backend | ASP.NET Core MVC (.NET 8) |
| ORM | Entity Framework Core (Code First) |
| Base de données | SQL Server (Express) |
| Authentification | ASP.NET Core Identity |
| Frontend | Bootstrap 5, jQuery |
| Temps réel | SignalR (Hub/) |
| Langages | C#, SQL (procédures stockées) |

---

## 3. Architecture du projet

```
/Areas/Identity/         → Pages d'authentification (Login, Register, ForgotPassword)
/Areas/Identity/Data/    → ApplicationUser, ApplicationDbContext, ApplicationUserManager
/Controllers/            → Logique métier (30+ contrôleurs)
/Models/hrms/            → Entités de données (26 modèles + ViewModels)
/Views/                  → Interfaces Razor (.cshtml)
/Views/Shared/_Layout/   → Sidebar de navigation dynamique selon le rôle
/wwwroot/                → Fichiers statiques (CSS, JS, images, uploads)
/Hub/                    → Notifications temps réel (SignalR)
```

**Fichier de démarrage clé :** `Program.cs` — initialise les rôles, crée le compte Admin, configure les middlewares.

---

## 4. Rôles et permissions

Le système définit **5 rôles** créés automatiquement au démarrage (`Program.cs`) :

| Rôle | Accès Dashboard | Fonctionnalités principales |
|------|----------------|----------------------------|
| `admin` | Tableau de bord Admin | Accès total à toutes les fonctionnalités |
| `HR` | Tableau de bord Admin | Gestion employés, paie, approbations finales, configuration |
| `HOD` | Tableau de bord Employé | Voir/approuver demandes de son département |
| `Employee` | Tableau de bord Employé | Ses propres présences, congés, prêts, bulletin de paie |
| `Viewer` | Tableau de bord Admin | Rapports en lecture seule uniquement |

### Matrice des permissions par contrôleur

| Ressource | admin | HR | HOD | Employee | Viewer |
|-----------|-------|----|-----|----------|--------|
| Créer/modifier employés | ✅ | ✅ | ❌ | ❌ | ❌ |
| Paie (génération/édition) | ✅ | ✅ | ❌ | ❌ | ❌ |
| Paie (lecture personnelle) | ✅ | ✅ | ✅ | ✅ | ❌ |
| Approbation congés (niveau 1) | ✅ | ❌ | ✅ | ❌ | ❌ |
| Approbation congés (niveau 2) | ✅ | ✅ | ❌ | ❌ | ❌ |
| Demande de congé | ✅ | ✅ | ✅ | ✅ | ❌ |
| Actions disciplinaires | ✅ | ✅ | ❌ | ❌ | ❌ |
| Rapports présence (tous) | ✅ | ✅ | ❌ | ❌ | ✅ |
| Rapports présence (département) | ✅ | ✅ | ✅ | ❌ | ✅ |

### Sidebar dynamique (`Views/Shared/_Layout.cshtml`)

La navigation latérale s'affiche conditionnellement selon le rôle :

```csharp
// Ligne 688 — Section Tableau de bord Admin
@if (userRoles.Contains("HR") || userRoles.Contains("admin") || userRoles.Contains("Viewer"))
{ ... lien vers /Home/Index ... }

// Ligne 692 — Section Tableau de bord Employé
@if (userRoles.Contains("Employee") || userRoles.Contains("admin") || userRoles.Contains("HOD"))
{ ... lien vers /Home/EmpIndex ... }

// Ligne 698 — Menu Employé (Présence, Congés, Prêt, Paie perso)
@if (userRoles.Contains("Employee") || userRoles.Contains("admin"))
{ ... }

// Ligne 745 — Menu HOD (Présence subordonné, Congés subordonné)
@if (userRoles.Contains("HOD") || userRoles.Contains("admin"))
{ ... }

// Ligne 804 — Menu HR complet (Approbations, Jours fériés, Configuration, Paie)
@if (userRoles.Contains("HR") || userRoles.Contains("admin"))
{ ... }

// Ligne 876 — Menu Viewer (Rapports lecture seule)
@if (userRoles.Contains("Viewer"))
{ ... }
```

---

## 5. Workflow d'authentification

### 5.1 Schéma complet

```
[Page Login]
     │
     ▼
Saisie email + mot de passe
     │
     ▼
Vérification : utilisateur existe ? ──NON──► "Tentative de connexion invalide"
     │
    OUI
     ▼
Vérification : status == "Active" ? ──NON──► "Votre compte n'est pas actif."
     │
    OUI
     ▼
PasswordSignInAsync()
     │
     ├──ÉCHEC──► "Tentative de connexion invalide"
     ├──LOCKOUT──► /Account/Lockout
     └──SUCCÈS──► RedirectToAction("AfterLogin", "Home")
                        │
                        ▼
              [HomeController.AfterLogin()]
                        │
             ┌──────────┼──────────┬──────────┐
             ▼          ▼          ▼          ▼
           admin        HR      Employee     HOD
             │          │          │          │
             ▼          ▼          ▼          ▼
        /Home/Index /Home/Index /Home/EmpIndex /Home/EmpIndex
```

### 5.2 Code source clé

**Fichier :** `Areas/Identity/Pages/Account/Login.cshtml.cs`

```csharp
// Vérification du statut avant connexion (ligne 124)
if (user != null && user.status == "Active")
{
    var result = await _signInManager.PasswordSignInAsync(
        Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);

    if (result.Succeeded)
        return RedirectToAction("AfterLogin", "Home"); // ligne 133
}
else
{
    ModelState.AddModelError("", "Votre compte n'est pas actif."); // ligne 157
}
```

**Fichier :** `Controllers/HomeController.cs`

```csharp
// Routage post-login selon le rôle (lignes 44-65)
[Authorize]
public async Task<IActionResult> AfterLogin()
{
    var user = await _userManager.GetUserAsync(User);

    if (await _userManager.IsInRoleAsync(user, "admin"))
        return RedirectToAction("Index", "Home");        // Dashboard Admin
    else if (await _userManager.IsInRoleAsync(user, "HR"))
        return RedirectToAction("Index", "Home");        // Dashboard Admin
    else if (await _userManager.IsInRoleAsync(user, "Employee"))
        return RedirectToAction("EmpIndex", "Home");     // Dashboard Employé
    else if (await _userManager.IsInRoleAsync(user, "HOD"))
        return RedirectToAction("EmpIndex", "Home");     // Dashboard Employé

    return RedirectToAction("Index", "Home");            // Fallback
}
```

### 5.3 Règles importantes

- Le champ `status` de l'utilisateur **doit être "Active"** pour autoriser la connexion.
- La vérification du statut est faite **avant** `PasswordSignInAsync`, donc un mauvais mot de passe ne révèle pas l'existence du compte inactif.
- `IsInRoleAsync` est **insensible à la casse** (normalisation interne par ASP.NET Identity).
- `userRoles.Contains()` dans les vues est **sensible à la casse** — les noms de rôles doivent correspondre exactement.

---

## 6. Workflow de création d'un employé

### 6.1 Schéma complet

```
[Admin ou HR connecté]
         │
         ▼
  Menu → Configuration → Nouvel Employé
  (/Identity/Account/Register)
         │
         ▼
  Formulaire de création :
  ┌─────────────────────────────────────┐
  │ Email (identifiant de connexion)    │
  │ Mot de passe + Confirmation         │
  │ Nom complet (name)                  │
  │ Rôle (sélection dans liste)         │
  │ Statut : Active / Inactive          │
  │ Date d'embauche (joining_date)      │
  │ Entreprise, Département, Poste      │
  │ Horaire (Shift)                     │
  │ Infos bancaires, CNIC, Contact      │
  │ Photo de profil                     │
  │ Documents joints (multiple)         │
  └─────────────────────────────────────┘
         │
         ▼
  POST → Register.cshtml.cs
         │
         ▼
  1. Créer l'objet ApplicationUser
  2. SetUserNameAsync(email)
  3. SetEmailAsync(email)
  4. CreateAsync(user, password)  ◄─── L'utilisateur est créé en BDD
         │
      SUCCÈS
         │
         ▼
  5. FindByIdAsync(roleId)        ◄─── Trouver le rôle sélectionné
  6. AddToRoleAsync(user, role.Name) ◄─ Assigner le rôle (APRÈS création)
         │
         ▼
  7. Sauvegarder les documents joints
         │
         ▼
  Redirection → /Employee/Index
```

### 6.2 Code source clé

**Fichier :** `Areas/Identity/Pages/Account/Register.cshtml.cs`

```csharp
// Ordre correct de création + assignation de rôle (lignes 231-240)
var result = await _userManager.CreateAsync(user, Input.Password);

if (result.Succeeded)
{
    // Assignation du rôle APRÈS création réussie
    if (Input.roleId != null)
    {
        var role = await _manager.FindByIdAsync(Input.roleId);
        if (role != null)
            await _userManager.AddToRoleAsync(user, role.Name);
    }
    // Sauvegarde des documents joints...
    return RedirectToAction("Index", "Employee");
}
```

### 6.3 Points de vigilance critiques

| Point | Règle |
|-------|-------|
| **Status** | Doit être `"Active"` pour que la connexion fonctionne |
| **Rôle** | Doit être sélectionné explicitement (pas de rôle par défaut) |
| **Email** | Sert de `UserName` ET d'`Email` — doit être unique |
| **Ordre de création** | Le rôle est assigné **après** `CreateAsync()`, jamais avant |
| **Modification de rôle** | Via `Employee/Edit` : les anciens rôles sont retirés avant l'assignation du nouveau |

### 6.4 Modification d'un employé existant

**Fichier :** `Controllers/EmployeeController.cs` — action `Edit` (POST, lignes 141-288)

```csharp
// Retrait des anciens rôles avant assignation du nouveau (lignes 188-193)
var currentRoles = await _userManager.GetRolesAsync(user);
await _userManager.RemoveFromRolesAsync(user, currentRoles);

if (!string.IsNullOrEmpty(EmpModel.roleId))
{
    var role = await _manager.FindByIdAsync(EmpModel.roleId);
    if (role != null)
        await _userManager.AddToRoleAsync(user, role.Name);
}
```

---

## 7. Workflow Congés

### 7.1 Schéma d'approbation (2 niveaux)

```
[Employé / HOD / Employee]
         │
         ▼
  Soumet une demande de congé
  (LeaveApplyController.Create — POST)
  Champs : from, to, days, leaveId, reason
  status = "Pending" | hrstatus = "pending"
         │
         ▼
  ┌─────────────────────────────────────┐
  │    NIVEAU 1 : Approbation HOD        │
  │  LeaveRequestController.Index()      │
  │  [Authorize(Roles = "admin,HOD")]   │
  │                                     │
  │  Filtre : departId == HOD.departId  │
  │  Filtre : status == "Pending"       │
  │                                     │
  │  HOD → Approve / Reject             │
  │  → status = "Approve" | "Reject"   │
  └─────────────────────────────────────┘
         │
      status == "Approve"
         │
         ▼
  ┌─────────────────────────────────────┐
  │    NIVEAU 2 : Approbation HR         │
  │  LeaveRequestController.Indexhr()   │
  │  [Authorize(Roles = "admin,HR")]    │
  │                                     │
  │  Filtre : status == "Approve"       │
  │  Filtre : hrstatus == "pending"     │
  │                                     │
  │  HR → Approve / Reject              │
  │  → hrstatus = "Approve" | "Reject" │
  └─────────────────────────────────────┘
         │
  hrstatus == "Approve"
         │
         ▼
  Congé finalement approuvé
  Apparaît dans le calcul de présence
  (tempMonthAttModel.leave = nom du congé)
```

### 7.2 Valeurs des champs de statut

| Champ | Valeur initiale | Approuvé | Rejeté |
|-------|----------------|----------|--------|
| `status` (HOD) | `"Pending"` | `"Approve"` | `"Reject"` |
| `hrstatus` (HR) | `"pending"` | `"Approve"` | `"Reject"` |

### 7.3 Validation métier

- **Quota de congés** : Vérification que `leaveType.days >= daysRequested` avant création (`LeaveApplyController` ligne 164).
- **Types de congés** : Définis par HR dans `LeaveTypeModel` (ex: Congé annuel, Maladie, etc.) liés à une `FascalYearModel`.
- **Périmètre HOD** : Un HOD ne voit que les demandes de son département (`departId`).
- **Périmètre HR** : Voit toutes les demandes dont `status == "Approve"` (approuvées par HOD).

---

## 8. Workflow Présence (Attendance)

### 8.1 Architecture globale

```
[Machine biométrique]
         │
         ▼ (données brutes via API/import)
  Table rawattendances
  (id, empId, att_datetime, AttState)
  AttState : "4" = Entrée | "5" = Sortie
         │
         ▼
  [Procédure stockée SQL : Reconciliation]
  EXEC Reconciliation @Fromdate, @Todate, @empId, @companyId
         │
         ▼
  ReconciliationViewModel (Vue SQL : ReconciliationView)
  Champs calculés :
  ┌─────────────────────────────────────┐
  │ TotalDays          (jours du mois)  │
  │ Sundays            (dimanches)      │
  │ Saturdays          (samedis)        │
  │ GazettedHolidaysCount              │
  │ CompanyHolidaysCount               │
  │ TotalWorkingDays   (jours ouvrés)  │
  │ PresentDays        (jours présents)│
  └─────────────────────────────────────┘
         │
         ▼
  [Procédure stockée SQL : tempMonthlyAttendance]
  EXEC tempMonthlyAttendance @Fromdate, @Todate, @empId, @companyId
         │
         ▼
  tempMonthAttModel (détail jour par jour)
  Champs booléens de déduction :
  ┌─────────────────────────────────────┐
  │ late             (retard)           │
  │ absent           (absence)          │
  │ diciplinaryaction (sanction)        │
  │ halfday          (demi-journée)     │
  │ earlygoing       (départ anticipé)  │
  │ present          (présent)          │
  │ leave            (type de congé)    │
  │ holiday          (férié/company)    │
  └─────────────────────────────────────┘
         │
         ▼
  [Procédure stockée SQL : DeductionCount]
  EXEC DeductionCount @Fromdate, @Todate, @empId, @companyId
         │
         ▼
  DeductionCountViewModel (Vue SQL : DeductionCountView)
  ┌─────────────────────────────────────┐
  │ Late              (nb retards)      │
  │ Absent            (nb absences)     │
  │ DiciplinaryAction (nb sanctions)    │
  │ TotalDeduction    (total déductions)│
  └─────────────────────────────────────┘
```

### 8.2 Visibilité par rôle

| Vue | HR/admin | HOD | Employee/Viewer |
|-----|----------|-----|-----------------|
| Réconciliation tous employés | ✅ | ❌ | ❌ |
| Réconciliation département | ✅ | ✅ (son dept) | ❌ |
| Réconciliation personnelle | ✅ | ✅ | ✅ |
| Comptage déductions tous | ✅ | ❌ | ❌ |
| Comptage déductions département | ✅ | ✅ (son dept) | ❌ |
| Comptage déductions personnel | ✅ | ✅ | ✅ |

**Filtrage HOD (`AttendanceController.cs` lignes 99-103) :**
```csharp
else if (userRoles.FirstOrDefault() == "HOD" && role == "HOD")
{
    return View(reconciliation
        .Where(c => c.emp.departId == user.departId && c.empId != user.Id)
        ...);
}
```

### 8.3 Sandwich Attendance

Mécanisme permettant à RH de marquer une période de présence exemptée entre deux absences/congés (ex: un jour ouvré entre deux jours fériés).

**Modèle :** `SandwichAttModel`

| Champ | Type | Description |
|-------|------|-------------|
| `date` | DateTime | Date d'enregistrement de l'action |
| `from` | DateTime | Début de la période exclue |
| `to` | DateTime | Fin de la période exclue |
| `reason` | string | Justification |
| `empId` | string (FK) | Employé concerné |

**Accès :** HR et Admin uniquement `[Authorize(Roles = "admin,HR")]`.

---

## 9. Workflow Correction de Temps (COAT)

COAT = **Correction Of Attendance Time** — permet à un employé de demander la correction d'une entrée/sortie mal enregistrée par la machine biométrique.

### 9.1 Schéma d'approbation (identique aux congés)

```
[Employé]
    │ Soumet correction (date, correct_datetime, reason)
    │ status = "Pending" | hrstatus = "pending"
    ▼
[HOD]  → Approve / Reject  → status = "Approve" | "Reject"
    │ (filtre : son département uniquement)
    ▼
[HR]   → Approve / Reject  → hrstatus = "Approve" | "Reject"
    │ (filtre : status == "Approve")
    ▼
Correction appliquée dans le calcul de présence
(tempMonthAttModel.R_Checkin = correct_datetime)
```

### 9.2 Compteurs et quotas (COATController)

| Indicateur | Description |
|-----------|-------------|
| Compteur annuel | Nombre de demandes COAT sur l'année |
| Compteur mensuel | Demandes du mois en cours |
| Compteur hebdomadaire | Demandes de la semaine |
| Compteur en attente | Demandes `status == "Pending"` |

### 9.3 Champs du modèle `COATModel`

| Champ | Type | Description |
|-------|------|-------------|
| `date` | DateTime | Date pour laquelle la correction est demandée |
| `correct_datetime` | DateTime | Heure corrigée souhaitée |
| `reason` | string | Motif de la correction |
| `status` | string | Approbation HOD : "Pending" / "Approve" / "Reject" |
| `hrstatus` | string | Approbation HR : "pending" / "Approve" / "Reject" |
| `empId` | string (FK) | Employé demandeur |
| `companyId` | int (FK) | Entreprise |

---

## 10. Workflow Prêts (Loans)

### 10.1 Schéma complet

```
[Employé]
    │ Soumet demande de prêt
    │ (loanamount, repaymentamount, startdate, reason)
    │ status = "Pending" | hrstatus = "pending"
    ▼
[HOD]  → Approve / Reject  → status = "Approve" | "Reject"
    │ (son département uniquement)
    ▼
[HR]   → Approve / Reject  → hrstatus = "Approve" | "Reject"
    │ (status == "Approve" requis)
    ▼
Prêt accordé → Enregistré dans le Livre de Prêt
    │
    ▼
[LoanOpeningModel] : Solde d'ouverture initial (HR uniquement)
    │
    ▼
[Procédure stockée : loan_rep @fromdate, @todate, @empId]
    │ Génère le grand livre :
    │   - Received : montants perçus
    │   - Pay : remboursements via déductions de paie
    │   - Balance : solde courant
    ▼
[PayRollModel.loan_deduction] : Déduction mensuelle automatique en paie
```

### 10.2 Calcul du bilan de prêt (`LoanApplyController.Index`)

```csharp
// Total reçu = prêts approuvés + solde d'ouverture (lignes 41-43)
totalreceived = loansApproved.Sum(l => l.loanamount) + loanOpenings.Sum(o => o.opening);

// Total remboursé = somme des déductions en paie (ligne 45)
totalpay = payRolls.Sum(p => p.loan_deduction);

// Solde restant dû (ligne 47)
balance = totalreceived - totalpay;
```

### 10.3 Champs du modèle `LoanApplyModel`

| Champ | Type | Description |
|-------|------|-------------|
| `loanamount` | int | Montant du prêt demandé |
| `repaymentamount` | int | Montant de remboursement mensuel |
| `startdate` | DateTime | Date de début du remboursement |
| `reason` | string | Motif de la demande |
| `status` | string | Approbation HOD |
| `hrstatus` | string | Approbation HR |

---

## 11. Workflow Actions Disciplinaires

### 11.1 Création

- **Accès exclusif :** `[Authorize(Roles = "admin,HR")]`
- **Créateur :** HR ou Admin uniquement
- **Pas d'approbation nécessaire** — action directe

**Fichier :** `Controllers/DiciplinaryActionController.cs`

| Champ | Type | Description |
|-------|------|-------------|
| `ActionDate` | DateTime | Date de l'action disciplinaire |
| `reason` | string | Motif de la sanction |
| `empId` | string (FK) | Employé sanctionné |
| `companyId` | int (FK) | Entreprise |

### 11.2 Impact sur la paie et la présence

L'action disciplinaire est prise en compte dans deux calculs :

1. **Présence :** `tempMonthAttModel.diciplinaryaction = true` pour le jour concerné.
2. **Déductions :** Comptabilisée dans `DeductionCountViewModel.DiciplinaryAction` (vue SQL `DeductionCountView`).
3. **Paie :** Contribue au `deduction_count` → `att_deduction` dans `PayRollModel`.

---

## 12. Workflow Paie (Payroll)

### 12.1 Schéma de génération

```
[HR / Admin]
    │
    ▼
Sélection : mois + entreprise
    │
    ▼
[EXEC payroll @month, @companyId]  (Procédure stockée SQL)
    │ Calcule automatiquement :
    │   - Salaire brut (gross_salary)
    │   - Composantes : basic, HRA, medical, conveyance, utility, food
    │   - Déductions : retards/absences (att_deduction), PF, EOBI, impôt
    ▼
[PayRollModel] créé pour chaque employé
    │
    ▼
[HR ajuste manuellement] :
    ├── loan_deduction (remboursement prêt)
    ├── sessi_deduction (Sécurité Sociale)
    ├── other_deduction (autres)
    ├── arear (arriérés)
    ├── taxable_arear (arriérés imposables)
    ├── bonus
    └── remarks / CPR
    │
    ▼
[EXEC payroll_final @date, @monthyear, @companyId]
    │ Finalise les calculs définitifs
    ▼
Bulletin de paie disponible pour l'employé (/PayRoll/salaryslip)
Certificat fiscal (/PayRoll/incometaxcertificate)
Lettre bancaire (/PayRoll/bankletter)
```

### 12.2 Structure du `PayRollModel`

**Identifiants :**

| Champ | Type | Description |
|-------|------|-------------|
| `monthYear` | string | Période (ex: "JANUARY2025") |
| `employeeId` | string (FK) | Référence employé |
| `companyId` | int (FK) | Référence entreprise |

**Composantes de salaire :**

| Champ | Description |
|-------|-------------|
| `gross_salary` | Salaire brut total |
| `basic` | Salaire de base |
| `hra` | Indemnité logement |
| `medical_all` | Indemnité médicale |
| `con_all` | Indemnité transport |
| `utility_all` | Indemnité utilitaire |
| `food_all` | Indemnité repas |
| `other_all` | Autres indemnités |

**Déductions :**

| Champ | Description |
|-------|-------------|
| `days` | Jours travaillés dans le mois |
| `day_salary` | Salaire journalier |
| `deduction_count` | Nombre de jours déduits |
| `att_deduction` | Montant déduit pour absences/retards |
| `pf` | Fonds de prévoyance |
| `EOBI` | EOBI (retraite) |
| `incometax` | Impôt sur le revenu |
| `loan_deduction` | Remboursement prêt |
| `sessi_deduction` | Sécurité sociale |
| `other_deduction` | Autres déductions |
| `total_deduction` | Total des déductions |

**Additions et résultat :**

| Champ | Description |
|-------|-------------|
| `arear` | Arriérés |
| `taxable_arear` | Arriérés imposables |
| `bonus` | Prime |
| `total_addition` | Total des additions |
| `net_salary` | **Salaire net = brut + additions − déductions** |

### 12.3 Accès employé à sa propre paie

- `/PayRoll/emppayRoll` — Liste des bulletins mensuels
- `/PayRoll/salaryslip` — Bulletin de paie détaillé (mois sélectionné)
- `/PayRoll/incometaxcertificate` — Certificat fiscal annuel

---

## 13. Vues et rapports SQL

Le système utilise **7 vues SQL** (read-only) pour les rapports complexes :

| Vue SQL | Entité C# | Utilisation |
|---------|-----------|-------------|
| `ReconciliationView` | `ReconciliationViewModel` | Résumé mensuel de présence par employé |
| `TempMonthAttView` | `tempMonthAttViewModel` | Détail jour par jour des présences |
| `DeductionCountView` | `DeductionCountViewModel` | Comptage des déductions (retards, absences, sanctions) |
| `LeaveCountView` | `LeaveCountViewModel` | Rapport congés (colonnes dynamiques par type) |
| `LoanView` | `LoanViewModel` | Grand livre des prêts (entrées/sorties/solde) |
| `empAttendView` | `empAttendViewModel` | Vue présence simplifiée pour l'employé |
| `GetPakTimeView` | `GetPakTime` | Heure courante Pakistan (fuseau horaire) |

> **Note :** Ces vues sont mappées via `builder.Entity<...>().ToView("...")` dans `ApplicationDbContext.cs` et ne correspondent pas à des tables physiques EF-gérées.

---

## 14. Installation et démarrage

### Prérequis

- Visual Studio 2022 ou VS Code
- .NET 8 SDK
- SQL Server Express
- Node.js (pour les assets frontend si nécessaire)

### Configuration

**1. Connexion base de données** (`appsettings.json`) :

```json
"ConnectionStrings": {
  "ApplicationDbContextConnection": "Server=localhost\\SQLEXPRESS;Database=itgsgroup;Trusted_Connection=True;TrustServerCertificate=True"
}
```

**2. Appliquer les migrations** :

```bash
Update-Database
```

**3. Lancer l'application** — Au premier démarrage, `Program.cs` crée automatiquement :

- Les 5 rôles : `admin`, `HR`, `HOD`, `Employee`, `Viewer`
- Le compte administrateur par défaut

### Compte Admin par défaut

| Champ | Valeur |
|-------|--------|
| Email | `admin@hrm.com` |
| Mot de passe | `Admin@123` |
| Rôle | `admin` |

> ⚠️ Changer le mot de passe admin après la première connexion.

### Workflow de mise en service recommandé

```
1. Connexion Admin
2. Configuration → Locations → créer les villes/sites
3. Configuration → Companys → créer l'entreprise
4. Configuration → Département → créer les départements
5. Configuration → Poste → créer les désignations
6. Configuration → Horaire → définir les shifts
7. Configuration → Fiscal Year → définir l'année fiscale
8. Configuration → Leave Fixation → définir les quotas de congés
9. Configuration → Nouvel Employé → créer les utilisateurs HR, HOD, Employee
10. Vérifier que chaque employé a : status=Active, rôle assigné, département assigné
```

---

## Synthèse des valeurs de statut (référence rapide)

| Workflow | Champ | Valeurs possibles | Qui le modifie |
|----------|-------|------------------|----------------|
| Congés | `status` | `"Pending"` → `"Approve"` / `"Reject"` | HOD |
| Congés | `hrstatus` | `"pending"` → `"Approve"` / `"Reject"` | HR |
| Prêts | `status` | `"Pending"` → `"Approve"` / `"Reject"` | HOD |
| Prêts | `hrstatus` | `"pending"` → `"Approve"` / `"Reject"` | HR |
| COAT | `status` | `"Pending"` → `"Approve"` / `"Reject"` | HOD |
| COAT | `hrstatus` | `"pending"` → `"Approve"` / `"Reject"` | HR |
| Employé | `status` | `"Active"` / `"Inactive"` | Admin / HR |

---

*Documentation générée le 2026-04-19 — SUPMTI RH v1.0*
