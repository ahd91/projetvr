# TP final ZZ2 – Réalité virtuelle

Un FPS sur le thème Halloween à Noël, inspiré de *L'étrange Noël de Mr Jack*. Le héros affronte des vagues de créatures venues gâcher Noël. Son arme a une capacité spéciale, et un défi est généré pour lui à chaque partie.

Pour l'instant, la logique passe avant les graphismes : les créatures sont des capsules et le décor est fait de cubes.

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
| Échap | Libérer la souris |

## Notions du sujet

| Notion | Où on la trouve |
|---|---|
| Raycast | Le tir part du centre de l'écran : `PlayerController.FirePressed` |
| Prefabs | `Player` et `Creature`, la créature étant instanciée par le spawner |
| Navigation | La NavMesh est calculée sur l'arène, et les créatures suivent le joueur avec un `NavMeshAgent` |
| UI | Le viseur (bientôt aussi la vie, le score et le défi) |
| Import d'assets 3D | Prévu à la fin |

## Scripts

| Script | Rôle |
|---|---|
| `PlayerController.cs` | Gère le déplacement, la caméra et le tir par raycast, qui inflige des dégâts à l'objet touché. Fourni par le prof, puis complété. |
| `Health.cs` | Gère les points de vie. Utilisé par les créatures et par le joueur. |
| `Creature.cs` | La créature suit le joueur sur la NavMesh et lui fait des dégâts au contact, en mesurant la distance avec un temps de recharge (cooldown). |
| `Spawner.cs` | Fait apparaître une créature toutes les X secondes, sur un point de spawn éloigné du joueur (pour éviter le spawn kill), avec un maximum de créatures en même temps. |

## Avancement

- [x] 1. Scène de test : sol, murs, obstacles et NavMesh
- [x] 2. Tir par raycast et points de vie
- [x] 3. Prefab de créature qui suit le joueur et lui fait des dégâts au contact
- [x] 4. Spawner sur des points éloignés du joueur
- [ ] 5. Capacité spéciale : ralentir (clic droit)
- [ ] 6. Défi, qui augmente le score et génère un nouveau défi
- [ ] 7. HUD (vie, score, défi en cours) et game over
- [ ] 8. Vrais modèles 3D

## Problèmes rencontrés et solutions

- **Le tir comptait double après un deuxième lancement du jeu.** L'abonnement à l'événement `fire` de l'Input System n'était jamais désabonné. Comme le *domain reload* est désactivé quand on lance le jeu, il survivait d'une partie à l'autre. Solution : un désabonnement dans `OnDestroy()`, et `Health` ignore maintenant les coups reçus après la mort.
- **Le prefab Player refusait de s'enregistrer.** Il contenait un composant dont le script n'était pas fourni dans le package (*Missing Script*). Solution : retirer ce composant.
- **Des milliers d'erreurs s'affichaient dans la Console.** Une créature placée hors de la NavMesh fait échouer `SetDestination` à chaque frame. Solution : vérifier `agent.isOnNavMesh` au démarrage, afficher un seul avertissement et désactiver le script.
- **La scène a été perdue après une extinction du PC.** Unity n'enregistre pas les scènes automatiquement. Solution : récupérer la scène depuis `Assets/_Recovery`, puis enregistrer régulièrement avec Ctrl+S.
