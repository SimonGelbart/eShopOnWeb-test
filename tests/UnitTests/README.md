# Tests unitaires et mutations avec xUnit v3 / MTP

## Installation et premier lancement

Depuis la racine du dépôt, avec un SDK .NET 10 compatible avec `global.json` :

```bash
dotnet tool restore
dotnet tool list --local
dotnet run --project tests/UnitTests/UnitTests.csproj
```

Le manifeste racine fixe `dotnet-stryker` à **5.0.0**. Ne pas recréer de manifeste ni installer une autre version globale pour ces commandes. Le runner MTP et le projet exécutable sont activés dans `UnitTests.csproj`. xUnit v3 fournit le support MTP via ses dépendances existantes.

Vérifier que la sortie de `dotnet run` mentionne `Microsoft.Testing.Platform`, un nombre non nul de tests exécutés et zéro échec. Une compilation réussie seule ne valide pas les tests.

Puis, depuis `tests/UnitTests` :

```bash
dotnet stryker
```

Stryker charge automatiquement `stryker-config.json`. La commande cible `ApplicationCore`, mute `Basket.cs` et utilise **MTP** avec les tests de `UnitTests`. Ouvrir le rapport HTML au chemin indiqué dans la console. Le rapport JSON accompagne l'analyse par un agent.

Ces commandes portent sur les tests unitaires. Elles ne nécessitent pas l'installation d'un navigateur Playwright. La compilation de leurs projets référencés doit néanmoins réussir.

## Configurations disponibles

Toutes les commandes de ce tableau s'exécutent depuis `tests/UnitTests`.

| Fichier | Commande | Périmètre et objectif |
| --- | --- | --- |
| `stryker-config.json` | `dotnet stryker` | `Basket.cs`, niveau `Standard`, sélection par couverture de chaque test |
| `stryker-config.feature.json` | `dotnet stryker --config-file stryker-config.feature.json` | `Basket`, `PromotionCodes`, `Order` et `OrderService`, niveau `Advanced` |
| `stryker-config.diagnostic.json` | `dotnet stryker --config-file stryker-config.diagnostic.json --log-to-file` | Même code et niveau que la campagne panier, toute la suite pour chaque mutant, un seul worker |

Les trois configurations arrêtent Stryker si l'exécution de référence échoue, via `break-on-initial-test-failure`. Aucun filtre de tests ni exclusion de mutations n'est appliqué. Le périmètre feature utilise uniquement la suite unitaire, sans mesurer les tests d'intégration ni la persistance.

Le niveau `Advanced` étend les mutations de la campagne feature. Comparer les scores du mode panier et du mode diagnostic, qui ont le même périmètre et le même niveau. Le score de la campagne feature couvre un ensemble différent.

## Pourquoi MTP est explicite

Sans configuration, Stryker utilise VSTest par défaut. L'issue amont [#3117](https://github.com/stryker-mutator/stryker-net/issues/3117) décrit les difficultés de couverture et d'activation des mutants avec le processus séparé de xUnit v3. Un rapport où tous les mutants survivent peut donc traduire un problème d'exécution. Il ne permet pas, à lui seul, de conclure que toutes les assertions sont faibles.

Le runner MTP constitue la voie retenue ici. Stryker 5.0.0 contient des corrections de couverture MTP et un exemple d'intégration avec `xunit.v3` 3.2.2, qui est aussi la version du dépôt. Cela justifie le choix de configuration, sans constituer un résultat d'exécution de ce sample.

`UseMicrosoftTestingPlatformRunner` sélectionne l'interface MTP de l'exécutable xUnit. Le champ `test-runner` sélectionne le runner employé par Stryker. La sélection du runner de `dotnet test` dans .NET 10 est un mécanisme supplémentaire, piloté par `global.json`. Pour vérifier la même voie d'exécution que celle retenue pour Stryker, utiliser ici `dotnet run --project tests/UnitTests/UnitTests.csproj`.

## Si tous les mutants survivent encore

1. Vérifier la version locale avec `dotnet tool list --local`, puis le runner MTP annoncé par Stryker et le nombre de tests réellement exécutés.
2. Vérifier que le rapport contient bien `Basket.cs` et les changements de `DiscountAmount()`.
3. Exécuter la configuration diagnostic. Elle enlève l'optimisation par couverture et sérialise les exécutions, au prix d'un temps plus long.
4. Conserver la console, le journal produit par `--log-to-file` et les rapports HTML/JSON. Comparer les mêmes mutations par fichier, emplacement et remplacement.

Contrôle particulièrement lisible : le mutant `<` vers `<=` sur la comparaison à `MinimumSubtotal` doit être détecté par le test existant qui atteint exactement 50 €. C'est un résultat attendu par lecture du code, à confirmer dans le rapport. Si cette mutation survit malgré l'exécution effective de ce test, approfondir l'activation du mutant et le chargement de l'assembly avant de modifier les assertions.

Si le mode diagnostic détecte des mutations qui survivaient en mode normal, l'association entre tests et mutations est une piste à examiner. S'il ne les détecte pas non plus, consulter les journaux et vérifier quel code s'exécute. Un changement de runner ne garantit pas à lui seul un résultat fiable.

Ne pas utiliser `--test-case-filter` avec ces configurations : le runner xUnit v3 sous MTP n'expose pas le filtre générique `--filter` que Stryker attend. L'ancienne démonstration filtrée à 50,05 € a été remplacée dans [DEMO.md](../../présentation/DEMO.md).

## Validation de cette configuration

Les fichiers JSON, leurs périmètres et les propriétés du projet ont été contrôlés statiquement et comparés à la documentation et à l'exemple d'intégration amont. L'environnement de rédaction ne dispose pas du CLI .NET : aucune campagne de mutation ni compilation n'a été exécutée ici. Les premières mesures restent à effectuer avec les commandes ci-dessus.

Références : [configuration Stryker.NET](https://stryker-mutator.io/docs/stryker-net/configuration/), [release 5.0.0](https://github.com/stryker-mutator/stryker-net/releases/tag/dotnet-stryker%405.0.0), [exemple xUnit MTP de cette release](https://github.com/stryker-mutator/stryker-net/blob/dotnet-stryker%405.0.0/integrationtest/TargetProjects/MicrosoftTestPlatform/UnitTests.XUnit/UnitTests.XUnit.csproj), [configuration MTP de xUnit v3](https://xunit.net/docs/getting-started/v3/microsoft-testing-platform).
