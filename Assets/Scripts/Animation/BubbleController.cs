using UnityEngine;

public class BubbleController : MonoBehaviour 
{
    public static BubbleController Instance;
    
    [SerializeField] private Material bubbleMaterial;
    [SerializeField] private float pulsationMultiplier = 1.5f;
    [SerializeField] private float smoothingFactor = 0.2f;
    
    private float currentScale = 1f;
    private float targetScale = 1f;
    
    void Awake() 
    {
        Instance = this;
    }
    
    void Update() 
    {
        // Animation fluide
        currentScale = Mathf.Lerp(currentScale, targetScale, smoothingFactor);
        transform.localScale = Vector3.one * (1f + currentScale);
    }
    
    public void UpdateAnimation(float volume, float[] samples) 
    {
        // Mise à jour de l'échelle en fonction du volume
        targetScale = volume * pulsationMultiplier;
        
        // Mise à jour du shader avec les données de fréquence
        bubbleMaterial.SetFloatArray("_AudioSamples", samples);
        bubbleMaterial.SetFloat("_Volume", volume);
    }
}