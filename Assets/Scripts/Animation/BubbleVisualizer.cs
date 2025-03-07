using UnityEngine;

public class BubbleVisualizer : MonoBehaviour
{
    [SerializeField] private MicrophoneAnalyzer audioAnalyzer;
    
    [Header("Bubble Settings")]
    [SerializeField] private float baseRadius = 0.3f;
    [SerializeField] private float volumeMultiplier = 1.5f;
    [SerializeField] private float smoothingSpeed = 5f;
    [SerializeField] private float pulseSpeed = 1f;
    [SerializeField] private float deformAmount = 0.2f;
    
    [Header("Color Settings")]
    [SerializeField] private Color baseColor = new Color(0.4f, 0.6f, 1.0f, 0.7f);
    [SerializeField] private Color activeColor = new Color(1.0f, 0.5f, 0.5f, 0.8f);
    [SerializeField] private float colorTransitionSpeed = 3f;
    
    // Variables privées
    private Renderer bubbleRenderer;
    private float currentRadius = 0.3f;
    private float targetRadius = 0.3f;
    private float pulseTime = 0f;
    private float currentColorIntensity = 0f;
    private float targetColorIntensity = 0f;
    
    // Paramètres du shader
    private static readonly int BaseColorProperty = Shader.PropertyToID("_BaseColor");
    private static readonly int ActiveColorProperty = Shader.PropertyToID("_ActiveColor");
    private static readonly int ColorIntensityProperty = Shader.PropertyToID("_ColorIntensity");
    private static readonly int DeformAmountProperty = Shader.PropertyToID("_DeformAmount");
    private static readonly int PulseTimeProperty = Shader.PropertyToID("_PulseTime");
    
    void Start() 
	{
    	bubbleRenderer = GetComponent<Renderer>();
    	
    	// Au lieu de créer un nouveau matériau, assurez-vous que les propriétés sont définies
    	if (bubbleRenderer != null && bubbleRenderer.material != null)
    	{
   	     	bubbleRenderer.material.SetColor(BaseColorProperty, baseColor);
    	    bubbleRenderer.material.SetColor(ActiveColorProperty, activeColor);
        	bubbleRenderer.material.SetFloat(ColorIntensityProperty, 0);
        	bubbleRenderer.material.SetFloat(DeformAmountProperty, deformAmount);
    	}
    
    	// Définir la taille initiale
    	currentRadius = baseRadius;
    	transform.localScale = Vector3.one * currentRadius * 2;
	}
    
    void Update()
    {
        if (audioAnalyzer == null || bubbleRenderer == null) return;
        
        // Obtenir le volume actuel
        float volume = audioAnalyzer.GetCurrentVolume();
        
        // Mettre à jour la taille cible
        targetRadius = baseRadius + (volume * volumeMultiplier);
        
        // Mettre à jour l'intensité de couleur cible
        targetColorIntensity = Mathf.Clamp01(volume * 2);
        
        // Animer la taille avec lissage
        currentRadius = Mathf.Lerp(currentRadius, targetRadius, Time.deltaTime * smoothingSpeed);
        transform.localScale = Vector3.one * currentRadius * 2;
        
        // Animer la couleur avec lissage
        currentColorIntensity = Mathf.Lerp(currentColorIntensity, targetColorIntensity, 
                                      Time.deltaTime * colorTransitionSpeed);
        
        // Mettre à jour l'effet de pulsation
        pulseTime += Time.deltaTime * pulseSpeed;
        
        // Mettre à jour les paramètres du shader
        Material mat = bubbleRenderer.material;
        mat.SetFloat(ColorIntensityProperty, currentColorIntensity);
        mat.SetFloat(PulseTimeProperty, pulseTime);
        
        // Ajuster la déformation en fonction du volume
        float deform = deformAmount * (0.5f + volume * 0.5f);
        mat.SetFloat(DeformAmountProperty, deform);
    }
}