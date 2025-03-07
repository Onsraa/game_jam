using UnityEngine;
using UnityEngine.Events;

public class MicrophoneAnalyzer : MonoBehaviour
{
    public enum AudioMode
    {
        Standby,
        Microphone,
        AudioFile
    }

    [Header("Audio Settings")]
    [SerializeField] private string microphoneDeviceName = null; // null = default device
    [SerializeField] private int sampleRate = 44100;
    [SerializeField] private int clipLength = 2; // en secondes
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private bool playMicrophoneAudio = false;
    [SerializeField] private AudioClip audioFileClip; // Pour le fichier MP3 à tester
    
    [Header("Analysis Settings")]
    [SerializeField] private int sampleDataLength = 256;
    [SerializeField] private FFTWindow fftWindow = FFTWindow.Blackman;
    [SerializeField] private float smoothingSpeed = 5f;
    [SerializeField] private int spectrumSize = 64;
    [SerializeField] private float spectrumMultiplier = 50f;
    [SerializeField] private float spectrumSmoothingSpeed = 8f;
    
    [Header("Events")]
    public UnityEvent<string> OnSubtitleChanged;
    
    // Variables privées
    private AudioClip microphoneClip;
    private float[] sampleData;
    private float currentVolume = 0f;
    private float targetVolume = 0f;
    private bool isMicrophoneInitialized = false;
    
    // Variables pour le spectre
    private float[] spectrumData;
    private float[] smoothedSpectrumData;
    
    // Propriétés publiques
    public float[] SmoothedSpectrum => smoothedSpectrumData;
    public AudioMode CurrentMode { get; private set; } = AudioMode.Standby;
    
    void Start()
    {
        InitializeAudioAnalysis();
        DetectMicrophone();
    }
    
    private void InitializeAudioAnalysis()
    {
        // Initialiser les tableaux
        sampleData = new float[sampleDataLength];
        spectrumData = new float[spectrumSize];
        smoothedSpectrumData = new float[spectrumSize];
        
        // Configurer l'AudioSource
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        }
        
        // Initialiser l'événement si nécessaire
        if (OnSubtitleChanged == null)
        {
            OnSubtitleChanged = new UnityEvent<string>();
        }
    }
    
    private void DetectMicrophone()
    {
        // Vérifier si des microphones sont disponibles
        if (Microphone.devices.Length > 0)
        {
            // Sélectionner le premier microphone
            microphoneDeviceName = Microphone.devices[0];
            Debug.Log("Microphone détecté automatiquement : " + microphoneDeviceName);
            InitializeMicrophone();
        }
        else
        {
            Debug.LogWarning("Aucun microphone détecté !");
        }
    }
    
    private void InitializeMicrophone()
    {
        // Créer un AudioClip pour capturer l'audio du microphone
        microphoneClip = Microphone.Start(microphoneDeviceName, true, clipLength, sampleRate);
        
        if (microphoneClip == null)
        {
            Debug.LogError("Échec de démarrage du microphone!");
            return;
        }
        
        // Attendre que le microphone démarre
        while (!(Microphone.GetPosition(microphoneDeviceName) > 0)) { }
        
        isMicrophoneInitialized = true;
        
        // Par défaut, on commence en mode standby
        SetMode(AudioMode.Standby);
    }
    
    public void SetMode(AudioMode newMode)
    {
        // Si on change de mode, on arrête ce qui est en cours
        if (CurrentMode != newMode)
        {
            audioSource.Stop();
        }
        
        CurrentMode = newMode;
        
        switch (newMode)
        {
            case AudioMode.Standby:
                audioSource.clip = null;
                OnSubtitleChanged?.Invoke("");
                break;
                
            case AudioMode.Microphone:
                if (isMicrophoneInitialized)
                {
                    audioSource.clip = microphoneClip;
                    audioSource.loop = true;
                    audioSource.mute = !playMicrophoneAudio;
                    audioSource.Play();
                    OnSubtitleChanged?.Invoke("Mode écoute activé...");
                }
                break;
                
            case AudioMode.AudioFile:
                if (audioFileClip != null)
                {
                    audioSource.clip = audioFileClip;
                    audioSource.loop = false;
                    audioSource.mute = false;
                    audioSource.Play();
                    OnSubtitleChanged?.Invoke("Lecture du fichier audio...");
                }
                else
                {
                    Debug.LogWarning("Aucun fichier audio assigné !");
                    SetMode(AudioMode.Standby);
                }
                break;
        }
    }
    
    public void PlayAudioFile()
    {
        if (audioFileClip != null)
        {
            SetMode(AudioMode.AudioFile);
        }
    }
    
    public void StartListening()
    {
        if (isMicrophoneInitialized)
        {
            SetMode(AudioMode.Microphone);
        }
    }
    
    public void StopListening()
    {
        SetMode(AudioMode.Standby);
    }
    
    void Update()
    {
        // Toujours analyser, même en mode standby pour les petites animations
        AnalyzeAudio();
        AnalyzeSpectrum();
        
        // Détecter la fin de la lecture du fichier audio
        if (CurrentMode == AudioMode.AudioFile && !audioSource.isPlaying)
        {
            SetMode(AudioMode.Standby);
        }
    }
    
    private void AnalyzeAudio()
    {
        if (CurrentMode == AudioMode.Microphone && isMicrophoneInitialized)
        {
            // Obtenir la position actuelle d'enregistrement
            int micPosition = Microphone.GetPosition(microphoneDeviceName);
            
            // Éviter les indices négatifs
            int readPosition = micPosition - sampleData.Length;
            if (readPosition < 0) readPosition = 0;
            
            // Obtenir les données audio
            microphoneClip.GetData(sampleData, readPosition);
        }
        else if (CurrentMode == AudioMode.AudioFile && audioSource.isPlaying)
        {
            // Obtenir les données audio du fichier
            audioSource.GetOutputData(sampleData, 0);
        }
        else
        {
            // En mode standby, générer un peu de bruit aléatoire pour une légère animation
            for (int i = 0; i < sampleData.Length; i++)
            {
                sampleData[i] = Random.Range(-0.01f, 0.01f);
            }
        }
        
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
        if (audioSource.isPlaying)
        {
            // Obtenir les données du spectre
            audioSource.GetSpectrumData(spectrumData, 0, fftWindow);
        }
        else
        {
            // En mode standby, générer un spectre très faible mais un peu aléatoire
            for (int i = 0; i < spectrumSize; i++)
            {
                spectrumData[i] = Random.Range(0f, 0.005f) * (1f/(i+1));
            }
        }
        
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
        if (isMicrophoneInitialized)
        {
            Microphone.End(microphoneDeviceName);
        }
    }
}