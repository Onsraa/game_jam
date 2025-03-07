// Créez également un script simple pour changer la couleur en fonction du volume
// ColorResponder.cs
using UnityEngine;

public class ColorResponder : MonoBehaviour
{
    [SerializeField] private MicrophoneAnalyzer audioAnalyzer;
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Color lowVolumeColor = Color.blue;
    [SerializeField] private Color highVolumeColor = Color.red;
    [SerializeField] private float colorThreshold = 0.1f; // Seuil pour atteindre la couleur max
    
    void Start()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<Renderer>();
    }
    
    void Update()
    {
        if (audioAnalyzer != null && targetRenderer != null)
        {
            // Obtenir le volume actuel via réflexion (si nécessaire)
            float currentVolume = GetCurrentVolume();
            
            // Calculer la couleur en fonction du volume
            Color newColor = Color.Lerp(lowVolumeColor, highVolumeColor, 
                Mathf.Clamp01(currentVolume / colorThreshold));
            
            // Appliquer la couleur
            targetRenderer.material.color = newColor;
        }
    }
    
    // Cette méthode récupère le volume du MicrophoneAnalyzer
    // Si vous avez mis currentVolume en public, vous pouvez l'accéder directement
    private float GetCurrentVolume()
    {
        // Option 1: Si vous rendez currentVolume public dans MicrophoneAnalyzer
        // return audioAnalyzer.currentVolume;
        
        // Option 2: Utiliser une propriété publique exposée dans MicrophoneAnalyzer
        // return audioAnalyzer.Volume;
        
        // Option temporaire: utiliser la taille de l'objet comme approximation du volume
        return (audioAnalyzer.transform.localScale.x - 0.2f) / 2.0f;
    }
}