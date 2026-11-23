// OUTIL D'ÉDITEUR (pas du code de jeu) : installe le Krampus et les citrouilles-munitions.
// Menu : Outils > Installer la difficulté. Peut être relancé. À supprimer après usage.
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public static class DifficultySetup
{
    const string HunterPath = "Assets/Prefabs/Creature.prefab";
    const string KrampusPath = "Assets/Prefabs/Krampus.prefab";

    [MenuItem("Outils/Installer la difficulté")]
    static void Install()
    {
        GameObject krampus = BuildKrampus();

        Undo.SetCurrentGroupName("Difficulté");
        int group = Undo.GetCurrentGroup();

        // Les citrouilles de nuit deviennent des munitions
        int pumpkins = 0;
        GameObject citrouilles = GameObject.Find("Citrouilles");
        if (citrouilles != null)
        {
            foreach (Transform p in citrouilles.transform)
            {
                if (p.GetComponent<Collider>() == null) continue; // il faut un collider pour que le raycast la touche
                if (p.GetComponent<PumpkinAmmo>() == null) Undo.AddComponent<PumpkinAmmo>(p.gameObject);
                pumpkins++;
            }
        }

        // Branchements
        DayNightManager dnm = Object.FindAnyObjectByType<DayNightManager>();
        PlayerController pc = Object.FindAnyObjectByType<PlayerController>();
        if (dnm != null && pc != null)
        {
            SerializedObject so = new SerializedObject(dnm);
            so.FindProperty("player").objectReferenceValue = pc;
            so.ApplyModifiedProperties();
        }
        Spawner spawner = Object.FindAnyObjectByType<Spawner>();
        if (spawner != null)
        {
            SerializedObject so = new SerializedObject(spawner);
            so.FindProperty("bossPrefab").objectReferenceValue = krampus;
            so.ApplyModifiedProperties();
        }

        // Plus de son dans le jeu : on retire les sources audio
        foreach (string n in new[] { "DayMusic", "NightMusic" })
        {
            GameObject music = GameObject.Find(n);
            if (music != null) Undo.DestroyObjectImmediate(music);
        }
        WeaponEffects weapon = Object.FindAnyObjectByType<WeaponEffects>();
        if (weapon != null && weapon.GetComponent<AudioSource>() != null)
        {
            Undo.DestroyObjectImmediate(weapon.GetComponent<AudioSource>());
        }

        Undo.CollapseUndoOperations(group);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorUtility.DisplayDialog("Difficulté installée",
            "Krampus créé (Assets/Prefabs/Krampus) et branché sur le Spawner.\n" +
            pumpkins + " citrouilles servent maintenant de munitions.\nFais Ctrl+S, puis teste.", "OK");
    }

    // ------------------------------------------------------------------ Monstres plus durs

    [MenuItem("Outils/Monstres plus durs")]
    static void HarderMonsters()
    {
        // 1) Vitesses (le joueur court à 5)
        SetAgent("Assets/Prefabs/Creature.prefab", 3.5f);
        SetAgent("Assets/Prefabs/Creature_Witch.prefab", 3.8f);
        SetAgent("Assets/Prefabs/Thief.prefab", 4.5f);
        SetAgent("Assets/Prefabs/Thief_Ghost.prefab", 4.8f);
        SetAgent("Assets/Prefabs/Krampus.prefab", 2.2f);

        Undo.SetCurrentGroupName("Monstres plus durs");
        int group = Undo.GetCurrentGroup();

        // 2) 12 points d'apparition tout autour du village (posés sur la NavMesh)
        GameObject parent = GameObject.Find("Spawnpoints");
        if (parent == null) { EditorUtility.DisplayDialog("Monstres plus durs", "Objet « Spawnpoints » introuvable.", "OK"); return; }
        Vector3[] spots =
        {
            new Vector3(-12, 0, 25), new Vector3(0, 0, 26), new Vector3(12, 0, 25),     // cimetière
            new Vector3(-26, 0, 12), new Vector3(-26, 0, 0), new Vector3(-26, 0, -14),  // forêt ouest
            new Vector3(26, 0, 12), new Vector3(26, 0, 0), new Vector3(26, 0, -14),     // forêt est
            new Vector3(-16, 0, -26), new Vector3(0, 0, -26), new Vector3(16, 0, -26)   // sud
        };
        for (int i = parent.transform.childCount - 1; i >= 0; i--)
        {
            Undo.DestroyObjectImmediate(parent.transform.GetChild(i).gameObject);
        }
        Transform[] points = new Transform[spots.Length];
        for (int i = 0; i < spots.Length; i++)
        {
            Vector3 pos = spots[i];
            NavMeshHit hit;
            if (NavMesh.SamplePosition(pos, out hit, 5f, NavMesh.AllAreas)) pos = hit.position;
            GameObject sp = new GameObject("SP" + (i + 1));
            Undo.RegisterCreatedObjectUndo(sp, "Point");
            sp.transform.SetParent(parent.transform, false);
            sp.transform.position = pos + Vector3.up; // centre de la créature à 1 m du sol
            points[i] = sp.transform;
        }

        // 3) Plus de monstres, plus souvent
        Spawner spawner = Object.FindAnyObjectByType<Spawner>();
        if (spawner != null)
        {
            SerializedObject so = new SerializedObject(spawner);
            SerializedProperty arr = so.FindProperty("spawnPoints");
            arr.arraySize = points.Length;
            for (int i = 0; i < points.Length; i++) arr.GetArrayElementAtIndex(i).objectReferenceValue = points[i];
            so.FindProperty("minDistance").floatValue = 12f;
            so.FindProperty("baseMaxCreatures").intValue = 6;
            so.FindProperty("baseInterval").floatValue = 2.5f;
            so.FindProperty("intervalPerGift").floatValue = 0.15f;
            so.FindProperty("minInterval").floatValue = 0.8f;
            so.ApplyModifiedProperties();
        }

        Undo.CollapseUndoOperations(group);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorUtility.DisplayDialog("Monstres plus durs",
            "Vitesses augmentées, 12 points d'apparition autour du village, et plus de monstres.\nFais Ctrl+S, puis teste.", "OK");
    }

    static void SetAgent(string path, float speed)
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null) return;
        GameObject root = PrefabUtility.LoadPrefabContents(path);
        NavMeshAgent agent = root.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.speed = speed;
            agent.acceleration = 14f;   // démarre et tourne plus vite : plus difficile à semer
            agent.angularSpeed = 400f;
        }
        PrefabUtility.SaveAsPrefabAsset(root, path);
        PrefabUtility.UnloadPrefabContents(root);
    }

    // Le Krampus : un chasseur deux fois plus grand, plus lent, plus résistant et qui frappe plus fort
    static GameObject BuildKrampus()
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(KrampusPath) == null)
        {
            AssetDatabase.CopyAsset(HunterPath, KrampusPath);
        }

        GameObject root = PrefabUtility.LoadPrefabContents(KrampusPath);
        root.transform.localScale = Vector3.one;

        Transform model = root.transform.Find("Model");
        if (model != null)
        {
            // Taille x2 (calculée à partir du chasseur d'origine pour pouvoir relancer l'outil)
            GameObject hunter = AssetDatabase.LoadAssetAtPath<GameObject>(HunterPath);
            Transform hunterModel = hunter.transform.Find("Model");
            if (hunterModel != null)
            {
                model.localScale = hunterModel.localScale * 2f;
                model.localPosition = hunterModel.localPosition;
            }
            // On repose les pieds au sol (le centre de la créature est à 1 m du sol)
            Bounds b = GetBounds(model.gameObject);
            model.position += new Vector3(0, (root.transform.position.y - 1f) - b.min.y, 0);

            // Teinte rouge sombre pour le distinguer
            Renderer r = model.GetComponentInChildren<Renderer>();
            if (r != null && r.sharedMaterial != null)
            {
                string matPath = "Assets/Material/M_Krampus.mat";
                Material m = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                if (m == null)
                {
                    m = new Material(r.sharedMaterial);
                    m.SetColor("_BaseColor", new Color(1f, 0.45f, 0.45f));
                    AssetDatabase.CreateAsset(m, matPath);
                }
                foreach (Renderer rr in model.GetComponentsInChildren<Renderer>()) rr.sharedMaterial = m;
            }
        }

        CapsuleCollider col = root.GetComponent<CapsuleCollider>();
        if (col != null)
        {
            col.height = 4f;
            col.radius = 0.8f;
            col.center = new Vector3(0, 1f, 0);
        }

        NavMeshAgent agent = root.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.speed = 1.6f;
            agent.height = 4f;
        }

        SetInt(root.GetComponent<Health>(), "maxHp", 20);
        Creature creature = root.GetComponent<Creature>();
        SetInt(creature, "damage", 3);
        SetFloat(creature, "attackRange", 3f);

        Transform aura = root.transform.Find("Aura");
        if (aura != null)
        {
            aura.localPosition = new Vector3(0, 1.5f, 1.2f);
            Light l = aura.GetComponent<Light>();
            if (l != null)
            {
                l.color = new Color(1f, 0.1f, 0.05f);
                l.range = 8f;
                l.intensity = 5f;
            }
        }

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, KrampusPath);
        PrefabUtility.UnloadPrefabContents(root);
        return prefab;
    }

    static void SetInt(Object target, string field, int value)
    {
        if (target == null) return;
        SerializedObject so = new SerializedObject(target);
        SerializedProperty p = so.FindProperty(field);
        if (p != null) { p.intValue = value; so.ApplyModifiedPropertiesWithoutUndo(); }
    }

    static void SetFloat(Object target, string field, float value)
    {
        if (target == null) return;
        SerializedObject so = new SerializedObject(target);
        SerializedProperty p = so.FindProperty(field);
        if (p != null) { p.floatValue = value; so.ApplyModifiedPropertiesWithoutUndo(); }
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
