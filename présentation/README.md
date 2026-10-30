# Tests de mutation et développement agentique

## Support HTML et préparation locale

- Ouvrir [index.html](index.html) par double-clic pour projeter la séance. Le fichier est autonome, sans installation ni connexion requise pour son contenu. Les liens vers GitHub et la documentation demandent une connexion.
- Naviguer avec les flèches ou les boutons. `N` affiche les notes d’orateur et `F` demande le plein écran. Le menu permet de choisir une séquence. L’impression navigateur fournit un support de repli.
- À la séquence du rapport, charger `mutation-report.json` pour afficher les compteurs et inspecter les vrais survivants. Le traitement se fait localement dans le navigateur.
- Donner le contenu de [PROMPT-LOCAL.md](PROMPT-LOCAL.md) à un agent ouvert sur le dépôt pour exécuter les tests et Stryker, puis enrichir la présentation avec des résultats mesurés.

Le parcours HTML commence par un encadré sur la feature et ouvre le diff réel de la branche. Il poursuit avec le code, une mutation de borne et la preuve attendue dans les tests. Le score de 80 % est étiqueté comme un résultat communiqué tant qu’aucun rapport n’a été chargé. Aucune PR n’était ouverte sur cette branche au moment de sa préparation. Le bloc `session-config` dans le HTML permet de renseigner son URL lorsqu’elle existe.

## Première proposition de présentation

**Titre proposé : « Code et tests écrits par un agent : qu'est-ce qui nous donne confiance ? »**

Public : développeurs, principalement .NET. Format proposé : 25 minutes, dont 6 minutes de démonstration, puis échanges. Ce dossier prépare le contenu et le scénario, avant une éventuelle mise en slides.

**Idée centrale : les tests de mutation apportent une preuve supplémentaire que les tests détectent certaines altérations du comportement. Dans un développement agentique, ils alimentent une boucle de vérification avant la PR et donnent au relecteur des éléments concrets pour juger la robustesse des tests.**

Cette confiance reste liée aux règles spécifiées, aux mutations produites et au périmètre exécuté. Un bon score ne prouve pas que l'agent a compris toutes les règles métier.

## Point de départ et statut des preuves

Le sample est la fonctionnalité de code promotionnel de la branche [`feat/test_v1`](https://github.com/SimonGelbart/eShopOnWeb-test/tree/feat%2Ftest_v1). La lecture du code et des tests porte sur le commit [`b1595c0`](https://github.com/SimonGelbart/eShopOnWeb-test/commit/b1595c04af3fd600d5d59ae9253c2ffa900d2f24).

- Contexte de l'expérience fourni par Simon : un agent a ajouté une fonctionnalité et ses tests, puis les mutants ont été examinés.
- Constats vérifiés dans le dépôt : le code de promotion, ses tests unitaires, un test de transfert vers la commande et un test de persistance sont présents.
- Éléments absents au commit de référence : configuration Stryker et rapport de mutation versionnés, exécution GitHub Actions sur cette branche et PR associée. Le rapport de l'expérience peut avoir été produit localement.
- Ce dossier ne présente aucun score ni mutant survivant comme un résultat mesuré. Les résultats attendus du scénario sont des déductions du code, à confirmer par une exécution.

Les propositions de workflow et les prompts sont des recommandations pour la présentation. La documentation officielle de Stryker décrit les mécanismes de l'outil, sans démontrer à elle seule leur efficacité dans un workflow agentique.

La branche contient désormais un manifeste Stryker 5.0.0 et des configurations MTP pour les tests unitaires : voir [le guide d'exécution](../tests/UnitTests/README.md). Elles remplacent les premières commandes qui laissaient Stryker utiliser VSTest. Leur exécution reste à valider sur une machine .NET.

## Le fil narratif

On part d'une situation familière : l'agent termine une feature, les tests passent, la PR semble prête. On pose alors une question supplémentaire : **si une règle importante se dégrade, quel test échoue ?**

Une mutation rend cette question concrète. Changer `<` en `<=` oblige à se prononcer sur une borne métier. Un survivant devient une piste à examiner. Un mutant tué montre que la suite détecte cette altération, avec un intérêt particulier lorsque le test échoue sur l'assertion métier attendue.

On termine par deux usages complémentaires : l'agent exploite le rapport pour faire converger la feature avant la PR, puis le relecteur retrouve les preuves et les incertitudes dans la PR.

## Proposition de déroulé

### 1. L'agent annonce « terminé, les tests passent » — 2 min

À montrer : une feature, ses tests, un résultat vert.

À dire : déléguer l'implémentation et les tests au même agent peut laisser une interprétation erronée se retrouver des deux côtés. Les tests peuvent être cohérents avec le code tout en oubliant un cas attendu par le métier. Ce risque existe aussi avec des humains. L'automatisation rend utile une vérification systématique avant la revue.

Question au public : « Quelle régression ce test empêcherait-il réellement ? »

### 2. Trois signaux de confiance différents — 2 min

| Signal | Ce qu'il apporte | Limite |
| --- | --- | --- |
| Tests verts | Les vérifications exécutées réussissent sur cette version | Une assertion peut être trop faible ou un scénario absent |
| Couverture | Des portions du code sont exécutées | Exécuter une ligne ne démontre pas que son résultat est vérifié |
| Tests de mutation | La suite détecte certaines modifications artificielles du code | Le résultat dépend des mutations, des tests et du périmètre retenus |

Formulation à privilégier : « Le code est testé et nous avons aussi éprouvé la capacité des tests à réagir à certaines erreurs. »

### 3. Une mutation en C# — 2 min

À montrer : extrait simplifié de `Basket.DiscountAmount()`.

```csharp
if (PromoCode != PromotionCodes.Promo10
    || Subtotal() < PromotionCodes.MinimumSubtotal)
{
    return 0m;
}
```

Mutation : remplacer `<` par `<=`. Avec ce changement, le panier de **50 € exactement** perd sa remise.

Un test qui exige 5 € de remise pour ce panier doit échouer. Stryker appelle ce résultat `Killed`. Si tous les tests exécutés restent verts, le mutant est `Survived`. `NoCoverage` signale une absence de couverture de la mutation. Les timeouts et erreurs techniques demandent une lecture distincte.

### 4. Le sample PROMO10 — 2 min

À montrer : le code réel et les tests de la branche.

| Comportement lu dans l'implémentation | Exemple | Test ou point d'entrée |
| --- | --- | --- |
| Code normalisé en majuscules, espaces retirés | `"  promo10  "` devient `PROMO10` | `AppliesPromo10IgnoringCaseAndSurroundingSpaces` |
| Remise de 10 % dès 50 € de sous-total | 50 € donne 5 € de remise | `KeepsPromoCodeWithoutDiscountBelowMinimumAndRecalculatesAfterQuantityChange` |
| Remise plafonnée à 15 € | 200 € donne un total de 185 € | `CapsDiscountAtFifteenEuros` |
| Arrondi à deux décimales, `AwayFromZero` | 50,05 € donne 5,01 € de remise | `AppliesPromo10IgnoringCaseAndSurroundingSpaces` |
| Nouvelle application du même code sans cumul | Deux applications donnent toujours 10 € sur 100 € | `ApplyingSameCodeAgainDoesNotAccumulateDiscount` |
| Code inconnu rejeté, promotion existante conservée | `UNKNOWN` après `PROMO10` | `RejectsUnknownCodeWithoutChangingExistingPromotion` |
| Suppression du code et de sa remise | Retour à 100 € sans promotion | `RemovesPromotionCodeAndDiscount` |
| Copie du code et du montant vers la commande | 100 € devient 90 € dans la commande | `CopiesPromotionCodeAndDiscountFromBasket` |

Ces comportements décrivent le code actuel. La présentation doit les confronter à la demande métier initiale, notamment pour les bornes et les arrondis.

### 5. Une démonstration qui rend le résultat lisible — 6 min

Scénario détaillé dans [DEMO.md](DEMO.md).

1. Présenter le mutant `<` vers `<=` et faire prédire son effet à 50 €.
2. Comparer sur une table le cas à 50,05 €, qui ne distingue pas les opérateurs, et le cas à 50 € exactement.
3. Exécuter la campagne panier avec toute la suite unitaire, via le runner MTP. Le test existant qui atteint exactement 50 € devrait tuer ce mutant.
4. Montrer le test responsable et son assertion sur les 5 € de remise.

**Le runner xUnit v3 sous MTP ne permet pas d'utiliser le filtre générique `test-case-filter` de Stryker.** La comparaison avec un test limité à 50,05 € reste une explication du pouvoir discriminant des données, sans rapport de mutation filtré. Le test de borne existe déjà dans la branche. La démonstration montre sa valeur sans prétendre découvrir une lacune historique.

Si tous les mutants survivent, lancer le mode diagnostic et vérifier l'exécution réelle des tests avant d'interpréter le score. Les difficultés connues de Stryker avec xUnit v3 via VSTest rendent cette vérification nécessaire.

Si le rapport original est disponible, ajouter ensuite un véritable survivant : emplacement, altération, règle concernée, décision et résultat après correction. Choisir un seul cas lisible.

### 6. Un survivant ouvre une enquête — 2 min

| Analyse | Action utile |
| --- | --- |
| Scénario métier absent ou assertion insuffisante | Ajouter un exemple discriminant avec un attendu issu de la règle métier |
| Règle comprise de travers | Corriger l'implémentation et les tests à partir de la spécification |
| Règle ambiguë ou absente | Formuler une question au responsable métier, avec deux comportements possibles |
| Mutation équivalente sur le domaine valide | Justifier l'équivalence et faire relire cette justification |
| Problème d'outil, de runner ou timeout | Diagnostiquer et relancer avant de conclure |

Exemple de question utile : « Le seuil porte-t-il sur le montant avant ou après arrondi ? » Le code actuel arrondit `Subtotal()` avant la comparaison. Cette lecture ne suffit pas à établir l'intention métier.

### 7. Une étape de convergence avant la PR — 3 min

Proposition de workflow :

1. Le développeur et le métier explicitent les règles et des exemples attendus.
2. L'agent implémente la feature et ses tests, puis exécute les contrôles habituels.
3. Avant d'ouvrir la PR, une étape dédiée exécute les mutations sur le périmètre pertinent.
4. Un agent d'analyse lit les survivants et les confronte aux règles initiales. Il propose un test discriminant, une correction ou une question métier, avec justification.
5. Les corrections autorisées passent par une nouvelle exécution des tests et des mutations. Une ambiguïté métier reste explicite jusqu'à sa résolution humaine.
6. La PR rassemble le résultat et les décisions pour le relecteur.

On peut confier l'analyse à un autre agent ou à une nouvelle session pour obtenir un regard supplémentaire. Cela ne crée pas une source indépendante de vérité métier. Le prompt, les critères d'arrêt et un modèle de restitution figurent dans [WORKFLOW-AGENT.md](WORKFLOW-AGENT.md).

### 8. Ce que le relecteur gagne — 2 min

À montrer : un extrait de description de PR avec le commit analysé, le périmètre, le rapport et les décisions sur les survivants.

Le relecteur peut vérifier une chaîne concrète : **règle métier, exemple attendu, mutation, test qui la détecte**. Il peut concentrer son attention sur les règles discutées et les survivants encore pertinents.

Un mutant tué peut aussi échouer sur une exception incidente. Pour les règles critiques, inspecter la raison de l'échec apporte une meilleure assurance qu'un compteur seul.

### 9. Coût et limites — 2 min

Commencer par le domaine modifié, les calculs et les règles à risque. Ajuster le périmètre au temps disponible, puis prévoir des campagnes plus larges. Stryker propose `mutate` pour cibler des fichiers et `since` pour cibler les changements. Un rapport partiel doit être présenté comme tel.

Le score dépend du périmètre et des exclusions. Stryker calcule le score sur les mutants valides, avec les mutants tués et les timeouts comme détectés. Un score de 100 % peut coexister avec une règle absente du code, une mauvaise spécification ou des cas non modélisés par les opérateurs de mutation.

Les tests de mutation complètent la revue humaine et les autres niveaux de tests. Ils ne démontrent pas la sécurité globale, les performances ou le bon fonctionnement d'une migration en base réelle.

### 10. La pratique à emporter — 2 min

« Avant de proposer une feature agentique en PR, nous demandons aussi quelles altérations ses tests détectent et quelles règles les survivants nous obligent à clarifier. »

Proposition de règle d'équipe : toute mutation survivante pertinente sur une règle métier critique reçoit une correction, une justification relue ou une question explicite avant la validation de la PR. Le seuil numérique vient après cette lecture.

## Préparation de la version suivante

- Récupérer le prompt métier initial pour distinguer les exigences explicites des choix faits par l'agent.
- Récupérer le rapport de mutation de l'expérience, son commit, sa version Stryker et sa configuration.
- Choisir un survivant réel, si le rapport en contient un pertinent, et documenter sa résolution.
- Répéter la démonstration et garder les rapports HTML comme solution de repli.
- Adapter ensuite cette trame au temps de parole retenu.

## Sources

### Dépôt, figé au commit analysé

- [Basket.cs](https://github.com/SimonGelbart/eShopOnWeb-test/blob/b1595c04af3fd600d5d59ae9253c2ffa900d2f24/src/ApplicationCore/Entities/BasketAggregate/Basket.cs)
- [PromotionCodes.cs](https://github.com/SimonGelbart/eShopOnWeb-test/blob/b1595c04af3fd600d5d59ae9253c2ffa900d2f24/src/ApplicationCore/Entities/BasketAggregate/PromotionCodes.cs)
- [BasketPromotion.cs](https://github.com/SimonGelbart/eShopOnWeb-test/blob/b1595c04af3fd600d5d59ae9253c2ffa900d2f24/tests/UnitTests/ApplicationCore/Entities/BasketTests/BasketPromotion.cs)
- [Order.cs](https://github.com/SimonGelbart/eShopOnWeb-test/blob/b1595c04af3fd600d5d59ae9253c2ffa900d2f24/src/ApplicationCore/Entities/OrderAggregate/Order.cs)
- [CreateOrder.cs](https://github.com/SimonGelbart/eShopOnWeb-test/blob/b1595c04af3fd600d5d59ae9253c2ffa900d2f24/tests/UnitTests/ApplicationCore/Services/OrderServiceTests/CreateOrder.cs)
- [PromotionCodePersistence.cs](https://github.com/SimonGelbart/eShopOnWeb-test/blob/b1595c04af3fd600d5d59ae9253c2ffa900d2f24/tests/IntegrationTests/Repositories/PromotionRepositoryTests/PromotionCodePersistence.cs)

### Documentation officielle, consultée le 30 octobre 2026

- [Démarrer avec Stryker.NET](https://stryker-mutator.io/docs/stryker-net/getting-started/)
- [Configuration : projets, filtres et périmètre](https://stryker-mutator.io/docs/stryker-net/configuration/)
- [Mutations disponibles en .NET](https://stryker-mutator.io/docs/stryker-net/mutations/)
- [États des mutants et métriques](https://stryker-mutator.io/docs/mutation-testing-elements/mutant-states-and-metrics/)
- [Mutants équivalents](https://stryker-mutator.io/docs/mutation-testing-elements/equivalent-mutants/)
- [MTP avec xUnit v3](https://xunit.net/docs/getting-started/v3/microsoft-testing-platform)
- [Difficultés de Stryker avec xUnit v3 via VSTest, issue #3117](https://github.com/stryker-mutator/stryker-net/issues/3117)
