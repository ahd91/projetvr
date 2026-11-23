// OUTIL D'ÉDITEUR (pas du code de jeu) : crée les éléments d'interface de l'UX en un clic.
// Menu : Outils > Installer l'UX. À supprimer après usage.
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class UXSetup
{
    [MenuItem("Outils/Installer l'UX")]
    static void Install()
    {
        HUD hud = Object.FindAnyObjectByType<HUD>();
        GameObject canvasGo = GameObject.Find("Canvas");
        if (hud == null || canvasGo == null) { EditorUtility.DisplayDialog("UX", "HUD ou Canvas introuvable.", "OK"); return; }
        Transform canvas = canvasGo.transform;
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        Undo.SetCurrentGroupName("Installer l'UX");
        int group = Undo.GetCurrentGroup();

        // 1) Flash rouge plein écran (en premier dans le Canvas : il passe derrière les textes)
        GameObject flash = new GameObject("DamageFlash", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        Undo.RegisterCreatedObjectUndo(flash, "DamageFlash");
        flash.transform.SetParent(canvas, false);
        flash.transform.SetAsFirstSibling();
        RectTransform frt = flash.GetComponent<RectTransform>();
        frt.anchorMin = Vector2.zero; frt.anchorMax = Vector2.one;
        frt.offsetMin = Vector2.zero; frt.offsetMax = Vector2.zero;
        Image fimg = flash.GetComponent<Image>();
        fimg.color = new Color(0.8f, 0f, 0f, 0f); // rouge, invisible au repos
        fimg.raycastTarget = false;

        // 2) Barre de vie (Slider), en bas à gauche
        GameObject sliderGo = DefaultControls.CreateSlider(new DefaultControls.Resources());
        Undo.RegisterCreatedObjectUndo(sliderGo, "HealthBar");
        sliderGo.name = "HealthBar";
        sliderGo.transform.SetParent(canvas, false);
        RectTransform srt = sliderGo.GetComponent<RectTransform>();
        srt.anchorMin = new Vector2(0, 0); srt.anchorMax = new Vector2(0, 0); srt.pivot = new Vector2(0, 0);
        srt.anchoredPosition = new Vector2(30, 30);
        srt.sizeDelta = new Vector2(350, 28);
        Slider slider = sliderGo.GetComponent<Slider>();
        slider.interactable = false;       // le joueur ne peut pas la modifier
        slider.transition = Selectable.Transition.None;
        slider.wholeNumbers = true;
        Transform handle = sliderGo.transform.Find("Handle Slide Area");
        if (handle != null) Object.DestroyImmediate(handle.gameObject); // pas de poignée
        Transform bg = sliderGo.transform.Find("Background");
        if (bg != null) bg.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        Transform fillArea = sliderGo.transform.Find("Fill Area");
        if (fillArea != null)
        {
            RectTransform fa = fillArea.GetComponent<RectTransform>();
            fa.offsetMin = new Vector2(3, 3); fa.offsetMax = new Vector2(-3, -3);
            Transform fill = fillArea.Find("Fill");
            if (fill != null)
            {
                fill.GetComponent<Image>().color = new Color(0.85f, 0.1f, 0.1f);
                RectTransform fr = fill.GetComponent<RectTransform>();
                fr.offsetMin = Vector2.zero; fr.offsetMax = Vector2.zero;
            }
        }
        GameObject label = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        label.transform.SetParent(sliderGo.transform, false);
        RectTransform lrt = label.GetComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one; lrt.offsetMin = Vector2.zero; lrt.offsetMax = Vector2.zero;
        Text lt = label.GetComponent<Text>();
        lt.text = "VIE"; lt.font = font; lt.fontSize = 16; lt.fontStyle = FontStyle.Bold;
        lt.alignment = TextAnchor.MiddleCenter; lt.color = Color.white; lt.raycastTarget = false;

        // 3) Grand texte au centre (changement de phase)
        GameObject big = new GameObject("BigText", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text), typeof(Outline));
        Undo.RegisterCreatedObjectUndo(big, "BigText");
        big.transform.SetParent(canvas, false);
        RectTransform brt = big.GetComponent<RectTransform>();
        brt.anchorMin = new Vector2(0.5f, 0.5f); brt.anchorMax = new Vector2(0.5f, 0.5f); brt.pivot = new Vector2(0.5f, 0.5f);
        brt.anchoredPosition = new Vector2(0, 120);
        brt.sizeDelta = new Vector2(1200, 120);
        Text bt = big.GetComponent<Text>();
        bt.text = ""; bt.font = font; bt.fontSize = 54; bt.fontStyle = FontStyle.Bold;
        bt.alignment = TextAnchor.MiddleCenter; bt.color = Color.white; bt.raycastTarget = false;
        bt.horizontalOverflow = HorizontalWrapMode.Overflow;
        Outline ol = big.GetComponent<Outline>();
        ol.effectColor = new Color(0, 0, 0, 0.8f); ol.effectDistance = new Vector2(3, -3);

        // Les panneaux de fin restent devant tout le reste
        foreach (string panel in new[] { "GameOverPanel", "VictoryPanel" })
        {
            Transform p = canvas.Find(panel);
            if (p != null) p.SetAsLastSibling();
        }

        // 4) L'ancien texte « Vie : 10 » est caché
        Transform oldText = canvas.Find("HealthText");
        if (oldText != null) { Undo.RecordObject(oldText.gameObject, "HealthText"); oldText.gameObject.SetActive(false); }

        // 5) Branchements sur le HUD
        SerializedObject so = new SerializedObject(hud);
        so.FindProperty("healthBar").objectReferenceValue = slider;
        so.FindProperty("damageFlash").objectReferenceValue = fimg;
        so.FindProperty("bigText").objectReferenceValue = bt;
        so.ApplyModifiedProperties();

        Undo.CollapseUndoOperations(group);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorUtility.DisplayDialog("UX installée", "Barre de vie, flash rouge et grand texte créés et branchés.\nFais Ctrl+S, puis teste.", "OK");
    }
}
