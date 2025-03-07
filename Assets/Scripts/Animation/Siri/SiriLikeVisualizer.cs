using UnityEngine;

public class SiriLikeVisualizer : MonoBehaviour
{
    [SerializeField] private MicrophoneAnalyzer audioAnalyzer;
    
    [Header("Size Settings")]
    [SerializeField] private float baseRadius = 0.3f;
    [SerializeField] private float volumeMultiplier = 0.3f;
    [SerializeField] private float maxRadius = 0.5f;
    [SerializeField] private float smoothingSpeed = 5f;
    
    [Header("Audio Analysis")]
    [SerializeField] private int bassFreqBand = 2;
    [SerializeField] private int midFreqBand = 10;
    [SerializeField] private int highFreqBand = 20;
    [SerializeField] private float frequencyResponseMultiplier = 3.0f;
    [SerializeField] private float audioResponseSpeed = 5.0f;
    
    // Variables privées
    private Renderer sphereRenderer;
    private float currentRadius = 0.3f;
    private float targetRadius = 0.3f;
    private float bassLevel = 0.0f;
    private float midLevel = 0.0f;
    private float highLevel = 0.0f;
    private float volumeLevel = 0.0f;
    
    // Shader property IDs
    private static readonly int BassLevelProperty = Shader.PropertyToID("_BassLevel");
    private static readonly int MidLevelProperty = Shader.PropertyToID("_MidLevel");
    private static readonly int HighLevelProperty = Shader.PropertyToID("_HighLevel");
    private static readonly int VolumeLevelProperty = Shader.PropertyToID("_VolumeLevel");
    
    void Start()
    {
        sphereRenderer = GetComponent<Renderer>();
        if (sphereRenderer == null)
        {
            Debug.LogError("Aucun Renderer trouvé sur l'objet sphère!");
            return;
        }
        
        // Définir la taille initiale
        currentRadius = baseRadius;
        transform.localScale = Vector3.one * currentRadius * 2;
    }
    
    void Update()
    {
        if (audioAnalyzer == null || sphereRenderer == null) return;
        
        // Obtenir le volume et analyser les fréquences
        float volume = audioAnalyzer.GetCurrentVolume();
        AnalyzeFrequencyBands();
        
        // Mettre à jour la taille cible avec limitation
        targetRadius = Mathf.Clamp(baseRadius + (volume * volumeMultiplier), baseRadius, maxRadius);
        
        // Animer la taille avec lissage
        currentRadius = Mathf.Lerp(currentRadius, targetRadius, Time.deltaTime * smoothingSpeed);
        transform.localScale = Vector3.one * currentRadius * 2;
        
        // Mettre à jour le niveau de volume
        volumeLevel = Mathf.Lerp(volumeLevel, volume, Time.deltaTime * audioResponseSpeed);
        
        // Appliquer les valeurs au shader - uniquement les valeurs audio qui changent
        sphereRenderer.material.SetFloat(BassLevelProperty, bassLevel);
        sphereRenderer.material.SetFloat(MidLevelProperty, midLevel);
        sphereRenderer.material.SetFloat(HighLevelProperty, highLevel);
        sphereRenderer.material.SetFloat(VolumeLevelProperty, volumeLevel);
    }
    
    void AnalyzeFrequencyBands()
    {
        if (audioAnalyzer == null) return;
        
        float[] spectrum = audioAnalyzer.SmoothedSpectrum;
        if (spectrum == null || spectrum.Length == 0) return;
        
        // S'assurer que les indices sont dans les limites du tableau
        int bassIndex = Mathf.Min(bassFreqBand, spectrum.Length - 1);
        int midIndex = Mathf.Min(midFreqBand, spectrum.Length - 1);
        int highIndex = Mathf.Min(highFreqBand, spectrum.Length - 1);
        
        // Extraire les valeurs des différentes bandes de fréquence avec lissage
        // Réponse plus rapide pour une meilleure réactivité
        float targetBass = Mathf.Clamp01(spectrum[bassIndex] * frequencyResponseMultiplier);
        float targetMid = Mathf.Clamp01(spectrum[midIndex] * frequencyResponseMultiplier);
        float targetHigh = Mathf.Clamp01(spectrum[highIndex] * frequencyResponseMultiplier);
        
        // Lissage moins important pour une réponse plus directe aux changements de son
        bassLevel = Mathf.Lerp(bassLevel, targetBass, Time.deltaTime * audioResponseSpeed);
        midLevel = Mathf.Lerp(midLevel, targetMid, Time.deltaTime * audioResponseSpeed);
        highLevel = Mathf.Lerp(highLevel, targetHigh, Time.deltaTime * audioResponseSpeed);
    }
}