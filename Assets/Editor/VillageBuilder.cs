// OUTIL D'ÉDITEUR (pas du code de jeu) : construit le village de la H1 en un clic.
// Menu : Outils > Construire le village. Tout est annulable avec Ctrl+Z.
// Une fois le village construit, ce fichier peut être supprimé : le jeu n'en a pas besoin.
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class VillageBuilder
{
    const string DemoScenePath = "Assets/Models/ithappy/Christmas_Pack_FREE/Scenes/URP_Scenes/Demonstration.unity";
    const string HousePrefabPath = "Assets/Prefabs/House.prefab";
    const string SnowMaterialPath = "Assets/Material/Snow.mat";
    const float Half = 30f; // la carte fait 60 x 60 m

    static Transform village;

    [MenuItem("Outils/Construire le village")]
    static void Build()
    {
        if (GameObject.Find("Village") != null)
        {
            EditorUtility.DisplayDialog("Village", "Un objet « Village » existe déjà dans la scène. Supprime-le d'abord (ou fais Ctrl+Z).", "OK");
            return;
        }
        if (!EditorUtility.DisplayDialog("Construire le village",
            "Cela va supprimer les cubes obstacles et la Cible, agrandir le sol, rendre les murs invisibles, " +
            "créer le village et déplacer les points d'apparition.\n\nTout est annulable avec Ctrl+Z.", "Construire", "Annuler"))
        {
            return;
        }

        Undo.SetCurrentGroupName("Construire le village");
        int undoGroup = Undo.GetCurrentGroup();
        Random.InitState(2026); // même village à chaque fois

        GameObject house = BuildHousePrefab();
        if (house == null)
        {
            return;
        }

        CleanUpOldLevel();
        PrepareGroundAndWalls();

        GameObject root = new GameObject("Village");
        Undo.RegisterCreatedObjectUndo(root, "Village");
        village = root.transform;

        BuildHouses(house);
        BuildCemetery();
        BuildForest();
        BuildSouthAndPlaza();
        MoveSpawnPoints();

        Undo.CollapseUndoOperations(undoGroup);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

        EditorUtility.DisplayDialog("Village construit",
            "Il reste à :\n1. sélectionner l'objet NavMesh et cliquer sur Bake ;\n2. faire Ctrl+S.", "OK");
    }

    // ------------------------------------------------------------------ Maison

    // Récupère la maison de la scène de démo du pack de Noël et en fait un prefab
    static GameObject BuildHousePrefab()
    {
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(HousePrefabPath);
        if (existing != null)
        {
            return existing;
        }

        Scene demo = EditorSceneManager.OpenScene(DemoScenePath, OpenSceneMode.Additive);
        Transform demoRoot = null;
        foreach (GameObject go in demo.GetRootGameObjects())
        {
            if (go.name == "Demonstration")
            {
                demoRoot = go.transform;
            }
        }
        if (demoRoot == null)
        {
            EditorSceneManager.CloseScene(demo, true);
            EditorUtility.DisplayDialog("Erreur", "Objet « Demonstration » introuvable dans la scène de démo.", "OK");
            return null;
        }

        string[] pieces = { "Wall_", "Floor_", "Roof_", "Door_", "Doorway_", "Window_" };
        GameObject houseRoot = new GameObject("House");
        foreach (Transform child in demoRoot)
        {
            bool keep = false;
            foreach (string p in pieces)
            {
                if (child.name.StartsWith(p)) keep = true;
            }
            if (!keep) continue;

            GameObject source = PrefabUtility.GetCorrespondingObjectFromSource(child.gameObject);
            GameObject copy = source != null
                ? (GameObject)PrefabUtility.InstantiatePrefab(source)
                : Object.Instantiate(child.gameObject);
            copy.name = child.name;
            copy.transform.SetParent(houseRoot.transform, false);
            copy.transform.localPosition = child.localPosition;
            copy.transform.localRotation = child.localRotation;
            copy.transform.localScale = child.localScale;
        }
        EditorSceneManager.CloseScene(demo, true);

        // Centre la maison et pose-la au sol (y = 0)
        Bounds b = GetBounds(houseRoot);
        Vector3 shift = new Vector3(-b.center.x, -b.min.y, -b.center.z);
        foreach (Transform child in houseRoot.transform)
        {
            child.localPosition += shift;
        }

        Directory.CreateDirectory("Assets/Prefabs");
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(houseRoot, HousePrefabPath);
        Object.DestroyImmediate(houseRoot);
        return prefab;
    }

    // ------------------------------------------------------------------ Ancien niveau

    static void CleanUpOldLevel()
    {
        GameObject level = GameObject.Find("Level");
        if (level != null)
        {
            // On copie la liste avant de supprimer (on ne supprime pas pendant qu'on parcourt)
            Transform[] children = new Transform[level.transform.childCount];
            for (int i = 0; i < children.Length; i++) children[i] = level.transform.GetChild(i);
            foreach (Transform t in children)
            {
                if (t.name.StartsWith("obstacle")) Undo.DestroyObjectImmediate(t.gameObject);
            }
        }
        GameObject cible = GameObject.Find("Cible");
        if (cible != null) Undo.DestroyObjectImmediate(cible);
    }

    static void PrepareGroundAndWalls()
    {
        GameObject ground = GameObject.Find("Ground");
        if (ground != null)
        {
            Undo.RecordObject(ground.transform, "Sol");
            ground.transform.localScale = new Vector3(Half * 2 / 10f, 1, Half * 2 / 10f);
            Renderer r = ground.GetComponent<Renderer>();
            Undo.RecordObject(r, "Sol");
            r.sharedMaterial = GetSnowMaterial();
        }

        SetWall("murnord", new Vector3(0, 0, Half), new Vector3(Half * 2, 10, 1));
        SetWall("mursud", new Vector3(0, 0, -Half), new Vector3(Half * 2, 10, 1));
        SetWall("murest", new Vector3(-Half, 0, 0), new Vector3(1, 10, Half * 2));
        SetWall("murouest", new Vector3(Half, 0, 0), new Vector3(1, 10, Half * 2));
    }

    static void SetWall(string name, Vector3 pos, Vector3 scale)
    {
        GameObject wall = GameObject.Find(name);
        if (wall == null) return;
        Undo.RecordObject(wall.transform, "Mur");
        wall.transform.position = pos;
        wall.transform.localScale = scale;
        MeshRenderer mr = wall.GetComponent<MeshRenderer>();
        if (mr != null)
        {
            Undo.RecordObject(mr, "Mur");
            mr.enabled = false; // limite invisible : le décor la cache
        }
    }

    static Material GetSnowMaterial()
    {
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(SnowMaterialPath);
        if (mat != null) return mat;
        mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.SetColor("_BaseColor", new Color(0.9f, 0.94f, 1f));
        mat.SetFloat("_Smoothness", 0.2f);
        AssetDatabase.CreateAsset(mat, SnowMaterialPath);
        return mat;
    }

    // ------------------------------------------------------------------ Village

    static void BuildHouses(GameObject house)
    {
        Vector3 tree = new Vector3(0, 0, 6);
        Vector3[] spots = { new Vector3(-16, 0, 12), new Vector3(16, 0, 12), new Vector3(-16, 0, -10), new Vector3(16, 0, -10) };
        Transform group = Group("Maisons");
        foreach (Vector3 s in spots)
        {
            GameObject h = Place(house, s, 0, group);
            Vector3 look = tree - s;
            look.y = 0;
            h.transform.rotation = Quaternion.LookRotation(look);
        }
    }

    static void BuildCemetery()
    {
        Transform group = Group("Cimetiere");
        float wallZ = 20f;

        // Muret avec une ouverture au milieu pour la grille
        PlaceRow("Wall_Graveyard_3m", -Half + 1, -2.5f, wallZ, group);
        PlaceRow("Wall_Graveyard_3m", 2.5f, Half - 1, wallZ, group);
        PlaceAligned(Load("Gate_Graveyard"), new Vector3(0, 0, wallZ), group);

        // Tombes au hasard dans le cimetière, en laissant libres les points d'apparition
        string[] graves = { "Gravestone_Old_Classic_Cross", "Gravestone_Old_Cross", "Gravestone_Old_Small_A",
                            "Gravestone_Old_Small_B", "Gravestone_Old_Small_C", "Gravestone_Old_Classic_Broken", "Gravestone_Old_Tower" };
        for (int i = 0; i < 22; i++)
        {
            Vector3 p = new Vector3(Random.Range(-26f, 26f), 0, Random.Range(22.5f, 28f));
            if (Mathf.Abs(p.x) < 4 || Mathf.Abs(Mathf.Abs(p.x) - 12) < 3) continue; // couloirs vers la grille et les spawns
            Place(Load(graves[Random.Range(0, graves.Length)]), p, Random.Range(-20f, 20f), group);
        }

        // Citrouilles allumées de part et d'autre de la grille, arbres morts aux coins
        Place(Load("Pumpkin_Carved_Lit_A"), new Vector3(-2.5f, 0, wallZ - 1.2f), 180, group);
        Place(Load("Pumpkin_Carved_Lit_B"), new Vector3(2.5f, 0, wallZ - 1.2f), 180, group);
        Place(Load("Tree_Scary_Dead"), new Vector3(-25, 0, 26), 30, group);
        Place(Load("Tree_Scary_Dead"), new Vector3(25, 0, 25), -40, group);
        Place(Load("Lantern_Street_Fixed_Halloween"), new Vector3(-5, 0, wallZ - 1.5f), 0, group);
        Place(Load("Lantern_Street_Fixed_Halloween"), new Vector3(5, 0, wallZ - 1.5f), 0, group);
    }

    static void BuildForest()
    {
        Transform group = Group("Foret");
        string[] trees = { "Tree_Scary_Dead", "Tree_Poplar_Dead_A" };
        for (int side = -1; side <= 1; side += 2)
        {
            for (float z = -26; z <= 18; z += 3.5f)
            {
                if (Mathf.Abs(z) < 4) continue; // chemin qui sort de la forêt (point d'apparition)
                float x = side * Random.Range(26.5f, 28.5f);
                Place(Load(trees[Random.Range(0, trees.Length)]), new Vector3(x, 0, z + Random.Range(-1f, 1f)), Random.Range(0f, 360f), group);
            }
            Place(Load("Rock_A"), new Vector3(side * 25.5f, 0, 5), Random.Range(0f, 360f), group);
            Place(Load("Rock_A"), new Vector3(side * 25.5f, 0, -5), Random.Range(0f, 360f), group);
        }
    }

    static void BuildSouthAndPlaza()
    {
        Transform south = Group("Sud");
        PlaceRow("Fence_001", -Half + 1, Half - 1, -Half + 1.5f, south);
        Place(Load("Christmass_tree_outdoor_002_toys"), new Vector3(-24, 0, -24), 0, south);
        Place(Load("Christmass_tree_outdoor_002"), new Vector3(24, 0, -24), 0, south);
        Place(Load("Christmass_tree_outdoor_001"), new Vector3(-8, 0, -25), 0, south);
        Place(Load("Christmass_tree_outdoor_002"), new Vector3(9, 0, -25), 0, south);

        Transform plaza = Group("Place");
        Place(Load("Snowman_001"), new Vector3(-6, 0, 0), 160, plaza);
        Place(Load("Snowman_001"), new Vector3(7, 0, 12), -30, plaza);
        Vector3[] lamps = { new Vector3(-7, 0, 6), new Vector3(7, 0, 6), new Vector3(0, 0, -2), new Vector3(0, 0, 14) };
        foreach (Vector3 l in lamps) Place(Load("Lantern_Street_Fixed_Halloween"), l, 0, plaza);
        string[] snows = { "Snow_001", "Snow_002", "Snow_003", "Snow_004" };
        for (int i = 0; i < 14; i++)
        {
            Vector3 p = new Vector3(Random.Range(-24f, 24f), 0, Random.Range(-24f, 17f));
            if (Vector3.Distance(p, new Vector3(0, 0, 6)) < 8) continue; // pas sur la place
            Place(Load(snows[Random.Range(0, snows.Length)]), p, Random.Range(0f, 360f), plaza);
        }
    }

    // ------------------------------------------------------------------ Points d'apparition

    static void MoveSpawnPoints()
    {
        // Monstres : 2 dans le cimetière (derrière la grille), 2 aux chemins de la forêt
        Vector3[] monsters = { new Vector3(-12, 1, 25), new Vector3(12, 1, 25), new Vector3(-26, 1, 0), new Vector3(26, 1, 0) };
        MoveChildren("Spawnpoints", monsters);

        // Cadeaux : dans les ruelles et autour des maisons, loin du sapin
        Vector3[] gifts = { new Vector3(-8, 0, 17), new Vector3(8, 0, 17), new Vector3(-22, 0, 3), new Vector3(22, 0, 3),
                            new Vector3(-20, 0, -21), new Vector3(20, 0, -21), new Vector3(0, 0, -20), new Vector3(-10, 0, -4) };
        MoveChildren("GiftSpawnPoints", gifts);

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            Undo.RecordObject(player.transform, "Joueur");
            player.transform.position = new Vector3(0, 0, -5);
            player.transform.rotation = Quaternion.identity; // regarde le sapin
        }
    }

    static void MoveChildren(string parentName, Vector3[] positions)
    {
        GameObject parent = GameObject.Find(parentName);
        if (parent == null) return;
        for (int i = 0; i < parent.transform.childCount && i < positions.Length; i++)
        {
            Transform t = parent.transform.GetChild(i);
            Undo.RecordObject(t, "Point");
            t.position = positions[i];
        }
    }

    // ------------------------------------------------------------------ Utilitaires

    static Transform Group(string name)
    {
        GameObject g = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(g, name);
        g.transform.SetParent(village, false);
        return g.transform;
    }

    static GameObject Load(string prefabName)
    {
        foreach (string guid in AssetDatabase.FindAssets(prefabName + " t:Prefab"))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (Path.GetFileNameWithoutExtension(path) == prefabName)
            {
                return AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }
        }
        Debug.LogWarning("Prefab introuvable : " + prefabName);
        return null;
    }

    // Place un prefab, posé au sol (le bas de son modèle à y = 0)
    static GameObject Place(GameObject prefab, Vector3 pos, float rotY, Transform parent)
    {
        if (prefab == null) return null;
        GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        Undo.RegisterCreatedObjectUndo(go, prefab.name);
        go.transform.SetParent(parent, false);
        go.transform.position = pos;
        go.transform.rotation = Quaternion.Euler(0, rotY, 0);
        Bounds b = GetBounds(go);
        if (b.size != Vector3.zero)
        {
            go.transform.position += new Vector3(0, pos.y - b.min.y, 0);
        }
        return go;
    }

    // Place un objet long (grille) dans l'axe X, centré sur pos
    static void PlaceAligned(GameObject prefab, Vector3 pos, Transform parent)
    {
        if (prefab == null) return;
        GameObject go = Place(prefab, pos, 0, parent);
        Bounds b = GetBounds(go);
        if (b.size.z > b.size.x)
        {
            go.transform.rotation = Quaternion.Euler(0, 90, 0);
            b = GetBounds(go);
        }
        go.transform.position += new Vector3(pos.x - b.center.x, 0, pos.z - b.center.z);
    }

    // Aligne des copies d'un prefab (muret, barrière) le long de l'axe X, de xStart à xEnd
    static void PlaceRow(string prefabName, float xStart, float xEnd, float z, Transform parent)
    {
        GameObject prefab = Load(prefabName);
        if (prefab == null) return;

        // On mesure une pièce pour connaître sa longueur et son orientation
        GameObject probe = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        Bounds b = GetBounds(probe);
        float rotY = b.size.z > b.size.x ? 90 : 0;
        Object.DestroyImmediate(probe);
        float length = Mathf.Max(b.size.x, b.size.z);
        if (length < 0.1f) return;

        for (float x = xStart + length / 2; x + length / 2 <= xEnd + 0.01f; x += length)
        {
            GameObject piece = Place(prefab, new Vector3(x, 0, z), rotY, parent);
            // On recentre la pièce (son pivot n'est pas forcément au milieu)
            Bounds pb = GetBounds(piece);
            piece.transform.position += new Vector3(x - pb.center.x, 0, z - pb.center.z);
        }
    }

    static Bounds GetBounds(GameObject go)
    {
        Renderer[] rs = go.GetComponentsInChildren<Renderer>();
        if (rs.Length == 0) return new Bounds(go.transform.position, Vector3.zero);
        Bounds b = rs[0].bounds;
        foreach (Renderer r in rs) b.Encapsulate(r.bounds);
        return b;
    }
}
