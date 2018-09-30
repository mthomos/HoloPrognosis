using System.Collections.Generic;
using HoloToolkit.Unity;
using UnityEngine;

public class ObjectCollectionManager : Singleton<ObjectCollectionManager>
{
    //Public Variables - For Editor
    public GameObject TreePrefab;
    public GameObject FruitPrefab;
    public GameObject BoxPrefab;
    public Vector3 TreeSize;
    public Vector3 FruitSize;
    public Vector3 BoxSize;
    public float FruitScale;
    public int NumberOfFruits;
    //Demo variables
    public bool Demo;
    public GameObject TreeDemoPrefab;

    private GameObject createdTree;
    private float ScaleFactor;
    private List<GameObject> ActiveHolograms = new List<GameObject>();
    private Dictionary<int, int> HandsForActiveHolograms = new Dictionary<int, int>();//key: id, value: hands
    private bool treeCreated, boxCreated;

    public void CreateTree(Vector3 positionCenter, Quaternion rotation)
    {
        GameObject prefab;
        if (Demo) prefab = TreeDemoPrefab;
        else prefab = TreePrefab;
        // Stay center in the square but move down to the ground
        Vector3 position = positionCenter - new Vector3(0, TreeSize.y * .5f, 0);
        GameObject newObject = Instantiate(prefab, position, rotation);
        newObject.name = "Tree";
        if (newObject != null)
        {
            newObject.transform.parent = gameObject.transform;
            newObject.transform.localScale = RescaleToSameScaleFactor(TreePrefab);
            ActiveHolograms.Add(newObject);
            createdTree = newObject;
            HandsForActiveHolograms.Add(newObject.GetInstanceID(), -1);
            treeCreated = true;
            checkIfWordCreated();
        }
        if (Demo)
            DemoFun();
        else
            CreateFruits();
    }

    public void CreateFruits()
    {
        for (float i = 0.0f; i < (float)NumberOfFruits; i = i + 1.0f)
        {
            GameObject newFruit = Instantiate(FruitPrefab);
            newFruit.name = "Fruit_" + i;
            if (newFruit != null)
            {
                newFruit.transform.parent = createdTree.transform;
                newFruit.transform.localScale = new Vector3(FruitScale, FruitScale, FruitScale);
                Vector3 treePos = createdTree.transform.position;
                Vector3 treeSize = createdTree.GetComponent<Renderer>().bounds.size / 1.5f;
                float theta = 2.0f * Mathf.PI * (i / (float)NumberOfFruits + 1);
                float y = Random.Range(treeSize.y / 1.3f, treeSize.y);
                float x = (Mathf.Cos(theta) * treeSize.x) + treePos.x;
                float z = (Mathf.Sin(theta) * treeSize.z) + treePos.z;
                Vector3 pos = new Vector3(x, y, z);
                pos = createdTree.GetComponent<CapsuleCollider>().bounds.ClosestPoint(pos);
                newFruit.transform.position = pos;
                if (createdTree.GetComponent<Renderer>().bounds.Contains(pos))
                    Debug.Log(newFruit.name + "Inside");
                HandsForActiveHolograms.Add(newFruit.GetInstanceID(), 1);
            }
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
            SpatialUnderstandingState.Instance.SpaceQueryDescription = "";
            boxCreated = true;
            checkIfWordCreated();
        }
    }

    private void AddMeshColliderToAllChildren(GameObject obj)
    {
        for (int i = 0; i < obj.transform.childCount; i++)
            obj.transform.GetChild(i).gameObject.AddComponent<MeshCollider>();
    }

    private Vector3 RescaleToSameScaleFactor(GameObject objectToScale)
    {
        if (ScaleFactor == 0f)  CalculateScaleFactor();
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

    private void checkIfWordCreated()
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

    public void DemoFun()
    {
        GameObject child;
        for (int i = 0; i < createdTree.transform.childCount; i++)
        {
            child = gameObject.transform.GetChild(i).gameObject;
            child.name = "Fruit";
            child.GetComponent<Interpolator>();
            Interpolator interpolator = child.AddComponent<Interpolator>();
            interpolator.SmoothLerpToTarget = true;
            interpolator.SmoothPositionLerpRatio = 0.5f;
            interpolator.PositionPerSecond = 40.0f;
            //child.transform.localScale = new Vector3(FruitScale, FruitScale, FruitScale);
            ObjectCollectionManager.Instance.setActiveHologram(child.GetInstanceID(), 1);
        }
    }
}
