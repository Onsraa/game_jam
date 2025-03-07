using UnityEngine;

public class MicrophoneAnalyzer : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private string microphoneDeviceName = null; // null = default device
    [SerializeField] private int sampleRate = 44100;
    [SerializeField] private int clipLength = 2; // en secondes
    [SerializeField] private AudioSource microphoneSource;
    [SerializeField] private bool playMicrophoneAudio = false;
    
    [Header("Analysis Settings")]
    [SerializeField] private int sampleDataLength = 256;
    [SerializeField] private FFTWindow fftWindow = FFTWindow.Blackman;
    [SerializeField] private float smoothingSpeed = 5f;
    [SerializeField] private int spectrumSize = 64;
    [SerializeField] private float spectrumMultiplier = 50f;
    [SerializeField] private float spectrumSmoothingSpeed = 8f; // Augmenté pour une meilleure réactivité
    
    // Variables privées
    private AudioClip microphoneClip;
    private float[] sampleData;
    private float currentVolume = 0f;
    private float targetVolume = 0f;
    private bool isMicrophoneActive = false;
    
    // Variables pour le spectre
    private float[] spectrumData;
    private float[] smoothedSpectrumData;
    
    // Propriétés publiques
    public float[] SmoothedSpectrum => smoothedSpectrumData;
    
    void Start()
    {
        InitializeAudioAnalysis();
        StartMicrophone();
    }
    
    private void InitializeAudioAnalysis()
    {
        // Initialiser les tableaux
        sampleData = new float[sampleDataLength];
        spectrumData = new float[spectrumSize];
        smoothedSpectrumData = new float[spectrumSize];
        
        // Configurer l'AudioSource
        if (microphoneSource == null)
        {
            microphoneSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        }
        
        // Sélectionner le premier microphone disponible si aucun n'est spécifié
        if (string.IsNullOrEmpty(microphoneDeviceName) && Microphone.devices.Length > 0)
        {
            microphoneDeviceName = Microphone.devices[0];
        }
    }
    
    private void StartMicrophone()
    {
        if (Microphone.devices.Length == 0)
        {
            Debug.LogWarning("Aucun microphone détecté!");
            return;
        }
        
        // Créer un AudioClip pour capturer l'audio du microphone
        microphoneClip = Microphone.Start(microphoneDeviceName, true, clipLength, sampleRate);
        
        if (microphoneClip == null)
        {
            Debug.LogError("Échec de démarrage du microphone!");
            return;
        }
        
        // Attendre que le microphone démarre
        while (!(Microphone.GetPosition(microphoneDeviceName) > 0)) { }
        
        // Configurer l'AudioSource
        microphoneSource.clip = microphoneClip;
        microphoneSource.loop = true;
        microphoneSource.mute = !playMicrophoneAudio;
        microphoneSource.Play();
        
        isMicrophoneActive = true;
    }
    
    void Update()
    {
        if (!isMicrophoneActive) return;
        
        AnalyzeAudio();
        AnalyzeSpectrum();
    }
    
    private void AnalyzeAudio()
    {
        // Obtenir la position actuelle d'enregistrement
        int micPosition = Microphone.GetPosition(microphoneDeviceName);
        
        // Éviter les indices négatifs
        int readPosition = micPosition - sampleData.Length;
        if (readPosition < 0) readPosition = 0;
        
        // Obtenir les données audio
        microphoneClip.GetData(sampleData, readPosition);
        
        // Calculer le volume moyen
        float sum = 0;
        for (int i = 0; i < sampleData.Length; i++)
        {
            sum += Mathf.Abs(sampleData[i]);
        }
        
        targetVolume = sum / sampleData.Length;
        currentVolume = Mathf.Lerp(currentVolume, targetVolume, Time.deltaTime * smoothingSpeed);
    }
    
    private void AnalyzeSpectrum()
    {
        // Obtenir les données du spectre
        microphoneSource.GetSpectrumData(spectrumData, 0, fftWindow);
        
        // Traiter et lisser les données
        for (int i = 0; i < spectrumSize; i++)
        {
            float targetValue = spectrumData[i] * spectrumMultiplier;
            smoothedSpectrumData[i] = Mathf.Lerp(smoothedSpectrumData[i], 
                                              targetValue, 
                                              Time.deltaTime * spectrumSmoothingSpeed);
        }
    }
    
    public float GetCurrentVolume()
    {
        return currentVolume;
    }
    
    void OnDestroy()
    {
        if (isMicrophoneActive)
        {
            Microphone.End(microphoneDeviceName);
        }
    }
}