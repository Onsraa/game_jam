using UnityEngine;
using TMPro;

public class SubtitleDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI subtitleText;
    [SerializeField] private float fadeSpeed = 3.0f;
    [SerializeField] private CanvasGroup canvasGroup;
    
    private bool hasSubtitle = false;
    
    void Start()
    {
        // Assurez-vous que les références sont correctement définies
        if (subtitleText == null)
        {
            subtitleText = GetComponentInChildren<TextMeshProUGUI>();
            if (subtitleText == null)
            {
                // Créer le système de sous-titres s'il n'existe pas
                GameObject canvasObj = new GameObject("SubtitleCanvas");
                canvasObj.transform.SetParent(transform);
                
                Canvas canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.WorldSpace;
                
                // Positionner sous la bulle
                RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
                canvasRect.localPosition = new Vector3(0, -0.7f, 0);
                canvasRect.sizeDelta = new Vector2(3, 0.5f);
                canvasRect.localScale = Vector3.one * 0.2f;
                
                // Ajouter CanvasGroup pour le fade
                canvasGroup = canvasObj.AddComponent<CanvasGroup>();
                canvasGroup.alpha = 0;
                
                // Ajouter un fond pour les sous-titres
                GameObject bgObj = new GameObject("Background");
                bgObj.transform.SetParent(canvasRect, false);
                RectTransform bgRect = bgObj.AddComponent<RectTransform>();
                bgRect.anchorMin = Vector2.zero;
                bgRect.anchorMax = Vector2.one;
                bgRect.offsetMin = Vector2.zero;
                bgRect.offsetMax = Vector2.zero;
                
                // Fond semi-transparent
                var bgImage = bgObj.AddComponent<UnityEngine.UI.Image>();
                bgImage.color = new Color(0, 0, 0, 0.5f);
                
                // Créer le texte
                GameObject textObj = new GameObject("SubtitleText");
                textObj.transform.SetParent(canvasRect, false);
                
                subtitleText = textObj.AddComponent<TextMeshProUGUI>();
                subtitleText.alignment = TextAlignmentOptions.Center;
                subtitleText.fontSize = 24;
                subtitleText.color = Color.white;
                
                RectTransform textRect = subtitleText.GetComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = new Vector2(10, 5);
                textRect.offsetMax = new Vector2(-10, -5);
            }
        }
        
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = subtitleText.GetComponentInParent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }
            canvasGroup.alpha = 0;
        }
    }
    
    void Update()
    {
        // Fade in/out des sous-titres
        if (canvasGroup != null)
        {
            float targetAlpha = hasSubtitle ? 1 : 0;
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);
        }
    }
    
    public void DisplaySubtitle(string text)
    {
        if (subtitleText != null)
        {
            if (string.IsNullOrEmpty(text))
            {
                hasSubtitle = false;
            }
            else
            {
                subtitleText.text = text;
                hasSubtitle = true;
            }
        }
    }
}