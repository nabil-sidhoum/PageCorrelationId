<div align="center">

# 🔍 PageCorrelationId

### Traçabilité métier entre un front ASP.NET Core MVC et une Web API

[![Build & Tests](https://github.com/nabil-sidhoum/PageCorrelationId/actions/workflows/build.yml/badge.svg)](https://github.com/nabil-sidhoum/PageCorrelationId/actions/workflows/build.yml)
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-MVC_+_Web_API-512BD4?logo=dotnet&logoColor=white)](https://learn.microsoft.com/aspnet/core)
[![Architecture](https://img.shields.io/badge/Architecture-Clean_Architecture-blue)](https://learn.microsoft.com/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures)
[![Tests](https://img.shields.io/badge/Tests-xUnit_+_Moq-green?logo=xunit)](https://xunit.net/)
[![License](https://img.shields.io/badge/License-MIT-yellow)](LICENSE)

*Propager automatiquement un Correlation ID entre toutes les requêtes déclenchées par une même page — sans toucher au JavaScript ni aux controllers métier.*

</div>

---

## 🎯 Le problème

Dans une application web qui charge ses données via des appels AJAX, une seule ouverture de page peut déclencher plusieurs requêtes vers une API. En production, quand une requête échoue ou est lente, il est très difficile de retrouver dans les logs **lesquels de ces appels ont été déclenchés par la même page**.

<table>
<tr>
<th>❌ Sans traçabilité</th>
<th>✅ Avec Correlation ID</th>
</tr>
<tr>
<td>

```
[API] GET /api/stats     → 200 in 12ms
[API] GET /api/stats     → 200 in 45ms
[API] GET /api/stats     → 500 in 3ms
[API] GET /api/something → 200 in 8ms
```
*Impossible de savoir quels appels*
*appartiennent à la même page.*

</td>
<td>

```
[API] GET /api/stats     → 200 | CID: a3f1c2...
[API] GET /api/stats     → 200 | CID: a3f1c2...
[API] GET /api/stats     → 500 | CID: 9e8d7c...
[API] GET /api/something → 200 | CID: 9e8d7c...
```
*Filtrer par CID → toutes les requêtes*
*d'une même page en un instant.*

</td>
</tr>
</table>

---

## ✨ La solution

Ce projet démontre comment propager automatiquement un **Correlation ID métier** entre un front MVC et une Web API — **sans aucune modification dans le JavaScript ni dans les controllers métier**.

### Flow complet

```
  Navigateur                    PageCorrelationId.Site          PageCorrelationId.Api
      │                                  │                            │
      │  GET /Dashboard                  │                            │
      │  Sec-Fetch-Mode: navigate        │                            │
      │ ───────────────────────────────► │                            │
      │                         ┌────────┴────────┐                  │
      │                         │  Site Middleware  │                  │
      │                         │  Nouveau CID ✨  │                  │
      │                         │  Cookie: cid=... │                  │
      │                         └────────┬────────┘                  │
      │ ◄──────────── Set-Cookie: cid=.. │                            │
      │                                  │                            │
      │  GET /Dashboard/GetStats (AJAX)  │                            │
      │  Cookie: cid=<même guid>         │                            │
      │ ───────────────────────────────► │                            │
      │                         ┌────────┴────────┐                  │
      │                         │  Site Middleware  │                  │
      │                         │  Réutilise CID ♻️ │                  │
      │                         └────────┬────────┘                  │
      │                         ┌────────┴──────────────┐            │
      │                         │ DelegatingHandler      │            │
      │                         │ X-Correlation-ID: ...  │            │
      │                         └────────┬──────────────┘            │
      │                                  │  X-Correlation-ID: <guid> │
      │                                  │ ─────────────────────────►│
      │                                  │                   ┌────────┴───────┐
      │                                  │                   │ Api Middleware  │
      │                                  │                   │ Lit le header  │
      │                                  │                   │ Log + Items    │
      │                                  │                   └────────┬───────┘
      │                                  │  X-Correlation-ID: <guid> │
      │ ◄──────────────────────────────────────────────────────────── │
```

---

## 🏗️ Architecture

Ce projet suit les principes de la **Clean Architecture** avec une séparation stricte des responsabilités.

```
src/
├── 📦 Services/
│   ├── PageCorrelationId.Api              # Web API — controllers et pipeline HTTP
│   ├── PageCorrelationId.Api.Utils        # Middlewares réutilisables côté API
│   │   ├── ApiCorrelationId/            # Lecture et propagation du CID entrant
│   │   └── RequestLogging/             # Logging structuré avec CID sur chaque requête
│   ├── PageCorrelationId.Application      # Couche applicative — queries MediatR
│   ├── PageCorrelationId.Domain           # Entités métier
│   └── PageCorrelationId.Infrastructure   # Implémentations des repositories
│
└── 🌐 Websites/
    └── PageCorrelationId.Site             # Front ASP.NET Core MVC
        └── CorrelationId/              # Mécanisme complet de propagation du CID
            ├── SiteCorrelationIdConstants.cs          # Contrat partagé
            ├── SiteCorrelationIdMiddleware.cs         # Génération et lecture du CID
            └── SiteCorrelationIdDelegatingHandler.cs  # Injection du header vers l'API

tests/
├── 🧪 PageCorrelationId.Site.Tests/
│   └── CorrelationId/
│       ├── SiteCorrelationIdMiddlewareTests.cs        # 5 tests — règles métier
│       └── SiteCorrelationIdDelegatingHandlerTests.cs # 3 tests — injection header
│
└── 🧪 PageCorrelationId.Api.Utils.Tests/
    ├── ApiCorrelationId/
    │   └── ApiCorrelationIdMiddlewareTests.cs         # 3 tests — propagation CID
    └── RequestLogging/
        └── RequestLoggingMiddlewareTests.cs           # 2 tests — succès et erreur
```

---

## 🧠 Décisions techniques

### Pourquoi `Sec-Fetch-Mode: navigate` pour détecter une navigation de page ?

Ce header est envoyé **automatiquement par tous les navigateurs modernes** lors d'une navigation réelle — clic sur un lien, saisie d'URL, F5. Il n'est **jamais** envoyé par les appels AJAX, `fetch` ou `XMLHttpRequest`.

| Navigateur | Support |
|---|---|
| Chrome | 76+ |
| Firefox | 90+ |
| Edge | 79+ |
| Safari | 16.4+ |

Cela permet de détecter une nouvelle page **côté serveur**, sans aucune configuration JavaScript, et compatible avec jQuery, DataTables, axios, etc.

### Pourquoi un cookie plutôt qu'un header JavaScript ?

Le cookie est renvoyé **automatiquement** par le navigateur sur chaque requête vers la même origine. Aucun code JavaScript n'est nécessaire pour propager le CID entre la page et ses appels AJAX — ce qui élimine toute surface d'erreur côté front et reste compatible avec n'importe quelle librairie JS.

### Pourquoi un `DelegatingHandler` plutôt que d'injecter le header dans chaque controller ?

Le `SiteCorrelationIdDelegatingHandler` est enregistré **une seule fois** sur le `HttpClient` nommé `ApiClient`. Tous les appels vers l'API héritent automatiquement du header `X-Correlation-ID` — les controllers ne connaissent pas ce mécanisme et n'ont pas à le connaître.

### Pourquoi une classe `SiteCorrelationIdConstants` séparée ?

Évite le couplage implicite entre les consommateurs du contrat (`DelegatingHandler`, controllers) et son implémenteur (`SiteCorrelationIdMiddleware`). Chacun dépend du **contrat**, pas de l'implémentation.

---

## 🚀 Lancer le projet

### Prérequis

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Configuration

Les valeurs de développement sont déjà configurées dans les `appsettings.Development.json` de chaque projet. Vérifiez qu'elles correspondent à vos ports locaux :

**`src/Services/PageCorrelationId.Api/appsettings.Development.json`**
```json
{
  "WebsiteUrl": "https://localhost:5101"
}
```

**`src/Websites/PageCorrelationId.Site/appsettings.Development.json`**
```json
{
  "ApiSettings": {
    "BaseUrl": "https://localhost:5001"
  }
}
```

### Démarrage

```bash
# Terminal 1 — API (https://localhost:5001)
dotnet run --project src/Services/PageCorrelationId.Api

# Terminal 2 — Site (https://localhost:5101)
dotnet run --project src/Websites/PageCorrelationId.Site
```

Ouvrez **`https://localhost:5101`** dans votre navigateur.

### Tests

```bash
dotnet test
```

---

## 🔬 Ce que vous observez

1. Ouvrez **Dashboard 1** → un nouveau CID est généré et affiché sur la page
2. Cliquez sur **⚡ Appel AJAX → GetStats** → le même CID apparaît dans les logs de l'API
3. Ouvrez **Dashboard 2** → un nouveau CID **différent** est généré
4. Les appels AJAX de Dashboard 2 portent leur propre CID, distinct de celui de Dashboard 1

Dans les logs console, filtrez par CID pour retrouver instantanément toutes les requêtes déclenchées par une même ouverture de page :

```
[SITE] PAGE GET /Dashboard — NOUVEAU CID : a3f1c2d4e5b6...
[SITE] NON-PAGE GET /Dashboard/GetStats — CID réutilisé : a3f1c2d4e5b6...
[SITE][HttpClient] → GET https://localhost:5001/api/stats — CID : a3f1c2d4e5b6...
[API]  HTTPS GET /api/stats → 200 in 8ms | CID: a3f1c2d4e5b6...
```

---

<div align="center">

*Projet de démonstration — observabilité et traçabilité distribuée avec ASP.NET Core*

</div>
