# Démonstration proposée : la borne de 50 €

## Objectif et statut

En six minutes, montrer comment un mutant relie une règle métier à une vérification précise, puis comment donner ce résultat à un agent et à un relecteur de PR.

**Scénario préparatoire, non exécuté lors de la rédaction.** L'environnement de rédaction ne dispose pas du CLI `dotnet`. Aucun rapport ni score n'a été produit ici. Les commandes et les résultats attendus doivent être validés sur une machine .NET avant la présentation.

La lecture du comportement métier repose sur le commit `b1595c04af3fd600d5d59ae9253c2ffa900d2f24`. Pour exécuter la démonstration, utiliser la branche `feat/test_v1` à jour, qui contient désormais le manifeste Stryker et les configurations MTP. Consigner le SHA réellement exécuté.

## Préparation de la machine

Prérequis : SDK .NET 10 compatible avec `global.json` et dépendances restaurables. Les tests unitaires utilisent xUnit v3. Le projet référence `ApplicationCore` et `Web`, d'où la sélection explicite du projet à muter dans les configurations.

Depuis la racine du dépôt :

```bash
dotnet --info
dotnet tool restore
dotnet tool list --local
dotnet run --project tests/UnitTests/UnitTests.csproj
```

Le manifeste racine fixe Stryker à **5.0.0**. Vérifier que l'exécution de référence annonce le runner `Microsoft.Testing.Platform`, exécute des tests et termine sans échec. Arrêter la préparation si seuls les projets compilent, si aucun test ne s'exécute ou si des tests échouent.

Les configurations et la procédure de diagnostic figurent dans [tests/UnitTests/README.md](../tests/UnitTests/README.md). La campagne unitaire ne nécessite pas de navigateur Playwright.

## Le changement de comportement

Le code actuel exclut la remise lorsque le sous-total est strictement inférieur à 50 € :

```csharp
Subtotal() < PromotionCodes.MinimumSubtotal
```

Stryker.NET documente la mutation de `<` vers `<=`. Cette mutation exclut aussi le panier à 50 € exactement.

| Panier avec PROMO10 | Remise du code actuel | Remise avec le mutant `<` vers `<=` |
| --- | --- | --- |
| 25 € | 0 € | 0 € |
| 50 € | 5 € | 0 € |
| 50,05 € | 5,01 € | 5,01 € |

Le cas à 50,05 € ne distingue pas les deux implémentations. Le cas à 50 € les distingue immédiatement.

**Cette comparaison est une lecture des données et du code.** Elle ne décrit pas un rapport produit avec un seul test. L'ancienne proposition qui utilisait `--test-case-filter` a été retirée : le runner xUnit v3 sous MTP n'expose pas le filtre générique que Stryker attend.

## Exécution de la campagne panier

Depuis `tests/UnitTests` :

```bash
dotnet stryker
```

La configuration versionnée `stryker-config.json` sélectionne :

- Le projet à muter `ApplicationCore.csproj` et le fichier `Basket.cs`.
- Le runner `mtp`, le niveau de mutation `Standard` et la couverture `perTest`.
- Les rapports HTML et JSON, avec arrêt si l'exécution de référence échoue.

Dans le rapport HTML, ouvrir `Basket.DiscountAmount()` et sélectionner la mutation de `<` vers `<=` sur la comparaison du sous-total au minimum.

Le test existant `KeepsPromoCodeWithoutDiscountBelowMinimumAndRecalculatesAfterQuantityChange` :

1. Ajoute un article à 25 € et applique `PROMO10`.
2. Vérifie l'absence de remise sous le minimum.
3. Ajoute une unité du même article, ce qui porte le sous-total à 50 €.
4. Exige une remise de 5 € et un total de 45 €.

**Résultat attendu par lecture du code : `Killed`.** Le mutant renverrait 0 € de remise à l'étape 4. Vérifier dans le rapport quel test l'a tué. Si plusieurs tests le détectent, le runner peut en signaler un autre.

La démonstration montre la valeur d'un test existant. Elle ne prétend pas découvrir ou corriger une lacune historique de l'expérience initiale.

## Si tout survit

Le problème connu avec xUnit v3 via VSTest peut produire une mesure trompeuse. Le choix explicite de MTP traite cette incompatibilité. L'absence de mutations tuées reste un signal à diagnostiquer avant de conclure sur la qualité des tests.

Depuis `tests/UnitTests` :

```bash
dotnet stryker --config-file stryker-config.diagnostic.json --log-to-file
```

Ce mode conserve le même périmètre et le même niveau. Il exécute toute la suite pour chaque mutant, sans sélection par couverture, avec un seul worker. Il peut prendre plus de temps.

Comparer le mutant de borne entre les rapports par fichier, emplacement et remplacement. Ne pas supposer que son identifiant reste stable. Conserver les journaux et vérifier les tests réellement exécutés, le runner et l'assembly chargé si ce mutant survit encore.

## Passage au travail de l'agent

Ouvrir ensuite [WORKFLOW-AGENT.md](WORKFLOW-AGENT.md). Demander à l'agent d'expliquer le résultat à partir de la règle, des données et des assertions. Une bonne restitution cite la borne exacte, le test et le montant attendu.

Avec le rapport complet de l'expérience initiale, choisir un survivant réel et demander :

- Quel comportement observable change pour une entrée valide ?
- Quel attendu découle de la règle métier initiale ?
- Un test manque-t-il, ou la règle est-elle incertaine ?
- Quelle preuve justifie une éventuelle équivalence ?

Une question métier possible porte sur l'arrondi du seuil : un sous-total brut de 49,995 € devient 50,00 € avec l'implémentation actuelle de `Subtotal()`. Cela ouvre une discussion sur la spécification. Ce cas n'est pas présenté comme un mutant observé ou comme un défaut confirmé.

## Extension à la feature

Depuis `tests/UnitTests` :

```bash
dotnet stryker --config-file stryker-config.feature.json
```

Cette campagne inclut `Basket`, `PromotionCodes`, `Order` et `OrderService` au niveau `Advanced`, avec la suite unitaire. Elle couvre un ensemble de mutations différent de la campagne panier. Le transfert vers la commande et la persistance méritent aussi des vérifications d'intégration. Le test existant `PromotionCodePersistence` utilise EF Core InMemory et ne démontre pas l'exécution de la migration sur une base relationnelle réelle.

## Éléments à enregistrer après répétition

| Élément | Valeur à conserver |
| --- | --- |
| Commit du code, des tests et de la configuration | SHA exact |
| Environnement | SDK, Stryker 5.0.0, runner MTP |
| Sélection | Fichiers mutés, suite unitaire, niveau et mode de couverture |
| Résultat | Rapport HTML/JSON et statut observé du mutant de borne |
| Preuve | Test détecteur et assertion qui échoue |
| Diagnostic éventuel | Rapport du mode sans couverture et journal |
| Limites | Périmètre partiel et éventuelles erreurs techniques |

Garder les rapports comme support de repli pour le jour de la présentation. Si le résultat diffère de la prédiction, examiner l'exécution avant de raconter l'expérience.

Sources techniques : [configuration Stryker.NET](https://stryker-mutator.io/docs/stryker-net/configuration/), [mutations disponibles](https://stryker-mutator.io/docs/stryker-net/mutations/), [MTP avec xUnit v3](https://xunit.net/docs/getting-started/v3/microsoft-testing-platform), [issue amont #3117](https://github.com/stryker-mutator/stryker-net/issues/3117). Les références exactes au sample figurent dans le [README](README.md#sources).
