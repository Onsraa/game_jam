using UnityEngine;

public class SiriLikeVisualizer : MonoBehaviour
{
    [SerializeField] private MicrophoneAnalyzer audioAnalyzer;
    [SerializeField] private SubtitleDisplay subtitleDisplay;
    
    [Header("Size Settings")]
    [SerializeField] private float baseRadius = 0.3f;
    [SerializeField] private float volumeMultiplier = 0.3f;
    [SerializeField] private float maxRadius = 0.5f;
    [SerializeField] private float smoothingSpeed = 5f;
    
    [Header("Core Visualization")]
    [SerializeField] private Transform coreTransform; // Le "cœur" blanc au centre
    [SerializeField] private float coreBaseScale = 0.5f;
    [SerializeField] private float coreMaxScale = 1.2f;
    [SerializeField] private float coreResponseSpeed = 8f;
    [SerializeField] private float coreGlowIntensity = 2.5f; // Intensité de la lumière du cœur
    [SerializeField] private Color coreGlowColor = new Color(1f, 1f, 1f, 0.8f); // Couleur du cœur
    
    [Header("Noise Settings")]
    [SerializeField] private float minNoiseIntensity = 0.01f; // Bruit minimum en standby
    [SerializeField] private float maxNoiseIntensity = 0.06f; // Bruit maximum lors de détection de son
    [SerializeField] private float noiseSmoothSpeed = 4f;     // Vitesse de transition
    [SerializeField] private float soundDetectionThreshold = 0.02f; // Seuil de détection du son
    
    [Header("Wave Shape Settings")]
    [SerializeField] [Range(0.1f, 5f)] private float waveSharpness = 1.0f;
    [SerializeField] [Range(0.5f, 5f)] private float waveFrequency = 1.0f;
    
    [Header("Audio Analysis")]
    [SerializeField] private int bassFreqBand = 2;
    [SerializeField] private int midFreqBand = 10;
    [SerializeField] private int highFreqBand = 20;
    [SerializeField] private float frequencyResponseMultiplier = 3.0f;
    [SerializeField] private float audioResponseSpeed = 5.0f;
    
    // Variables privées
    private Renderer sphereRenderer;
    private Renderer coreRenderer;
    private float currentRadius = 0.3f;
    private float targetRadius = 0.3f;
    private float bassLevel = 0.0f;
    private float midLevel = 0.0f;
    private float highLevel = 0.0f;
    private float volumeLevel = 0.0f;
    private float coreScale = 0.5f;
    private float targetCoreScale = 0.5f;
    private float currentNoiseIntensity;
    private float targetNoiseIntensity;
    private Material coreMaterial;
    
    // Shader property IDs
    private static readonly int BassLevelProperty = Shader.PropertyToID("_BassLevel");
    private static readonly int MidLevelProperty = Shader.PropertyToID("_MidLevel");
    private static readonly int HighLevelProperty = Shader.PropertyToID("_HighLevel");
    private static readonly int VolumeLevelProperty = Shader.PropertyToID("_VolumeLevel");
    private static readonly int WaveSharpnessProperty = Shader.PropertyToID("_WaveSharpness");
    private static readonly int WaveFrequencyProperty = Shader.PropertyToID("_WaveFrequency");
    private static readonly int NoiseIntensityProperty = Shader.PropertyToID("_NoiseIntensity");
    private static readonly int EmissionColorProperty = Shader.PropertyToID("_EmissionColor");
    
    void Start()
    {
        // Récupérer le renderer de la bulle
        sphereRenderer = GetComponent<Renderer>();
        if (sphereRenderer == null)
        {
            Debug.LogError("Aucun Renderer trouvé sur l'objet sphère!");
            return;
        }
        
        // Initialiser l'intensité du bruit
        currentNoiseIntensity = minNoiseIntensity;
        targetNoiseIntensity = minNoiseIntensity;
        
        // Créer le cœur s'il n'existe pas
        SetupCore();
        
        // Créer le système de sous-titres s'il n'existe pas
        SetupSubtitles();
        
        // Relier l'événement de changement de sous-titre
        if (audioAnalyzer != null && subtitleDisplay != null)
        {
            audioAnalyzer.OnSubtitleChanged.AddListener(subtitleDisplay.DisplaySubtitle);
        }
        
        // Définir la taille initiale
        currentRadius = baseRadius;
        transform.localScale = Vector3.one * currentRadius * 2;
    }
    
    private void SetupCore()
    {
        if (coreTransform == null)
        {
            // Créer un objet pour le "cœur" lumineux central
            GameObject coreObj = new GameObject("SiriCore");
            coreTransform = coreObj.transform;
            coreTransform.SetParent(transform.parent);
            coreTransform.position = transform.position;
            
            // Ajouter un MeshFilter et un MeshRenderer
            MeshFilter mf = coreObj.AddComponent<MeshFilter>();
            mf.mesh = GetComponent<MeshFilter>().mesh; // Même mesh que la bulle
            
            coreRenderer = coreObj.AddComponent<MeshRenderer>();
            
            // Créer un matériau spécial pour le cœur lumineux
            coreMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            coreMaterial.SetFloat("_Surface", 1); // 1 = Transparent
            coreMaterial.SetFloat("_Blend", 0);  // 0 = Alpha blend
            coreMaterial.EnableKeyword("_EMISSION");
            
            // Augmenter le métallic et le smoothness pour un aspect plus lumineux
            coreMaterial.SetFloat("_Metallic", 0.8f);
            coreMaterial.SetFloat("_Smoothness", 1.0f);
            
            // Couleur de base blanche légèrement transparente
            coreMaterial.color = coreGlowColor;
            
            // Emission initiale (sera mise à jour dynamiquement)
            coreMaterial.SetColor(EmissionColorProperty, coreGlowColor * coreGlowIntensity);
            
            // Ajuster la queue de rendu pour qu'il apparaisse bien
            coreMaterial.renderQueue = 3100;
            
            coreRenderer.material = coreMaterial;
            
            // Définir l'échelle initiale
            coreScale = coreBaseScale;
            coreTransform.localScale = Vector3.one * coreScale;
            
            // Ajouter un Point Light pour renforcer l'effet lumineux
            Light pointLight = coreObj.AddComponent<Light>();
            pointLight.type = LightType.Point;
            pointLight.color = coreGlowColor;
            pointLight.intensity = coreGlowIntensity * 0.5f; // Intensité de départ
            pointLight.range = 1.0f;
        }
        else
        {
            coreRenderer = coreTransform.GetComponent<Renderer>();
            if (coreRenderer != null)
            {
                coreMaterial = coreRenderer.material;
            }
        }
    }
    
    private void SetupSubtitles()
    {
        if (subtitleDisplay == null)
        {
            // Créer le système de sous-titres
            GameObject subtitleObj = new GameObject("SubtitleSystem");
            subtitleObj.transform.SetParent(transform);
            subtitleObj.transform.localPosition = Vector3.zero;
            
            subtitleDisplay = subtitleObj.AddComponent<SubtitleDisplay>();
        }
    }
    
    void Update()
    {
        if (audioAnalyzer == null || sphereRenderer == null) return;
        
        // Gérer les entrées clavier
        HandleKeyboardInput();
        
        // Obtenir le volume et analyser les fréquences
        float volume = audioAnalyzer.GetCurrentVolume();
        AnalyzeFrequencyBands();
        
        // Mettre à jour la taille cible avec limitation
        targetRadius = Mathf.Clamp(baseRadius + (volume * volumeMultiplier), baseRadius, maxRadius);
        
        // Animer la taille avec lissage
        currentRadius = Mathf.Lerp(currentRadius, targetRadius, Time.deltaTime * smoothingSpeed);
        transform.localScale = Vector3.one * currentRadius * 2;
        
        // Détection de son pour contrôler le noise intensity
        UpdateNoiseIntensity(volume);
        
        // Animer le "cœur" en fonction des fréquences
        AnimateCore();
        
        // Mettre à jour le niveau de volume
        volumeLevel = Mathf.Lerp(volumeLevel, volume, Time.deltaTime * audioResponseSpeed);
        
        // Appliquer les valeurs au shader
        UpdateShaderProperties();
    }
    
    void LateUpdate()
    {
        // S'assurer que le cœur suit la bulle si elle bouge
        if (coreTransform != null)
        {
            coreTransform.position = transform.position;
        }
    }
    
    private void HandleKeyboardInput()
    {
        // Touche P pour lire le fichier audio
        if (Input.GetKeyDown(KeyCode.P))
        {
            audioAnalyzer.PlayAudioFile();
        }
        
        // Espace maintenu pour écouter le microphone
        if (Input.GetKeyDown(KeyCode.Space))
        {
            audioAnalyzer.StartListening();
        }
        
        if (Input.GetKeyUp(KeyCode.Space))
        {
            audioAnalyzer.StopListening();
        }
    }
    
    private void UpdateNoiseIntensity(float volume)
    {
        // Déterminer si un son est détecté (volume au-dessus du seuil)
        bool soundDetected = volume > soundDetectionThreshold;
        
        // Définir l'intensité du bruit cible en fonction de la détection du son
        targetNoiseIntensity = soundDetected ? maxNoiseIntensity : minNoiseIntensity;
        
        // Transition fluide entre les états
        currentNoiseIntensity = Mathf.Lerp(currentNoiseIntensity, targetNoiseIntensity, 
                                         Time.deltaTime * noiseSmoothSpeed);
    }
    
    private void AnimateCore()
    {
        if (coreTransform == null || coreRenderer == null) return;
        
        // Calculer l'échelle cible en fonction de l'influence des différentes fréquences
        float bassInfluence = bassLevel * 0.6f;  
        float midInfluence = midLevel * 0.3f;    
        float highInfluence = highLevel * 0.1f;  
        
        // Combiner les influences pour un effet organique
        float combinedInfluence = bassInfluence + midInfluence + highInfluence;
        
        // Calculer l'échelle cible avec limitation
        targetCoreScale = Mathf.Lerp(
            coreBaseScale,
            coreMaxScale,
            Mathf.Pow(combinedInfluence, 1.5f)  // Réponse non-linéaire
        );
        
        // Lisser l'animation du cœur avec une réponse rapide
        coreScale = Mathf.Lerp(coreScale, targetCoreScale, Time.deltaTime * coreResponseSpeed);
        
        // Ajouter une légère pulsation organique
        float pulseVariation = 1f + Mathf.Sin(Time.time * 5f) * 0.05f * combinedInfluence;
        coreTransform.localScale = Vector3.one * coreScale * pulseVariation;
        
        // Mettre à jour l'émission du matériau pour l'effet lumineux
        if (coreMaterial != null)
        {
            // Faire varier l'intensité de l'émission en fonction de l'activité audio
            float emissionIntensity = coreGlowIntensity * (1f + combinedInfluence * 2f);
            Color emissionColor = coreGlowColor * emissionIntensity;
            coreMaterial.SetColor(EmissionColorProperty, emissionColor);
            
            // Faire varier l'opacité en fonction de l'activité
            Color baseColor = coreGlowColor;
            baseColor.a = Mathf.Lerp(0.5f, 0.8f, combinedInfluence);
            coreMaterial.color = baseColor;
            
            // Mettre à jour la lumière si elle existe
            Light pointLight = coreTransform.GetComponent<Light>();
            if (pointLight != null)
            {
                pointLight.intensity = coreGlowIntensity * 0.5f * (1f + combinedInfluence * 3f);
                
                // Faire légèrement varier la couleur de la lumière (optionnel)
                float hueShift = Mathf.Sin(Time.time * 2f) * 0.05f * combinedInfluence;
                pointLight.color = Color.HSVToRGB(
                    Mathf.Repeat(0.6f + hueShift, 1f), // Teinte légèrement variable
                    0.2f, // Saturation faible pour rester proche du blanc
                    1f    // Luminosité maximale
                );
            }
        }
    }
    
    private void UpdateShaderProperties()
    {
        // Mettre à jour les propriétés du shader de la bulle
        sphereRenderer.material.SetFloat(BassLevelProperty, bassLevel);
        sphereRenderer.material.SetFloat(MidLevelProperty, midLevel);
        sphereRenderer.material.SetFloat(HighLevelProperty, highLevel);
        sphereRenderer.material.SetFloat(VolumeLevelProperty, volumeLevel);
        
        // Propriétés pour le contrôle des vagues
        sphereRenderer.material.SetFloat(WaveSharpnessProperty, waveSharpness);
        sphereRenderer.material.SetFloat(WaveFrequencyProperty, waveFrequency);
        
        // Mettre à jour l'intensité du bruit dynamiquement
        sphereRenderer.material.SetFloat(NoiseIntensityProperty, currentNoiseIntensity);
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
        
        // Extraire les valeurs des différentes bandes de fréquence
        float targetBass = Mathf.Clamp01(spectrum[bassIndex] * frequencyResponseMultiplier);
        float targetMid = Mathf.Clamp01(spectrum[midIndex] * frequencyResponseMultiplier);
        float targetHigh = Mathf.Clamp01(spectrum[highIndex] * frequencyResponseMultiplier);
        
        // Lissage pour une transition fluide mais réactive
        bassLevel = Mathf.Lerp(bassLevel, targetBass, Time.deltaTime * audioResponseSpeed);
        midLevel = Mathf.Lerp(midLevel, targetMid, Time.deltaTime * audioResponseSpeed);
        highLevel = Mathf.Lerp(highLevel, targetHigh, Time.deltaTime * audioResponseSpeed);
    }
}