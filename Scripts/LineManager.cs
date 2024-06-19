using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.AR;
using TMPro;
using UnityEngine.EventSystems;


public class LineManager : MonoBehaviour
{
    // La ligne 
    public LineRenderer lineRenderer;
    // Le component de gestion des action
    public ARPlacementInteractable placementInteractable;
    // Le texte de la distance
    public TextMeshPro mText;
    // 
    private int pointCount = 0;
    public bool continous;
    // Ligne entre deux points
    LineRenderer line;

    // Button de mode de mesure
    public TextMeshProUGUI buttonText;
    public TextMeshPro clearButton;
    public TextMeshProUGUI undoButton;
    public GameObject placementPrefab;
    private GameObject placementObject;


    // Start is called before the first frame update
    void Start()
    {
        // On intercepte un evenement de click
        placementInteractable.objectPlaced.AddListener(DrawLine);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    // Change le mode  de mesure
    public void ToggleBetweenContinousAndDiscrete()
    {
        continous = !continous;
        pointCount = 0;

        if(continous)
        {
            buttonText.text = "Mesure continue";
        }
        else
        {
            buttonText.text = "Mesure discrete";
        }
    }
    // Clear the scene
    public void ClearScene()
    {
        Destroy(placementInteractable.placementPrefab);
        
    }
    bool IsPointOverUIObject(Vector2 pos)
    {
        if (EventSystem.current.IsPointerOverGameObject())
            return false;

        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(pos.x, pos.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

        return results.Count > 0;

    }
    // Undo object
    public void UndoAction()
    {
        if(pointCount > 0)
        {
            line.positionCount = pointCount-1;
        }
    }
    // Dessine les lignes
    void DrawLine(ARObjectPlacementEventArgs args)
    {
        placementObject = args.placementObject;
        bool isOverUI = IsPointOverUIObject(placementObject.transform.position);
       if(!isOverUI)
       {
            //placementObject = Instantiate(placementPrefab,args.placementObject.transform.position);
            
            pointCount++;

            if(pointCount < 2)
            {
                line = Instantiate(lineRenderer);
                line.positionCount = 1;
            }
            else
            {
                line.positionCount = pointCount;
                if(!continous)
                    pointCount = 0;
            }
            
            // On ajoute les points dans le rendu
            line.SetPosition(line.positionCount-1,placementObject.transform.position);

            if(line.positionCount > 1)
            {
                    Vector3 pointA = line.GetPosition(line.positionCount-1);
                    Vector3 pointB = line.GetPosition(line.positionCount-2);
                    // Distance entre les points
                    float dist = Vector3.Distance(pointA,pointB);
                    // Creer un nouveau text a chaque line creer
                    TextMeshPro distText = Instantiate(mText);
                    distText.text = dist.ToString() + " m";
                    // La direction du vecteur
                    Vector3 directionVector = (pointB - pointA);
                    // La normal
                    Vector3 normal = args.placementObject.transform.up;
                    Vector3 upd = Vector3.Cross(directionVector,normal).normalized;
                    // On determine la rotation
                    Quaternion rotation = Quaternion.LookRotation(-normal,upd);

                    // On positionne bien le text
                    distText.transform.rotation = rotation;
                    distText.transform.position = (pointA + directionVector * 0.5f) + upd * 0.05f;
            }
       }
       return;
    }
}
