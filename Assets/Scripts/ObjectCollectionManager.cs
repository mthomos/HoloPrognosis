using System.Collections.Generic;
using HoloToolkit.Unity;
using UnityEngine;

public class ObjectCollectionManager : Singleton<ObjectCollectionManager>
{
    //Public Variables - For Editor
    public GameObject TreePrefab;
    public GameObject FruitPrefab;
    public GameObject BoxPrefab;
    public GameObject CalibrationPointPrefab;
    public Vector3 TreeSize;
    public Vector3 FruitSize;
    public Vector3 BoxSize;
    public Vector3 CalibrationPointSize;
    public float FruitScale;
    public int NumberOfFruits;

    private GameObject createdTree;
    private GameObject createdBox;
    private float ScaleFactor;
    private List<GameObject> ActiveHolograms = new List<GameObject>();
    private Dictionary<int, int> HandsForActiveHolograms = new Dictionary<int, int>();//key: id, value: hands
    private bool treeCreated, boxCreated;

    public void CreateCalibrationPoint(Vector3 positionCenter, Quaternion rotation)
    {
        // Stay center in the square but move down to the ground
        var position = positionCenter - new Vector3(0, CalibrationPointSize.y * .5f, 0);
        GameObject newObject = Instantiate(CalibrationPointPrefab, position, rotation);
        newObject.name = "Calibration";
        newObject.tag = "Calibration";
        if (newObject != null)
        {
            newObject.transform.parent = gameObject.transform;
            newObject.transform.localScale = RescaleToSameScaleFactor(CalibrationPointPrefab);
            ActiveHolograms.Add(newObject);
        }
    }

    public void CreateTree(Vector3 positionCenter, Quaternion rotation)
    {
        // Stay center in the square but move down to the ground
        Vector3 position = positionCenter - new Vector3(0, TreeSize.y * .5f, 0);
        GameObject newObject = Instantiate(TreePrefab, position, rotation);
        newObject.name = "Tree";
        if (newObject != null)
        {
            newObject.transform.parent = gameObject.transform;
            newObject.transform.localScale = RescaleToSameScaleFactor(TreePrefab);
            ActiveHolograms.Add(newObject);
            createdTree = newObject;
            HandsForActiveHolograms.Add(newObject.GetInstanceID(), -1);
            treeCreated = true;
            checkIfWorldCreated();
            SetFruitProps();
        }
    }

    public void CreateBox(Vector3 positionCenter, Quaternion rotation)
    {
        // Stay center in the square but move down to the ground
        var position = positionCenter - new Vector3(0, BoxSize.y * .5f, 0);
        GameObject newObject = Instantiate(BoxPrefab, position, rotation);
        newObject.name = "Box";
        if (newObject != null)
        {
            newObject.transform.parent = gameObject.transform;
            newObject.transform.localScale = RescaleToSameScaleFactor(BoxPrefab);
            ActiveHolograms.Add(newObject);
            HandsForActiveHolograms.Add(newObject.GetInstanceID(), -1);
            boxCreated = true;
            checkIfWorldCreated();
            newObject.GetComponent<Renderer>().enabled = false;
            createdBox = newObject;
        }
    }

    private void AddMeshColliderToAllChildren(GameObject obj)
    {
        for (int i = 0; i < obj.transform.childCount; i++)
            obj.transform.GetChild(i).gameObject.AddComponent<MeshCollider>();
    }

    private Vector3 RescaleToSameScaleFactor(GameObject objectToScale)
    {
        if (ScaleFactor == 0f) CalculateScaleFactor();
        return objectToScale.transform.localScale * ScaleFactor;
    }

    private Vector3 RescaleToDesiredSizeProportional(GameObject objectToScale, Vector3 desiredSize)
    {
        float scaleFactor = CalcScaleFactorHelper(objectToScale, desiredSize);
        return objectToScale.transform.localScale * scaleFactor;
    }

    private void CalculateScaleFactor()
    {
        float maxScale = float.MaxValue;
        var ratio = CalcScaleFactorHelper(TreePrefab, TreeSize);
        if (ratio < maxScale) maxScale = ratio;
        ScaleFactor = maxScale;
    }

    private float CalcScaleFactorHelper(GameObject obj, Vector3 desiredSize)
    {
        float maxScale = float.MaxValue;
        var curBounds = GetBoundsForAllChildren(obj).size;
        var difference = desiredSize - curBounds;
        float ratio;

        if (difference.x > difference.y && difference.x > difference.z)
            ratio = desiredSize.x / curBounds.x;
        else if (difference.y > difference.x && difference.y > difference.z)
            ratio = desiredSize.y / curBounds.y;
        else
            ratio = desiredSize.z / curBounds.z;

        if (ratio < maxScale)
            maxScale = ratio;

        return maxScale;
    }

    private Bounds GetBoundsForAllChildren(GameObject findMyBounds)
    {
        Bounds result = new Bounds(Vector3.zero, Vector3.zero);

        foreach (var curRenderer in findMyBounds.GetComponentsInChildren<Renderer>())
        {
            if (result.extents == Vector3.zero)
                result = curRenderer.bounds;
            else
                result.Encapsulate(curRenderer.bounds);
        }
        return result;
    }

    private void checkIfWorldCreated()
    {
        if (boxCreated && treeCreated)
        {
            SpatialUnderstandingState.Instance.SpaceQueryDescription = "";
            GameObject.Find("Spatial Understanding").GetComponent<SpatialUnderstandingState>().enabled = false;
        }
    }

    public int GetHandsNeededForManipulation(int objectID)
    {
        int hands;
        try
        {
            hands = HandsForActiveHolograms[objectID];
        }
        catch (KeyNotFoundException)
        {
            hands = 1;
        }
        return hands;
    }

    public void setActiveHologram(int objectID, int hands)
    {
        HandsForActiveHolograms.Add(objectID, hands);
    }

    public void SetFruitProps()
    {
        GameObject child;
        for (int i = 0; i < createdTree.transform.childCount; i++)
        {
            child = gameObject.transform.GetChild(i).gameObject;
            child.tag = "User";
            /*
            Interpolator interpolator = child.AddComponent<Interpolator>();
            interpolator.SmoothLerpToTarget = true;
            interpolator.SmoothPositionLerpRatio = 0.5f;
            interpolator.PositionPerSecond = 40.0f;
            */
            setActiveHologram(child.GetInstanceID(), 1);
        }
    }

    public void ClearScene()
    {
        foreach (GameObject i in ActiveHolograms)
        {
            Destroy(i);
        }
        ActiveHolograms.Clear();
        HandsForActiveHolograms.Clear();
    }

    public void appearBox(int counter, Vector3 initPos)
    {
        Vector3 handRef = initPos - Camera.main.transform.position;
        if (counter%2==0)
        {
            handRef.z = -handRef.z;
        }
        else
        {
            handRef.x = -handRef.x;
        }
        handRef.y = Camera.main.transform.position.y - 0.05f;
        Vector3 newPos = handRef + Camera.main.transform.position;
        createdBox.transform.position = newPos;
        createdBox.GetComponent<Renderer>().enabled = true;
    }

    public void disappearBox()
    {
        Renderer[] rend = createdBox.GetComponentsInChildren<Renderer>();
        foreach (Renderer i in rend)
        {
            i.enabled = false;
        }
    }

    public GameObject getCreatedBox()
    {
        return createdBox;
    }
}
