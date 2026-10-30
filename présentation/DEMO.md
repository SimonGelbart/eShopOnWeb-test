# Démonstration proposée : la borne de 50 €

## Objectif et statut

En six minutes, montrer comment un mutant relie une règle métier à une vérification précise, puis comment donner ce résultat à un agent et à un relecteur de PR.

**Scénario préparatoire, non exécuté lors de la rédaction.** L'environnement de rédaction ne dispose pas du CLI `dotnet`. Aucun rapport ni score n'a été produit. Les commandes et les résultats attendus doivent être validés sur une machine .NET avant la présentation.

Base du scénario : commit `b1595c04af3fd600d5d59ae9253c2ffa900d2f24` de `feat/test_v1`. Les fichiers de ce dossier n'altèrent pas le code applicatif ni les tests.

## Ce que l'on montre

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

## Préparation de la machine

Prérequis : SDK compatible avec le `global.json` du dépôt, dépendances restaurables et version de Stryker compatible avec le runner de tests. Le dépôt demande la famille SDK .NET 10 et les tests unitaires utilisent xUnit v3 avec l'adaptateur Visual Studio. Le projet de tests référence `ApplicationCore` et `Web`, d'où la sélection explicite du projet à muter.

Depuis la racine du dépôt, travailler dans un checkout de démonstration sur le commit indiqué. Pour une nouvelle installation locale de Stryker, créer un manifeste à la racine si aucun n'existe à cet endroit :

```bash
dotnet --info
dotnet new tool-manifest
dotnet tool install dotnet-stryker
dotnet tool list --local
dotnet restore tests/UnitTests/UnitTests.csproj
dotnet test tests/UnitTests/UnitTests.csproj
```

Si un manifeste existe déjà à la racine, utiliser celui-ci et ne pas le recréer. S'il contient déjà Stryker, exécuter `dotnet tool restore`. La première installation choisit une version disponible : conserver ensuite la version exacte dans le manifeste pour la répétition et l'exécution en CI. Le manifeste existant sous `src/Web/.config` ne constitue pas un manifeste à la racine.

Arrêter la préparation si les tests de référence échouent ou si aucun test n'est découvert. Résoudre le problème avant de lire des résultats de mutation.

Créer, **dans le checkout de démonstration**, le fichier `tests/UnitTests/stryker-config.demo.json` suivant :

```json
{
  "stryker-config": {
    "project": "ApplicationCore.csproj",
    "test-runner": "vstest",
    "mutate": [
      "**/Entities/BasketAggregate/Basket.cs"
    ],
    "mutation-level": "Standard",
    "reporters": ["progress", "html", "json"]
  }
}
```

Ce périmètre sert uniquement à rendre la démonstration courte. Il ne couvre pas toute la fonctionnalité. Le niveau `Standard` inclut les mutations de comparaison nécessaires à ce scénario. Vérifier dans le rapport que `Basket.cs` et le mutant de borne ont bien été sélectionnés.

## Première exécution : un test existant au-dessus du seuil

Depuis `tests/UnitTests` :

```bash
dotnet stryker --config-file stryker-config.demo.json --test-case-filter "FullyQualifiedName~AppliesPromo10IgnoringCaseAndSurroundingSpaces"
```

Le test sélectionné construit un panier à 50,05 € et vérifie notamment une remise de 5,01 €. Annoncer clairement que l'on réduit volontairement la suite pour isoler l'explication.

Dans le rapport HTML, ouvrir `Basket.DiscountAmount()`, puis sélectionner la mutation de `<` vers `<=` sur la comparaison du sous-total au minimum.

**Résultat attendu par lecture du code : `Survived`.** Le seul test retenu utilise un montant supérieur à la borne. Consigner le résultat réellement observé lors de la répétition. Ne pas généraliser ce résultat à la suite complète.

Conserver le rapport HTML et JSON de cette exécution avant la suivante. Le répertoire de sortie exact est indiqué par Stryker.

## Deuxième exécution : la suite unitaire complète

Depuis le même répertoire, retirer le filtre de tests :

```bash
dotnet stryker --config-file stryker-config.demo.json
```

Le périmètre de code reste `Basket.cs`. Toute la suite unitaire est maintenant disponible pour détecter les mutations de ce fichier.

Le test existant `KeepsPromoCodeWithoutDiscountBelowMinimumAndRecalculatesAfterQuantityChange` :

1. Ajoute un article à 25 € et applique `PROMO10`.
2. Vérifie l'absence de remise sous le minimum.
3. Ajoute une unité du même article, ce qui porte le sous-total à 50 €.
4. Exige une remise de 5 € et un total de 45 €.

**Résultat attendu pour le même mutant : `Killed`.** Le mutant renverrait 0 € de remise à l'étape 4. Vérifier dans le rapport quel test l'a tué. Si plusieurs tests le détectent, le runner peut en signaler un autre.

Comparer les mutations par fichier, emplacement et remplacement. Ne pas supposer que leurs identifiants restent stables entre les exécutions.

## Ce que la démonstration permet d'affirmer

« Pour cette mutation de borne, le test de 50,05 € reste vert. La suite complète contient un test à 50 € qui doit détecter la régression. Le rapport confirme cette capacité quand on l'exécute. »

Le passage de survivant à tué vient ici de l'élargissement de la suite exécutée. Aucun test manquant n'a été découvert ni ajouté dans l'expérience d'origine par ce scénario. Il ne faut pas présenter ces deux exécutions comme une amélioration historique du dépôt.

## Passage au travail de l'agent

Ouvrir ensuite [WORKFLOW-AGENT.md](WORKFLOW-AGENT.md). Demander à l'agent d'expliquer le résultat à partir de la règle, des données et des assertions. Une bonne restitution cite la borne exacte, le test et le montant attendu.

Avec le rapport complet de l'expérience initiale, choisir un survivant réel et demander :

- Quel comportement observable change pour une entrée valide ?
- Quel attendu découle de la règle métier initiale ?
- Un test manque-t-il, ou la règle est-elle incertaine ?
- Quelle preuve justifie une éventuelle équivalence ?

Une question métier possible porte sur l'arrondi du seuil : un sous-total brut de 49,995 € devient 50,00 € avec l'implémentation actuelle de `Subtotal()`. Cela ouvre une discussion sur la spécification. Ce cas n'est pas présenté comme un mutant observé ou comme un défaut confirmé.

## Éléments à enregistrer pour une démonstration fiable

| Élément | Valeur à conserver après répétition |
| --- | --- |
| Commit du code et des tests | SHA exact |
| Environnement | SDK, version Stryker, runner |
| Sélection | Configuration, fichier muté, filtre de tests éventuel |
| Première exécution | Rapport et statut observé du mutant de borne |
| Deuxième exécution | Rapport, statut observé et test détecteur |
| Limites | Périmètre partiel et éventuelles erreurs techniques |

Garder les rapports comme support de repli pour le jour de la présentation. Si le résultat diffère de la prédiction, examiner la version, le filtre, la mutation produite et les tests découverts avant de raconter l'expérience.

## Extension possible à la feature complète

Après la démonstration, proposer une campagne qui inclut `Basket`, `PromotionCodes`, `Order` et `OrderService`, avec les tests pertinents. Le transfert vers la commande et la persistance méritent une vérification séparée. Le test `PromotionCodePersistence` utilise EF Core InMemory et ne démontre pas l'exécution de la migration sur une base relationnelle réelle.

Éviter un objectif de score global sans préciser ce périmètre. Pour une analyse limitée aux changements, étudier `since` dans la version Stryker retenue et conserver une campagne plus large à une fréquence adaptée.

Sources techniques : [configuration Stryker.NET](https://stryker-mutator.io/docs/stryker-net/configuration/), [mutations disponibles](https://stryker-mutator.io/docs/stryker-net/mutations/), [installation](https://stryker-mutator.io/docs/stryker-net/getting-started/). Les références exactes au sample figurent dans le [README](README.md#sources).
