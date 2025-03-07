using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CustomizationManager : MonoBehaviour 
{
    [System.Serializable]
    public class CustomizationOptions 
    {
        public Color[] bubbleColors;
        public AnimationClip[] animationStyles;
        public float[] textSizes;
    }
    
    [SerializeField] private CustomizationOptions options;
    [SerializeField] private Material bubbleMaterial;
    [SerializeField] private Animator bubbleAnimator;
    [SerializeField] private TextMeshProUGUI subtitleText;
    
    public void SetBubbleColor(int colorIndex) 
    {
        if (colorIndex >= 0 && colorIndex < options.bubbleColors.Length) 
        {
            bubbleMaterial.SetColor("_BaseColor", options.bubbleColors[colorIndex]);
        }
    }
    
    public void SetAnimationStyle(int styleIndex) 
    {
        if (styleIndex >= 0 && styleIndex < options.animationStyles.Length) 
        {
            bubbleAnimator.Play(options.animationStyles[styleIndex].name);
        }
    }
    
    public void SetTextSize(int sizeIndex) 
    {
        if (sizeIndex >= 0 && sizeIndex < options.textSizes.Length) 
        {
            subtitleText.fontSize = options.textSizes[sizeIndex];
        }
    }
}