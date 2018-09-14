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
    public float ScaleFactor;

    public GameObject createdTree;

    private List<GameObject> ActiveHolograms = new List<GameObject>();
    private Dictionary<int, int> HandsForActiveHolograms = new Dictionary<int, int>();//key: id, value: hands


    public void CreateTree(Vector3 positionCenter, Quaternion rotation)
    {
        // Stay center in the square but move down to the ground
        var position = positionCenter - new Vector3(0, TreeSize.y * .5f, 0);
        GameObject newObject = Instantiate(TreePrefab, position, rotation);
        newObject.name = "Tree";
        if (newObject != null)
        {
            newObject.transform.parent = gameObject.transform;
            newObject.transform.localScale = RescaleToSameScaleFactor(TreePrefab);
            ActiveHolograms.Add(newObject);
            createdTree = newObject;
            HandsForActiveHolograms.Add(newObject.GetInstanceID(), -1);
        }
    }

    public void CreateFruit(Vector3 positionCenter, Quaternion rotation)
    {
        var position = positionCenter - new Vector3(0, FruitSize.y * .5f, 0);
        GameObject newObject = Instantiate(FruitPrefab, position, rotation);
        newObject.name = "Fruit";
        if (newObject != null)
        {
            newObject.transform.parent = gameObject.transform;
            newObject.transform.localScale = RescaleToSameScaleFactor(FruitPrefab);
            ActiveHolograms.Add(newObject);
            HandsForActiveHolograms.Add(newObject.GetInstanceID(), 1);
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
}
