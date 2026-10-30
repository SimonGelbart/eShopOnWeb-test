# Une analyse des mutations avant la PR

Proposition de contrat pour un agent autonome chargé d'aider à faire converger une feature. Ce document ne configure pas d'agent ni de pipeline dans le dépôt.

## Place dans le développement

Déclencher cette étape lorsque la feature et ses tests passent les contrôles habituels, avant l'ouverture de la PR. La reprendre après toute modification du code ou des tests qui affecte le résultat. L'agent peut exécuter les commandes et proposer ou appliquer les corrections autorisées. Une règle métier ambiguë nécessite une décision de la personne compétente.

## Entrées nécessaires

- Demande métier initiale, critères d'acceptation et exemples de référence.
- Commit, diff de la feature et tests concernés.
- Commande de tests de référence et résultat.
- Version Stryker, configuration et rapport JSON/HTML correspondant au code et aux tests analysés.
- Périmètre des fichiers et tests retenus, exclusions et éventuels résultats réutilisés.

En l'absence de spécification, l'agent peut décrire les comportements lus dans le code et proposer des questions. Il ne doit pas présenter cette lecture comme la confirmation de l'intention métier.

## Prompt proposé

```text
Tu analyses la qualité des tests d'une feature avant sa proposition en PR.

Objectif : établir quelles altérations de comportement la suite détecte,
puis repérer les règles métier insuffisamment vérifiées ou ambiguës.

Commence par vérifier la correspondance entre le commit, le code, les tests,
la configuration et le rapport. Vérifie que les tests de référence passent
et que le périmètre annoncé correspond au périmètre exécuté.

Pour chaque mutant Survived ou NoCoverage pertinent pour la feature :
1. Cite le fichier, l'emplacement, le code original et le remplacement.
2. Décris le changement observable avec une entrée valide et un résultat.
3. Relie-le à une exigence ou indique que l'exigence manque.
4. Explique pourquoi les tests actuels ne discriminent pas les comportements.
5. Propose une action justifiée : test, correction, question métier,
   équivalence démontrée ou diagnostic d'outil.

Construis les attendus à partir des critères métier et d'exemples calculés
indépendamment de la méthode testée. Ne reprends pas le calcul de production
comme seul oracle du test. Ne change pas une règle pour rendre le rapport vert.

Pour les mutants Killed qui touchent une règle critique, inspecte quelques
tests détecteurs : vérifient-ils l'effet métier attendu, ou échouent-ils
seulement à cause d'une exception incidente ? Signale la différence.

Examine aussi les règles absentes du code ou de la spécification : les mutations
ne peuvent pas nécessairement les faire apparaître. Propose des questions
précises sur les bornes, les arrondis et les transitions d'état pertinentes.

Dans le périmètre de modification autorisé, ajoute des tests discriminants
ou corrige les défauts confirmés. Montre que le test passe sur le comportement
attendu et détecte l'altération concernée lorsque c'est vérifiable.
Relance les tests de référence et les mutations après tes changements.

N'exclus pas une mutation et n'affaiblis pas une assertion pour augmenter
le score. Si une exclusion semble justifiée, propose-la avec sa preuve et
sa portée pour la revue. Distingue les erreurs techniques des résultats métier.

Arrête-toi avec une restitution explicite si une ambiguïté métier empêche
de choisir un attendu, si le budget convenu est atteint ou si l'outillage
empêche une mesure fiable. N'invente ni résultat ni résolution.

Rends un bilan court : règles examinées, preuves, changements, survivants
restants, limites du périmètre et décisions humaines attendues.
```

## Exemple de restitution sur PROMO10

Ce tableau illustre le format attendu. Les statuts ci-dessous sont des prédictions à confirmer avec les rapports de la démonstration.

| Élément | Restitution attendue |
| --- | --- |
| Mutation | `Basket.DiscountAmount()` : `<` devient `<=` devant `MinimumSubtotal` |
| Règle à confirmer dans les critères métier | La remise commence à 50 € inclus |
| Exemple discriminant | Panier à 50 €, code PROMO10, remise attendue de 5 € |
| Exécution filtrée à 50,05 € | Survie attendue, le test ne distingue pas la borne |
| Suite unitaire complète | Détection attendue par le test existant de changement de quantité |
| Action | Confirmer le rapport et citer le test, sans ajouter un doublon inutile |
| Question métier distincte | Le minimum de 50 € s'évalue-t-il avant ou après arrondi du sous-total ? |

## Conditions de sortie proposées

- Les tests habituels passent et le rapport correspond à la dernière version pertinente du code et des tests.
- Chaque survivant pertinent pour une règle critique a une décision documentée. Les ambiguïtés restantes sont visibles et empêchent de déclarer cette règle validée.
- Les exclusions et les équivalences proposées ont une justification inspectable.
- Le bilan précise les fichiers et les tests analysés, les erreurs techniques et le coût d'exécution observé.

Le nombre de tentatives et le budget de temps se fixent avant l'exécution. L'agent ne boucle pas sans fin sur un objectif de 100 %.

## Informations à joindre à la PR

Modèle à remplir avec des résultats mesurés :

```markdown
### Vérification par mutation

- Version analysée : <SHA du code et des tests>.
- Environnement : <SDK, version Stryker et runner>.
- Périmètre : <projet, fichiers, suite de tests, filtre éventuel>.
- Commande/configuration : <commande et lien>.
- Tests de référence : <résultat et lien>.
- Rapport : <lien vers l'artefact HTML/JSON accessible au relecteur>.
- Résultat : <killed, survived, no coverage, timeouts, erreurs et exclusions>.
- Score : <score mesuré et périmètre auquel il s'applique>.
- Règles critiques éprouvées : <règle, exemple, mutation et test détecteur>.
- Survivants restants : <analyse et décision pour chaque cas pertinent>.
- Questions métier : <question, impact et décision attendue>.
- Limites : <périmètre non analysé, budget atteint ou problème d'outil>.
```

## Lecture par le relecteur

Vérifier d'abord que le rapport concerne la version revue et que le périmètre couvre les règles modifiées. Examiner ensuite les mutations significatives et la justification des survivants. Pour une règle sensible, ouvrir un test détecteur et lire son assertion.

Le bénéfice recherché est une revue mieux informée : le relecteur voit quelles altérations les tests détectent et quelles décisions restent à prendre. Il conserve la responsabilité de confronter le comportement proposé aux attentes du produit.
