using UnityEngine;

public class ThinkingAnimationController : MonoBehaviour 
{
    [SerializeField] private GameObject thinkingEffectObject;
    [SerializeField] private float pulsationSpeed = 1.0f;
    [SerializeField] private float minScale = 0.9f;
    [SerializeField] private float maxScale = 1.1f;
    
    private bool isThinking = false;
    
    void Update() 
    {
        if (isThinking) 
        {
            // Animation de pulsation simple
            float scale = Mathf.Lerp(minScale, maxScale, 
                (Mathf.Sin(Time.time * pulsationSpeed) + 1f) / 2f);
            thinkingEffectObject.transform.localScale = Vector3.one * scale;
        }
    }
    
    public void StartThinking() 
    {
        isThinking = true;
        thinkingEffectObject.SetActive(true);
    }
    
    public void StopThinking() 
    {
        isThinking = false;
        thinkingEffectObject.SetActive(false);
    }
}