// OUTIL D'ÉDITEUR (pas du code de jeu) : installe les personnages de la H3 en un clic.
// Menu : Outils > Installer les personnages (H3). Peut être relancé. À supprimer après usage.
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class CharacterSetup
{
    const string AnimFolder = "Assets/Animations";
    const string SantaAnimFolder = "Assets/Models/ithappy/Christmas_Pack_FREE/Animations/Other_Animations/";
    const string SantaPrefab = "Assets/Models/ithappy/Christmas_Pack_FREE/Prefabs/Santa.prefab";
    const string SantaFbx = "Assets/Models/ithappy/Christmas_Pack_FREE/Meshes/Santa.fbx";
    const string PeopleFolder = "Assets/Models/polyperfect/Poly Halloween/Prefabs/People/";

    [MenuItem("Outils/Installer les personnages (H3)")]
    static void Install()
    {
        if (!EditorUtility.DisplayDialog("Installer les personnages",
            "Santa devient le joueur (vue TPS) et les monstres reçoivent leurs modèles et leurs animations.\n" +
            "Une sauvegarde Git a été faite juste avant.", "Installer", "Annuler"))
        {
            return;
        }

        // 1) Animations (en boucle) et Animator Controllers
        AnimationClip idle = LoopClip("Santa_IdleLookAround");
        AnimationClip run = LoopClip("Santa_Run");
        AnimationClip walk = LoopClip("Santa_Walk");
        if (idle == null || run == null || walk == null) { Fail("Animations de Santa introuvables."); return; }
        Directory.CreateDirectory(AnimFolder);
        AnimatorController playerCtrl = PlayerController(idle, run);
        AnimatorController hunterCtrl = SingleState("HunterAnimator", "Walk", walk);
        AnimatorController thiefCtrl = SingleState("ThiefAnimator", "Run", run);

        // 2) Monstres (prefabs) : 2 chasseurs, 2 voleurs
        Copy("Assets/Prefabs/Creature.prefab", "Assets/Prefabs/Creature_Witch.prefab");
        Copy("Assets/Prefabs/Thief.prefab", "Assets/Prefabs/Thief_Ghost.prefab");
        DressMonster("Assets/Prefabs/Creature.prefab", "Man_Killer_Halloween", hunterCtrl);
        DressMonster("Assets/Prefabs/Creature_Witch.prefab", "Woman_Witch_Halloween", hunterCtrl);
        DressMonster("Assets/Prefabs/Thief.prefab", "Boy_Ghost_Halloween", thiefCtrl);
        DressMonster("Assets/Prefabs/Thief_Ghost.prefab", "Man_Ghost_Halloween", thiefCtrl);
        FillSpawner();

        // 3) Le joueur : Santa en vue TPS
        int playerLayer = EnsureLayer("Player");
        if (playerLayer < 0) { Fail("Plus de couche libre dans Tags and Layers."); return; }
        SetupPlayer(playerCtrl, playerLayer);

        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorUtility.DisplayDialog("Personnages installés", "Fais Ctrl+S, puis teste en Play.", "OK");
    }

    // ------------------------------------------------------------------ Joueur

    static void SetupPlayer(AnimatorController ctrl, int layer)
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) { Fail("Aucun objet avec le Tag Player."); return; }
        Camera cam = player.GetComponentInChildren<Camera>();
        Undo.SetCurrentGroupName("Santa");
        int group = Undo.GetCurrentGroup();

        Transform santa = player.transform.Find("Santa");
        if (santa == null)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(SantaPrefab);
            GameObject s = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            Undo.RegisterCreatedObjectUndo(s, "Santa");
            s.name = "Santa";
            s.transform.SetParent(player.transform, false);
            s.transform.localPosition = Vector3.zero;
            s.transform.localRotation = Quaternion.identity;
            foreach (Collider c in s.GetComponentsInChildren<Collider>()) Object.DestroyImmediate(c);
            ScaleToHeight(s, 1.9f);
            santa = s.transform;
        }

        Animator anim = santa.GetComponentInChildren<Animator>();
        if (anim == null) anim = santa.gameObject.AddComponent<Animator>();
        if (anim.avatar == null) anim.avatar = LoadAvatar(SantaFbx);
        anim.runtimeAnimatorController = ctrl;
        anim.applyRootMotion = false; // c'est le CharacterController qui déplace Santa, pas l'animation
        anim.Rebind();

        // L'ancien visuel (capsule + visière) est caché
        foreach (string n in new[] { "Capsule", "Cube" })
        {
            Transform t = player.transform.Find(n);
            if (t == null && cam != null) t = cam.transform.Find(n);
            if (t != null && t.GetComponent<Renderer>() != null)
            {
                Undo.RecordObject(t.GetComponent<Renderer>(), "Visuel");
                t.GetComponent<Renderer>().enabled = false;
            }
        }

        // Caméra au-dessus de l'épaule droite
        if (cam != null)
        {
            Undo.RecordObject(cam.transform, "Camera");
            cam.transform.localPosition = new Vector3(0.7f, 2.1f, -3.2f);
            cam.transform.localRotation = Quaternion.identity;

            // L'arme passe dans la main droite de Santa
            Transform weapon = cam.transform.Find("Weapon");
            if (weapon != null)
            {
                Transform hand = anim.isHuman ? anim.GetBoneTransform(HumanBodyBones.RightHand) : null;
                Undo.SetTransformParent(weapon, hand != null ? hand : santa, "Arme");
                weapon.position = hand != null ? hand.position : santa.position + santa.rotation * new Vector3(0.35f, 1.1f, 0.4f);
                weapon.rotation = santa.rotation; // le sucre d'orge pointe vers l'avant
            }

            // Le cadeau porté passe devant Santa
            Transform carried = cam.transform.Find("CarriedGift");
            if (carried != null)
            {
                Undo.SetTransformParent(carried, santa, "Cadeau porté");
                carried.position = santa.position + santa.rotation * new Vector3(0, 1.1f, 0.55f);
            }
        }

        // Toute la hiérarchie du joueur sur la couche Player
        foreach (Transform t in player.GetComponentsInChildren<Transform>(true))
        {
            Undo.RecordObject(t.gameObject, "Couche");
            t.gameObject.layer = layer;
        }

        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc != null)
        {
            SerializedObject so = new SerializedObject(pc);
            so.FindProperty("animator").objectReferenceValue = anim;
            so.FindProperty("shootMask").intValue = ~(1 << layer); // tout sauf la couche Player
            so.ApplyModifiedProperties();
        }
        Undo.CollapseUndoOperations(group);
    }

    // ------------------------------------------------------------------ Monstres

    static void DressMonster(string prefabPath, string modelName, AnimatorController ctrl)
    {
        GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(PeopleFolder + modelName + ".prefab");
        if (model == null) { Debug.LogWarning("Modèle introuvable : " + modelName); return; }

        GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
        Transform old = root.transform.Find("Model");
        if (old != null) Object.DestroyImmediate(old.gameObject);

        MeshRenderer capsule = root.GetComponent<MeshRenderer>();
        if (capsule != null) capsule.enabled = false;

        GameObject m = (GameObject)PrefabUtility.InstantiatePrefab(model, root.transform);
        m.name = "Model";
        m.transform.localPosition = new Vector3(0, -1, 0); // le centre de la créature est à 1 m du sol
        m.transform.localRotation = Quaternion.identity;
        foreach (Collider c in m.GetComponentsInChildren<Collider>()) Object.DestroyImmediate(c);
        ScaleToHeight(m, 1.9f);
        Bounds b = GetBounds(m);
        m.transform.position += new Vector3(0, (root.transform.position.y - 1) - b.min.y, 0);

        Animator anim = m.GetComponentInChildren<Animator>();
        if (anim == null) anim = m.AddComponent<Animator>();
        anim.runtimeAnimatorController = ctrl;
        anim.applyRootMotion = false; // c'est le NavMeshAgent qui déplace le monstre

        // Le chasseur devient cyan quand il est ralenti : on lui donne le Renderer du modèle
        Creature creature = root.GetComponent<Creature>();
        if (creature != null)
        {
            SerializedObject so = new SerializedObject(creature);
            so.FindProperty("rend").objectReferenceValue = m.GetComponentInChildren<Renderer>();
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // Le voleur porte le cadeau devant lui
        Thief thief = root.GetComponent<Thief>();
        if (thief != null)
        {
            SerializedObject so = new SerializedObject(thief);
            Object gift = so.FindProperty("carriedGiftVisual").objectReferenceValue;
            if (gift is GameObject g) g.transform.localPosition = new Vector3(0, 0.2f, 0.6f);
        }

        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        PrefabUtility.UnloadPrefabContents(root);
    }

    static void FillSpawner()
    {
        Spawner spawner = Object.FindAnyObjectByType<Spawner>();
        if (spawner == null) return;
        SerializedObject so = new SerializedObject(spawner);
        SetArray(so.FindProperty("creaturePrefabs"), "Assets/Prefabs/Creature.prefab", "Assets/Prefabs/Creature_Witch.prefab");
        SetArray(so.FindProperty("thiefPrefabs"), "Assets/Prefabs/Thief.prefab", "Assets/Prefabs/Thief_Ghost.prefab");
        so.ApplyModifiedProperties();
    }

    static void SetArray(SerializedProperty array, params string[] paths)
    {
        array.arraySize = paths.Length;
        for (int i = 0; i < paths.Length; i++)
            array.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(paths[i]);
    }

    static void Copy(string from, string to)
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(to) == null) AssetDatabase.CopyAsset(from, to);
    }

    // ------------------------------------------------------------------ Animations

    static AnimationClip LoopClip(string name)
    {
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(SantaAnimFolder + name + ".anim");
        if (clip == null) return null;
        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        if (!settings.loopTime)
        {
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            EditorUtility.SetDirty(clip);
        }
        return clip;
    }

    static AnimatorController PlayerController(AnimationClip idle, AnimationClip run)
    {
        string path = AnimFolder + "/PlayerAnimator.controller";
        AssetDatabase.DeleteAsset(path);
        AnimatorController ctrl = AnimatorController.CreateAnimatorControllerAtPath(path);
        ctrl.AddParameter("Speed", AnimatorControllerParameterType.Float);
        AnimatorStateMachine sm = ctrl.layers[0].stateMachine;
        AnimatorState sIdle = sm.AddState("Idle");
        sIdle.motion = idle;
        AnimatorState sRun = sm.AddState("Run");
        sRun.motion = run;
        sm.defaultState = sIdle;

        AnimatorStateTransition go = sIdle.AddTransition(sRun);
        go.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
        go.hasExitTime = false;
        go.duration = 0.15f;
        AnimatorStateTransition stop = sRun.AddTransition(sIdle);
        stop.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
        stop.hasExitTime = false;
        stop.duration = 0.15f;
        return ctrl;
    }

    static AnimatorController SingleState(string name, string stateName, AnimationClip clip)
    {
        string path = AnimFolder + "/" + name + ".controller";
        AssetDatabase.DeleteAsset(path);
        AnimatorController ctrl = AnimatorController.CreateAnimatorControllerAtPath(path);
        AnimatorState s = ctrl.layers[0].stateMachine.AddState(stateName);
        s.motion = clip;
        ctrl.layers[0].stateMachine.defaultState = s;
        return ctrl;
    }

    // ------------------------------------------------------------------ Utilitaires

    static int EnsureLayer(string name)
    {
        SerializedObject tm = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layers = tm.FindProperty("layers");
        for (int i = 8; i < layers.arraySize; i++)
        {
            if (layers.GetArrayElementAtIndex(i).stringValue == name) return i;
        }
        for (int i = 8; i < layers.arraySize; i++)
        {
            if (string.IsNullOrEmpty(layers.GetArrayElementAtIndex(i).stringValue))
            {
                layers.GetArrayElementAtIndex(i).stringValue = name;
                tm.ApplyModifiedProperties();
                return i;
            }
        }
        return -1;
    }

    static Avatar LoadAvatar(string fbxPath)
    {
        foreach (Object o in AssetDatabase.LoadAllAssetsAtPath(fbxPath))
        {
            if (o is Avatar a) return a;
        }
        return null;
    }

    static void ScaleToHeight(GameObject go, float height)
    {
        Bounds b = GetBounds(go);
        if (b.size.y > 0.01f) go.transform.localScale *= height / b.size.y;
    }

    static Bounds GetBounds(GameObject go)
    {
        Renderer[] rs = go.GetComponentsInChildren<Renderer>();
        if (rs.Length == 0) return new Bounds(go.transform.position, Vector3.zero);
        Bounds b = rs[0].bounds;
        foreach (Renderer r in rs) b.Encapsulate(r.bounds);
        return b;
    }

    static void Fail(string message)
    {
        EditorUtility.DisplayDialog("Installer les personnages", message, "OK");
    }
}
