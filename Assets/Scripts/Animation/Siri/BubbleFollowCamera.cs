using UnityEngine;
using UnityEngine.XR;

public class BubbleFollowCamera : MonoBehaviour
{
    [Header("Positioning")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float followDistance = 1.5f;
    [SerializeField] private float smoothing = 5f;
    [SerializeField] private Vector3 positionOffset = new Vector3(0, -0.3f, 0);
    [SerializeField] private float minDistanceFromObstacles = 0.3f;
    
    [Header("Movement Damping")]
    [SerializeField] private float movementDamping = 0.95f;
    
    private Vector3 targetPosition;
    private Vector3 velocity = Vector3.zero;
    
    void Start()
    {
        if (cameraTransform == null)
        {
            // Trouver automatiquement la caméra XR ou la caméra principale
            Camera xrCamera = null;
            
            // Chercher d'abord la caméra dans un XR Rig
            var xrRigs = FindObjectsOfType<MonoBehaviour>();
            foreach (var rig in xrRigs)
            {
                // Vérifier différents types de XR Rig en fonction de la version
                if (rig.GetType().Name == "XROrigin" || 
                    rig.GetType().Name == "XRRig" || 
                    rig.GetType().Name == "XRCameraRig")
                {
                    // Utiliser la réflexion pour trouver la caméra
                    var cameraProperty = rig.GetType().GetProperty("Camera");
                    if (cameraProperty != null)
                    {
                        xrCamera = cameraProperty.GetValue(rig) as Camera;
                        if (xrCamera != null) break;
                    }
                    
                    // Si pas de propriété Camera, chercher un composant Camera enfant
                    xrCamera = rig.GetComponentInChildren<Camera>();
                    if (xrCamera != null) break;
                }
            }
            
            // Si aucune caméra XR n'est trouvée, utiliser la caméra principale
            if (xrCamera != null)
            {
                cameraTransform = xrCamera.transform;
                Debug.Log("Caméra XR trouvée et assignée automatiquement.");
            }
            else
            {
                cameraTransform = Camera.main.transform;
                Debug.Log("Caméra principale assignée par défaut.");
            }
        }
        
        // Position initiale
        UpdateTargetPosition();
        transform.position = targetPosition;
    }
    
    void LateUpdate()
    {
        if (cameraTransform == null) return;
        
        // Mettre à jour la position cible
        UpdateTargetPosition();
        
        // Mouvement lissé vers la cible
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, 
                                             ref velocity, 1.0f / smoothing, 
                                             Mathf.Infinity, Time.deltaTime);
        
        // Appliquer l'amortissement du mouvement
        velocity *= movementDamping;
        
        // Toujours faire face à la caméra
        transform.LookAt(cameraTransform);
    }
    
    private void UpdateTargetPosition()
    {
        // Calculer la position devant la caméra
        targetPosition = cameraTransform.position + 
                        cameraTransform.forward * followDistance +
                        cameraTransform.up * positionOffset.y +
                        cameraTransform.right * positionOffset.x;
        
        // Vérifier les obstacles
        RaycastHit hit;
        if (Physics.Linecast(cameraTransform.position, targetPosition, out hit))
        {
            // S'il y a un obstacle, positionner à une distance minimale de l'obstacle
            float distanceToObstacle = Vector3.Distance(cameraTransform.position, hit.point);
            float adjustedDistance = Mathf.Max(minDistanceFromObstacles, 
                                          distanceToObstacle - minDistanceFromObstacles);
            
            targetPosition = cameraTransform.position + 
                           cameraTransform.forward * adjustedDistance +
                           positionOffset;
        }
    }
}