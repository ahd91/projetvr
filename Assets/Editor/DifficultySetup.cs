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

        Undo.CollapseUndoOperations(group);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorUtility.DisplayDialog("Difficulté installée",
            "Krampus créé (Assets/Prefabs/Krampus) et branché sur le Spawner.\n" +
            pumpkins + " citrouilles servent maintenant de munitions.\nFais Ctrl+S, puis teste.", "OK");
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
