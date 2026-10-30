# Prompt pour préparer la séance depuis le dépôt local

Ouvre le dépôt dans ton environnement d’agent, puis copie le prompt ci-dessous. Il autorise l’exécution locale des tests et de Stryker pour enrichir la présentation avec des preuves réelles. Il porte sur la préparation de la séance, sans publication automatique.

---

Tu travailles dans le dépôt `SimonGelbart/eShopOnWeb-test`, sur la branche `feat/test_v1`.

## Objectif

Préparer une séance technique de 25 minutes pour un public principalement .NET sur l’intérêt des tests de mutation dans un développement agentique. Produire un support HTML autonome et enrichir le support existant avec les résultats réellement observés sur cette machine.

Le fil de la séance est une PR concrète : un agent a ajouté la fonctionnalité PROMO10 et ses tests. Nous voulons comprendre quelles preuves permettent de proposer cette PR à la revue et comment analyser les règles métier potentiellement mal comprises avant son ouverture.

Tu peux lire le dépôt, exécuter les tests et Stryker, modifier les fichiers de présentation et effectuer une expérience de test isolée si elle est nécessaire à la démonstration. Préserve les modifications locales déjà présentes. Termine avec un résultat local inspectable, sans créer de PR ni publier automatiquement.

## 1. Comprendre l’état de départ

- Lis les instructions `AGENTS.md` applicables, puis vérifie la branche et l’état du répertoire de travail. Ne supprime ni ne remplace des modifications préexistantes.
- Lis `présentation/index.html`, `présentation/README.md`, `présentation/DEMO.md`, `présentation/WORKFLOW-AGENT.md` et les configurations sous `tests/UnitTests`.
- Examine la fonctionnalité, notamment `Basket`, `PromotionCodes`, `Order`, `OrderService` et leurs tests.
- Recherche une PR existante liée à la branche si ton environnement dispose d’un accès GitHub. Sinon, utilise le lien de comparaison de branche déjà présent. N’invente pas de numéro de PR.
- Utilise la demande métier initiale si elle est disponible. À défaut, indique que les règles décrivent le sample et restent à confronter à cette demande.

Le code actuel prévoit une remise de 10 % dès 50 € de sous-total, plafonnée à 15 €, avec un arrondi à deux décimales `AwayFromZero`. Ces comportements sont un point de lecture du code, sans constituer une preuve autonome de l’intention métier.

## 2. Recueillir une exécution fiable

Un score de 80 % a été communiqué par Simon. Tant que tu n’as pas lu le rapport correspondant, conserve l’étiquette « résultat communiqué » et n’invente pas de compteurs, de périmètre ou de mutant survivant.

Cherche d’abord un rapport local existant. Réutilise-le si tu peux établir qu’il correspond au code, aux tests et à la configuration actuels. Sinon, exécute depuis la racine du dépôt :

```bash
dotnet --info
dotnet tool restore
dotnet tool list --local
dotnet run --project tests/UnitTests/UnitTests.csproj
```

Vérifie que des tests sont effectivement exécutés avec MTP et qu’ils passent. Arrête les étapes dépendantes si les tests de référence échouent. N’interprète pas une simple compilation comme une exécution de tests.

Depuis `tests/UnitTests`, lance ensuite :

```bash
dotnet stryker
```

Le manifeste racine fixe Stryker à 5.0.0. La configuration par défaut utilise MTP et cible `Basket.cs`. Conserve le rapport HTML/JSON et les informations de cette exécution avant toute modification.

Si le résultat est incohérent, notamment si un mutant manifestement détectable survit, utilise :

```bash
dotnet stryker --config-file stryker-config.diagnostic.json --log-to-file
```

Ce mode exécute toute la suite pour chaque mutant, sans optimisation par couverture. Ne le relance pas sans fin. Pour élargir au domaine de la feature, et seulement si cela apporte un cas utile à la présentation, tu peux utiliser :

```bash
dotnet stryker --config-file stryker-config.feature.json
```

Ne compare pas les scores de deux périmètres ou niveaux de mutation différents comme une amélioration de la qualité des tests. N’utilise pas `--test-case-filter` avec le runner xUnit v3 sous MTP. La campagne unitaire ne demande pas de lancer les tests de bout en bout ni de télécharger les navigateurs Playwright.

Si .NET ou une dépendance manque, explique le blocage et conserve le statut « non exécuté » pour les mesures concernées. N’installe pas silencieusement des composants système. Annonce un budget de préparation raisonnable avant de lancer les campagnes et arrête les répétitions qui n’apportent pas d’information nouvelle.

## 3. Choisir une preuve lisible

Dans le rapport, examine d’abord la mutation `<` vers `<=` sur le minimum de 50 € dans `Basket.DiscountAmount()`.

Le test existant `KeepsPromoCodeWithoutDiscountBelowMinimumAndRecalculatesAfterQuantityChange` atteint exactement 50 € et exige 5 € de remise. La détection de cette mutation est une attente déduite du code : confirme son statut et le test détecteur dans le rapport.

Choisis ensuite, si le rapport en contient, un seul survivant représentatif de la feature. Pour ce cas :

1. Cite le fichier, l’emplacement, le remplacement et le statut réellement observés.
2. Décris un changement observable pour une entrée valide.
3. Relie le résultat attendu à une règle métier ou explique quelle règle manque.
4. Propose un test discriminant, une correction confirmée, une question métier ou une justification d’équivalence.

Une exception incidente qui tue un mutant mérite une lecture différente d’une assertion qui observe la règle attendue. Un survivant ne démontre pas à lui seul un défaut de production.

Si une expérience avant/après est utile et repose sur une exigence claire, ajoute un seul test pertinent dans une modification isolée. Conserve la mesure avant, exécute les tests après, puis relance la même campagne. Ne modifie pas le code métier ou les exclusions pour obtenir un meilleur score. Si le test serait redondant ou si l’attendu est ambigu, expose le cas sans fabriquer une amélioration.

## 4. Enrichir le support HTML

Conserve le parcours suivant, en neuf séquences maximum :

1. Un encadré qui présente le sujet de la PR : besoin, règles, exemple et travail confié à l’agent.
2. La PR réelle ou le diff réel de la branche, avec deux points d’entrée précis dans les fichiers.
3. Les tests verts et les questions qu’ils permettent de trancher.
4. Une mutation simple et un exemple discriminant à 50 €.
5. Le test qui détecte ce changement.
6. Le rapport réel, son périmètre et ses limites.
7. Le rôle de l’agent dans l’analyse des survivants avant la PR.
8. Les preuves et décisions transmises au relecteur.
9. Une discussion et les commandes pour reproduire.

Le fichier `présentation/index.html` doit rester autonome : HTML/CSS/JavaScript dans un seul fichier, sans CDN, sans compilation et ouvrable par double-clic. Conserve la navigation au clavier, les notes d’orateur et l’impression.

Le laboratoire JavaScript est une illustration pédagogique. Ne le présente jamais comme une exécution .NET ou Stryker.

Le support permet déjà de charger un rapport JSON par un sélecteur local. Tu peux aussi renseigner le bloc `script#embedded-report` avec le rapport complet pour le précharger. Pour une insertion sûre, sérialise le JSON en remplaçant tous les caractères `<` par leur échappement JSON `\u003c`. Ne colle pas un fragment de rapport en laissant le support recalculer un score global sur ce sous-ensemble. Si le fichier est trop volumineux ou ne doit pas être embarqué, conserve le sélecteur local.

Si une PR existe, renseigne `prUrl` dans `script#session-config` et adapte les notes qui annonçaient son absence. Actualise les liens et le commit des extraits si les sources ont changé. Affiche les preuves réellement inspectées et conserve les marqueurs « illustration », « résultat communiqué », « attente à confirmer » ou « résultat mesuré » selon le cas.

Présente les informations d’environnement détaillées dans les notes ou dans le journal de séance, avec seulement les éléments nécessaires à la compréhension sur les écrans projetés. L’objectif pédagogique reste la décision de revue, avec un cas lisible plutôt qu’une accumulation de métriques.

## 5. Garder les preuves et vérifier la séance

Crée ou mets à jour `présentation/resultats/session.md` avec :

- Le SHA, l’état des changements locaux et la date de l’exécution.
- Les versions de SDK, Stryker et runner.
- Les commandes, leurs résultats et leurs codes de sortie.
- Le périmètre, la configuration et les chemins des rapports conservés.
- Les compteurs, le calcul du score et les timeouts/erreurs/exclusions.
- La mutation de borne examinée et le cas réel choisi pour la séance.
- Les éventuels changements avant/après, les décisions métier attendues et les limites.

Les rapports complets restent dans `StrykerOutput`, ignoré par Git. Le journal contient une synthèse inspectable et des références exactes. Les résultats d’expériences ne doivent pas effacer ceux de l’état de départ.

Vérifie le support dans un navigateur si ton environnement le permet : navigation, laboratoire à 25 €, 50 €, 50,05 € et 200 €, import du vrai JSON, lecture d’un survivant, notes et impression. Vérifie aussi les liens locaux et les extraits de code. Ne prétends pas avoir vérifié ce qui n’a pas pu être exécuté.

Termine en indiquant les fichiers modifiés, les contrôles exécutés, la preuve centrale de la présentation et ce qui reste à décider. Donne une courte procédure pour ouvrir le support et rejouer la démonstration.
