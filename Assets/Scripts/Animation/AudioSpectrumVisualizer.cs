// AudioSpectrumVisualizer.cs
using UnityEngine;

public class AudioSpectrumVisualizer : MonoBehaviour
{
    [SerializeField] private MicrophoneAnalyzer audioAnalyzer;
    [SerializeField] private int numberOfBars = 32;
    [SerializeField] private float barHeight = 2.0f;
    [SerializeField] private float barWidth = 0.05f;
    [SerializeField] private float spacing = 0.01f;
    [SerializeField] private Color startColor = Color.blue;
    [SerializeField] private Color endColor = Color.red;
    [SerializeField] private bool useLogarithmicScale = true;
    [SerializeField] private float minimumHeight = 0.01f;
    
    private Transform[] bars;
    private Renderer[] barRenderers;
    
    void Start()
    {
        CreateBars();
    }
    
    void CreateBars()
    {
        bars = new Transform[numberOfBars];
        barRenderers = new Renderer[numberOfBars];
        
        // Calculer la largeur totale
        float totalWidth = numberOfBars * (barWidth + spacing) - spacing;
        float startX = -totalWidth / 2 + barWidth / 2;
        
        for (int i = 0; i < numberOfBars; i++)
        {
            // Créer une barre
            GameObject bar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bar.name = $"Bar_{i}";
            bar.transform.SetParent(transform);
            bar.transform.localPosition = new Vector3(startX + i * (barWidth + spacing), 0, 0);
            bar.transform.localScale = new Vector3(barWidth, minimumHeight, barWidth);
            
            // Configurer le renderer
            Renderer renderer = bar.GetComponent<Renderer>();
            Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = Color.Lerp(startColor, endColor, (float)i / numberOfBars);
            renderer.material = material;
            
            // Stocker les références
            bars[i] = bar.transform;
            barRenderers[i] = renderer;
        }
    }
    
    void Update()
    {
        if (audioAnalyzer == null) return;
        
        // Obtenir les données du spectre audio
        float[] spectrumData = audioAnalyzer.SmoothedSpectrum;
        
        if (spectrumData == null || spectrumData.Length == 0) return;
        
        // Mettre à jour la hauteur des barres
        for (int i = 0; i < numberOfBars && i < bars.Length; i++)
        {
            if (bars[i] == null) continue;
            
            // Calculer l'indice dans le spectre (avec échelle logarithmique optionnelle)
            int spectrumIndex;
            if (useLogarithmicScale)
            {
                // Distribution logarithmique pour mieux représenter les fréquences
                spectrumIndex = Mathf.FloorToInt(Mathf.Pow(spectrumData.Length, (float)i / numberOfBars));
            }
            else
            {
                // Distribution linéaire
                spectrumIndex = i * spectrumData.Length / numberOfBars;
            }
            
            // S'assurer que l'indice est dans les limites du tableau
            spectrumIndex = Mathf.Clamp(spectrumIndex, 0, spectrumData.Length - 1);
            
            // Calculer la hauteur en fonction des données du spectre
            float height = minimumHeight + spectrumData[spectrumIndex] * barHeight;
            
            // Mettre à jour l'échelle de la barre
            Vector3 scale = bars[i].localScale;
            scale.y = height;
            bars[i].localScale = scale;
            
            // Centrer la barre verticalement
            Vector3 pos = bars[i].localPosition;
            pos.y = height / 2;
            bars[i].localPosition = pos;
            
            // Mettre à jour la couleur en fonction de la hauteur
            if (barRenderers[i] != null)
            {
                float t = Mathf.InverseLerp(minimumHeight, barHeight, height);
                barRenderers[i].material.color = Color.Lerp(startColor, endColor, t);
            }
        }
    }
}