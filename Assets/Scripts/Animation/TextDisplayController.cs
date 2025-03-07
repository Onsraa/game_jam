using UnityEngine;
using TMPro;
using System.Collections;

public class TextDisplayController : MonoBehaviour 
{
    [SerializeField] private TextMeshProUGUI textDisplay;
    [SerializeField] private float typingSpeed = 0.03f;
    
    private Coroutine typingCoroutine;
    
    public void DisplayText(string text) 
    {
        // Arrêter l'animation précédente si nécessaire
        if (typingCoroutine != null) 
        {
            StopCoroutine(typingCoroutine);
        }
        
        // Démarrer une nouvelle animation de texte
        typingCoroutine = StartCoroutine(TypeText(text));
    }
    
    private IEnumerator TypeText(string text) 
    {
        textDisplay.text = "";
        
        for (int i = 0; i < text.Length; i++) 
        {
            textDisplay.text += text[i];
            yield return new WaitForSeconds(typingSpeed);
        }
        
        typingCoroutine = null;
    }
}