# TP final ZZ2 – Réalité virtuelle

Un FPS sur le thème Halloween à Noël, inspiré de *L'étrange Noël de Mr Jack*. Le jeu alterne deux phases :

- **Le jour, c'est Noël.** Des cadeaux ensorcelés (les « cadeaux farceurs ») s'enfuient quand le joueur s'approche. Il faut les ralentir avec l'arme, les attraper et les déposer sous le sapin.
- **La nuit, c'est Halloween.** Il faut survivre et protéger le sapin. Des **chasseurs** poursuivent le joueur, et des **voleurs** foncent sur le sapin, prennent un cadeau et s'enfuient avec. Un voleur tué avant de s'échapper rend le cadeau.

**Victoire :** 10 cadeaux sous le sapin. **Défaite :** le joueur meurt. Plus il y a de cadeaux sous le sapin, plus les nuits sont dures. Une partie dure environ 4 à 5 cycles jour/nuit.

Chaque nuit, un **défi** est tiré au hasard (par exemple « ne perds aucun cadeau cette nuit »). S'il est réussi, un **cadeau doré** apparaît le lendemain : il vaut 2 cadeaux, mais il est plus rapide que les autres.

## Ouvrir le projet

- **Unity 6** (version `6000.6.9f1`), avec le pipeline URP.
- Il faut le package **AI Navigation** (2.0.16), installé via le Package Manager.
- Ouvre le dossier du projet avec Unity Hub, puis lance la scène `Assets/Scenes/SampleScene`.

## Contrôles

| Touche | Action |
|---|---|
| ZQSD | Se déplacer |
| Souris | Regarder |
| Clic gauche | Tirer |
| Clic droit (jour) | Ralentisseur : ralentit le cadeau visé |
| Clic droit (nuit) | Canon : une explosion de zone (4 m) qui tue en un coup, puis 5 s de recharge |
| E | Attraper le cadeau visé (s'il est ralenti et à moins de 3 m), ou le déposer au pied du sapin |
| Échap | Libérer la souris |

## Notions du sujet

| Notion | Où on la trouve |
|---|---|
| Raycast | Le tir, la capacité spéciale, le ramassage des cadeaux, et la vérification qu'aucun mur ne sépare une créature du joueur |
| Prefabs | Le joueur, les créatures (chasseurs et voleurs) et les cadeaux, instanciés par des spawners |
| Navigation | La NavMesh de l'arène, avec trois comportements : poursuivre le joueur (chasseurs), aller au sapin (voleurs) et fuir le joueur (cadeaux farceurs) |
| UI | Le viseur et le HUD (vie, phase et chrono, cadeaux, défi, recharge du canon), ainsi que les écrans Game Over et Victoire |
| Capacité spéciale | Le clic droit : ralentisseur le jour, canon la nuit |
| Défi (« recette ») | Le défi de nuit tiré au hasard, récompensé par le cadeau doré |
| Import d'assets 3D | *Christmas Pack FREE* (ithappy) et *Poly Halloween Pack* (polyperfect), depuis l'Unity Asset Store |
| Bonus | Les animations Walk et Run du Père Noël, reciblées sur les monstres humanoïdes, et des particules (fumée) |

## Scripts

| Script | Rôle |
|---|---|
| `PlayerController.cs` | Gère le déplacement et la caméra. Le même raycast sert au tir (clic gauche), au ralentisseur (clic droit le jour), au canon à explosion de zone (clic droit la nuit, avec `Physics.OverlapSphere`) et au ramassage des cadeaux (E). Fourni par le prof, puis complété. |
| `Health.cs` | Gère les points de vie. Utilisé par les créatures et par le joueur. |
| `Creature.cs` | La créature suit le joueur sur la NavMesh et lui fait des dégâts quand elle est proche, avec un temps de recharge (cooldown). Elle peut être ralentie : elle passe à 30 % de sa vitesse et devient cyan pendant 3 s. |
| `Spawner.cs` | Fait apparaître une créature toutes les X secondes, sur un point de spawn éloigné du joueur (pour éviter le spawn kill), avec un maximum de créatures en même temps. Une partie des créatures sont des voleurs (`thiefChance`), les autres des chasseurs. |
| `Thief.cs` | Le voleur va au sapin, prend un cadeau en entrant dans la zone trigger et s'enfuit vers son point d'apparition. S'il est tué ou s'il disparaît à l'aube, le cadeau revient (grâce à `OnDestroy`). |
| `DayNightManager.cs` | Alterne le jour et la nuit avec un chrono. Il change la lumière, le ciel, le brouillard, la lumière ambiante et le post-traitement de nuit, et il n'active le spawner de monstres que la nuit. |
| `ChristmasTree.cs` | La zone de dépôt au pied du sapin (un trigger). Il compte les cadeaux, affiche autant de modèles de cadeaux sous le sapin, et permet aux voleurs d'en retirer un. |
| `Gift.cs` | Le cadeau farceur : il s'enfuit à l'opposé du joueur sur la NavMesh. Il peut être ralenti, et il ne peut être attrapé que dans ce cas. |
| `GiftSpawner.cs` | Fait apparaître 4 cadeaux farceurs au début de chaque journée, sur des points tirés au hasard. |
| `HUD.cs` | Affiche la vie, le score, le défi, la phase avec le temps restant, et le nombre de cadeaux. À 10 cadeaux, il affiche l'écran de victoire. À la mort du joueur, il affiche le Game Over, met le jeu en pause et libère la souris. Le bouton Rejouer recharge la scène. |
| `ChallengeManager.cs` | Ancien système de défis (« tue X créatures »), qui sera remplacé par le défi de nuit à l'étape G. |

## Avancement

**Version 1 : les bases (terminée)**

- [x] 1. Scène de test : sol, murs, obstacles et NavMesh
- [x] 2. Tir par raycast et points de vie
- [x] 3. Prefab de créature qui suit le joueur et lui fait des dégâts au contact
- [x] 4. Spawner sur des points éloignés du joueur
- [x] 5. Capacité spéciale : ralentir (clic droit)
- [x] 6. Défi, qui augmente le score et génère un nouveau défi
- [x] 7. HUD (vie, score, défi en cours) et game over

**Version 2 : le jeu jour/nuit (en cours)**

- [x] A. Cycle jour/nuit : chrono, lumière, ciel, brouillard, monstres uniquement la nuit
- [x] B. Le sapin : zone de dépôt, compteur de cadeaux et victoire à 10
- [x] C. Les cadeaux farceurs : ils fuient le joueur ; on les ralentit, on les ramasse et on les dépose
- [x] D. L'arme double : ralentisseur le jour, canon la nuit avec un temps de recharge
- [x] E. Les voleurs : ils vont au sapin, prennent un cadeau et s'enfuient ; un voleur tué rend le cadeau
- [ ] F. La difficulté progressive selon le nombre de cadeaux
- [ ] G. Le défi de nuit et sa récompense, le cadeau doré
- [ ] H. L'habillage : modèles 3D, animations, décor de Noël et d'Halloween, particules

### Équilibrage prévu (n = nombre de cadeaux sous le sapin)

| Réglage | Formule |
|---|---|
| Créatures maximum la nuit | 3 + n |
| Intervalle d'apparition | 4 s − 0,2 × n (minimum 1,5 s) |
| Part de voleurs | 20 % + 5 % × n |
| Vitesse des cadeaux farceurs | 3 + 0,2 × n |
| Cadeaux par journée | environ 4 qui apparaissent, dont environ 3 récupérés |

## Problèmes rencontrés et solutions

- **Le tir comptait double après un deuxième lancement du jeu.** L'abonnement à l'événement `fire` de l'Input System n'était jamais désabonné. Comme le *domain reload* est désactivé quand on lance le jeu, il survivait d'une partie à l'autre. Solution : un désabonnement dans `OnDestroy()`, et `Health` ignore maintenant les coups reçus après la mort.
- **Le prefab Player refusait de s'enregistrer.** Il contenait un composant dont le script n'était pas fourni dans le package (*Missing Script*). Solution : retirer ce composant.
- **Des milliers d'erreurs s'affichaient dans la Console.** Une créature placée hors de la NavMesh fait échouer `SetDestination` à chaque frame. Solution : vérifier `agent.isOnNavMesh` au démarrage, afficher un seul avertissement et désactiver le script.
- **Le code doit rester au niveau du cours.** Tous les scripts ont été réécrits dans le style des supports de cours : `GetComponent`, `GameObject.FindWithTag`, des tableaux, des `if` simples, et les entrées branchées comme dans le code du prof (`InputActionReference`). Le joueur et les créatures sont retrouvés grâce à leur Tag (`Player` et `Creature`).
- **On pouvait tirer pendant le Game Over.** `Time.timeScale = 0` ne met pas en pause les événements de l'Input System. Solution : `FirePressed` et `SlowPressed` s'arrêtent tout de suite quand `Time.timeScale == 0`.
- **Les créatures frappaient à travers les murs.** L'attaque ne vérifiait que la distance. Solution : avant de frapper, la créature lance un raycast vers le centre du joueur et ne frappe que si le premier objet touché est bien le joueur.
- **Le défi semblait bloqué.** Le texte du défi était coupé à l'écran : « ralenties (0/3) » était caché, et les créatures tuées sans être ralenties ne comptaient pas, ce qui était voulu. Solution : élargir les textes, régler Horizontal Overflow sur Overflow, mettre le pivot à gauche (Pivot X = 0), et afficher un message dans la Console quand une créature ne compte pas.
- **Le brouillard de nuit n'était pas convaincant.** Sur un sol vide, le brouillard délave seulement l'image, le ciel n'est pas touché par le brouillard, et la lumière ambiante restait celle du jour. Solution : une couleur de brouillard proche de l'horizon de nuit, une lumière ambiante plus faible la nuit, et le volume de post-traitement du pack Halloween (il faut cocher Post Processing sur la caméra).
- **Les voleurs ne volaient rien.** Ils devaient voler à moins de 3 m du centre du sapin, mais le sapin agrandi et le trou de la NavMesh autour du tronc les arrêtaient plus loin. Solution : le vol passe par la zone trigger du sapin, et le voleur a un Rigidbody en Is Kinematic (sans Rigidbody, Unity ne déclenche pas le trigger).
- **La scène a été perdue après une extinction du PC.** Unity n'enregistre pas les scènes automatiquement. Solution : récupérer la scène depuis `Assets/_Recovery`, puis enregistrer régulièrement avec Ctrl+S.
