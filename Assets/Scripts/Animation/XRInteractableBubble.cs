using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class XRInteractableBubble : XRSimpleInteractable 
{
    [SerializeField] private Transform followTransform;
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private float maxFollowDistance = 2f;
    
    private Vector3 targetPosition;
    private bool isFollowing = false;
    
    protected override void OnSelectEntered(SelectEnterEventArgs args) 
    {
        base.OnSelectEntered(args);
        
        // Commencer à suivre la main de l'utilisateur
        isFollowing = true;
    }
    
    protected override void OnSelectExited(SelectExitEventArgs args) 
    {
        base.OnSelectExited(args);
        
        // Arrêter de suivre la main
        isFollowing = false;
    }
    
    void Update() 
    {
        if (isFollowing && followTransform != null) 
        {
            // Calculer la position cible (limiter la distance)
            Vector3 direction = followTransform.position - transform.position;
            float distance = direction.magnitude;
            
            if (distance > maxFollowDistance) 
            {
                targetPosition = transform.position + direction.normalized * maxFollowDistance;
            } 
            else 
            {
                targetPosition = followTransform.position;
            }
            
            // Déplacement fluide
            transform.position = Vector3.Lerp(transform.position, targetPosition, 
                followSpeed * Time.deltaTime);
        }
    }
}