using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class IKStatusUI : MonoBehaviour
{
    private TextMeshProUGUI statusText;
    private Actor Actor;

    private bool? lastStatus = null;

    void Start()
    {
        GameObject canvasGO = new GameObject("IKStatusCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // Create Text Object
        GameObject textGO = new GameObject("IKStatusText");
        textGO.transform.SetParent(canvasGO.transform);

        statusText = textGO.AddComponent<TextMeshProUGUI>();
        statusText.fontSize = 55;
        statusText.alignment = TextAlignmentOptions.TopLeft;
        statusText.rectTransform.anchorMin = new Vector2(0, 1);
        statusText.rectTransform.anchorMax = new Vector2(0, 1);
        statusText.rectTransform.pivot = new Vector2(0, 1);
        statusText.rectTransform.anchoredPosition = new Vector2(1500, -100);

        statusText.text = "";

        Actor = GetComponent<Actor>();
    }

    void Update()
    {
        if (!Actor.isIKClip) return;
        if (Actor.ApplyIk != lastStatus)
        {
            statusText.text = Actor.ApplyIk ? "IK: ON" : "IK: OFF";
            statusText.color = Actor.ApplyIk ? new Color(0, 0.3f, 0) : new Color(0.5f, 0, 0);
        }
    }
}
