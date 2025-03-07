using UnityEngine;
using System.Collections.Generic;

public class EmotionAnalyzer : MonoBehaviour 
{
    [System.Serializable]
    public class EmotionColorMapping 
    {
        public string emotion;
        public Color color;
        public float intensity;
    }
    
    [SerializeField] private List<EmotionColorMapping> emotionMappings;
    [SerializeField] private Material bubbleMaterial;
    [SerializeField] private float colorChangeDuration = 0.5f;
    
    private Color currentColor;
    private Color targetColor;
    private float colorChangeTime;
    
    void Update() 
    {
        // Transition fluide entre les couleurs
        if (colorChangeTime < colorChangeDuration) 
        {
            colorChangeTime += Time.deltaTime;
            float t = colorChangeTime / colorChangeDuration;
            bubbleMaterial.SetColor("_EmotionColor", Color.Lerp(currentColor, targetColor, t));
        }
    }
    
    public void AnalyzeEmotion(string input) 
    {
        // Analyse simple basée sur des mots-clés
        // Dans un système réel, vous utiliseriez une IA pour l'analyse des sentiments
        
        foreach (var mapping in emotionMappings) 
        {
            if (input.ToLower().Contains(mapping.emotion.ToLower())) 
            {
                SetEmotionColor(mapping.color, mapping.intensity);
                break;
            }
        }
    }
    
    private void SetEmotionColor(Color color, float intensity) 
    {
        currentColor = bubbleMaterial.GetColor("_EmotionColor");
        targetColor = color * intensity;
        colorChangeTime = 0f;
    }
}