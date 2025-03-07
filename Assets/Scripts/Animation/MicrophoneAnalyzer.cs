using UnityEngine;

public class MicrophoneAnalyzer : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private string microphoneDeviceName = null; // null = default device
    [SerializeField] private int sampleRate = 44100;
    [SerializeField] private int clipLength = 2; // en secondes
    
    [Header("Analysis Settings")]
    [SerializeField] private int sampleDataLength = 256;
    [SerializeField] private FFTWindow fftWindow = FFTWindow.Blackman;
    
    [Header("Visualization References")]
    [SerializeField] private Transform visualizerObject;
    [SerializeField] private float scaleMultiplier = 2f;
    [SerializeField] private float smoothingSpeed = 5f;
    
    // Ajout d'un AudioSource pour jouer le microphone
    [SerializeField] private AudioSource microphoneSource;
    [SerializeField] private bool playMicrophoneAudio = false;
    
    // Spectrum Analysis
    [Header("Spectrum Analysis")]
    [SerializeField] private int spectrumSize = 64;
    [SerializeField] private float spectrumMultiplier = 50f;
    [SerializeField] private float spectrumSmoothingSpeed = 3f;
    
    // Variables privées
    private AudioClip microphoneClip;
    private float[] sampleData;
    private float currentVolume = 0f;
    private float targetVolume = 0f;
    private bool isMicrophoneActive = false;
    
    // Variables pour le spectre
    private float[] spectrumData;
    private float[] smoothedSpectrumData;
    
    // Pour le debug
    [SerializeField] private TMPro.TextMeshProUGUI debugText;
    
    // Propriété pour accéder aux données du spectre
    public float[] SmoothedSpectrum => smoothedSpectrumData;
    
    void Start()
    {
        // Initialiser les tableaux pour les échantillons audio
        sampleData = new float[sampleDataLength];
        spectrumData = new float[spectrumSize];
        smoothedSpectrumData = new float[spectrumSize];
        
        // Vérifier si l'AudioSource est assigné
        if (microphoneSource == null)
        {
            // Tenter de trouver un AudioSource sur cet objet
            microphoneSource = GetComponent<AudioSource>();
            
            // Si toujours pas d'AudioSource, en créer un
            if (microphoneSource == null)
            {
                microphoneSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        // Lister les périphériques disponibles et les afficher dans le debug
        string micDevices = "Microphones disponibles: ";
        foreach (string device in Microphone.devices)
        {
            micDevices += device + ", ";
        }
        
        if (Microphone.devices.Length == 0)
        {
            Debug.LogWarning("Aucun microphone détecté!");
            if (debugText != null)
                debugText.text = "Aucun microphone détecté!";
            return;
        }
        
        // Si aucun nom de périphérique n'est spécifié, utiliser le premier disponible
        if (string.IsNullOrEmpty(microphoneDeviceName) && Microphone.devices.Length > 0)
        {
            microphoneDeviceName = Microphone.devices[0];
        }
        
        Debug.Log(micDevices);
        if (debugText != null)
            debugText.text = micDevices;
        
        // Démarrer l'enregistrement du microphone
        StartMicrophone();
    }
    
    void StartMicrophone()
    {
        // Créer un AudioClip pour capturer l'audio du microphone
        microphoneClip = Microphone.Start(microphoneDeviceName, true, clipLength, sampleRate);
        
        if (microphoneClip == null)
        {
            Debug.LogError("Échec de démarrage du microphone!");
            if (debugText != null)
                debugText.text = "Échec de démarrage du microphone!";
            return;
        }
        
        // Attendre que le microphone démarre
        while (!(Microphone.GetPosition(microphoneDeviceName) > 0)) { }
        
        // Configurer l'AudioSource pour jouer le son du microphone
        microphoneSource.clip = microphoneClip;
        microphoneSource.loop = true;
        microphoneSource.mute = !playMicrophoneAudio; // Jouer ou non le son
        microphoneSource.Play();
        
        isMicrophoneActive = true;
        
        if (debugText != null)
            debugText.text = "Microphone actif: " + microphoneDeviceName;
        
        Debug.Log("Microphone démarré: " + microphoneDeviceName);
    }
    
    void Update()
    {
        if (!isMicrophoneActive) return;
        
        // Obtenir les données audio du microphone
        AnalyzeAudio();
        
        // Analyser le spectre audio
        AnalyzeSpectrum();
        
        // Animer l'objet visualiseur
        AnimateVisualizer();
        
        // Mettre à jour le texte de debug
        if (debugText != null)
            debugText.text = string.Format("Volume: {0:F3} | Max Spectrum: {1:F3}", 
                                        currentVolume, 
                                        smoothedSpectrumData.Length > 0 ? 
                                        GetMaxValue(smoothedSpectrumData) : 0);
    }
    
    void AnalyzeAudio()
    {
        // Obtenir la position actuelle d'enregistrement
        int micPosition = Microphone.GetPosition(microphoneDeviceName);
        
        // Obtenir les données audio et calculer le volume
        microphoneClip.GetData(sampleData, micPosition - sampleData.Length >= 0 ? micPosition - sampleData.Length : 0);
        
        float sum = 0;
        for (int i = 0; i < sampleData.Length; i++)
        {
            sum += Mathf.Abs(sampleData[i]);
        }
        
        // Mettre à jour le volume cible
        targetVolume = sum / sampleData.Length;
    }
    
    void AnalyzeSpectrum()
    {
        // Utiliser l'AudioSource pour obtenir les données du spectre
        microphoneSource.GetSpectrumData(spectrumData, 0, fftWindow);
        
        // Lisser les données du spectre
        for (int i = 0; i < spectrumSize; i++)
        {
            // Appliquer un multiplicateur pour mieux voir les valeurs
            float targetValue = spectrumData[i] * spectrumMultiplier;
            
            // Lisser la transition
            smoothedSpectrumData[i] = Mathf.Lerp(smoothedSpectrumData[i], 
                                              targetValue, 
                                              Time.deltaTime * spectrumSmoothingSpeed);
        }
    }
    
    void AnimateVisualizer()
    {
        if (visualizerObject == null) return;
        
        // Lisser la transition du volume
        currentVolume = Mathf.Lerp(currentVolume, targetVolume, Time.deltaTime * smoothingSpeed);
        
        // Calculer la nouvelle échelle en fonction du volume
        float newScale = 0.1f + (currentVolume * scaleMultiplier);
        
        // Appliquer l'échelle à l'objet visualiseur
        visualizerObject.localScale = new Vector3(newScale, newScale, newScale);
    }
    
    // Fonction utilitaire pour trouver la valeur maximale dans un tableau
    private float GetMaxValue(float[] array)
    {
        float max = 0;
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i] > max) max = array[i];
        }
        return max;
    }
    
    // Méthode publique pour accéder au volume actuel
    public float GetCurrentVolume()
    {
        return currentVolume;
    }
    
    void OnDestroy()
    {
        // Arrêter le microphone quand le script est détruit
        if (isMicrophoneActive)
        {
            Microphone.End(microphoneDeviceName);
        }
    }
}