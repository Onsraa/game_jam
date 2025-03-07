using UnityEngine;
using System.Collections.Generic;

public class AudioAnalyzer : MonoBehaviour 
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private int sampleSize = 256;
    private float[] samples;
    private float averageVolume;
    
    void Start() 
    {
        samples = new float[sampleSize];
    }
    
    void Update() 
    {
        // Analyse du volume et des fréquences
        audioSource.GetSpectrumData(samples, 0, FFTWindow.Blackman);
        
        // Calcul du volume moyen
        float sum = 0;
        for (int i = 0; i < samples.Length; i++) 
        {
            sum += samples[i];
        }
        averageVolume = sum / samples.Length;
        
        // Envoyer les données aux systèmes d'animation
        BubbleController.Instance.UpdateAnimation(averageVolume, samples);
    }
}