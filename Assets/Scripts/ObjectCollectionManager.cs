using System.Collections.Generic;
using HoloToolkit.Unity;
using UnityEngine;

public class ObjectCollectionManager : Singleton<ObjectCollectionManager>
{
    //Public Variables - For Editor
    public GameObject TreePrefab;
    public GameObject FruitPrefab;
    public Vector3 TreeSize;
    public Vector3 FruitSize;
    public float ScaleFactor;

    private List<GameObject> ActiveHolograms = new List<GameObject>();
    private Dictionary<int, int> HandsForActiveHolograms = new Dictionary<int, int>();//key: id, value: hands

    public void CreateTree(Vector3 positionCenter, Quaternion rotation)
    {
        //TreeSize = TreePrefab.GetComponent<Renderer>().bounds.size;
        var position = positionCenter - new Vector3(0, TreeSize.y * .5f, 0);

        GameObject newObject = Instantiate(TreePrefab, position, rotation);

        if (newObject != null)
        {
            // Set the parent of the new object the GameObject it was placed on
            newObject.transform.parent = gameObject.transform;
            // Scale Tree
            //newObject.transform.localScale = RescaleToSameScaleFactor(TreePrefab);
            AddMeshColliderToAllChildren(newObject); // collisions
            ActiveHolograms.Add(newObject);
            HandsForActiveHolograms.Add(newObject.GetInstanceID(), 0);
        }
    }

    public void CreateFruit (Vector3 positionCenter, Quaternion rotation)
    {
        //FruitSize = FruitPrefab.GetComponent<Renderer>().bounds.size;
        var position = positionCenter - new Vector3(0, FruitSize.y * .5f, 0);
       
        GameObject newObject = Instantiate(FruitPrefab, position, rotation);

        if (newObject != null)
        {
            // Set the parent of the new object the GameObject it was placed on
            newObject.transform.parent = gameObject.transform;
            // Scale Tree
            //newObject.transform.localScale = RescaleToSameScaleFactor(TreePrefab);
            AddMeshColliderToAllChildren(newObject); // collisions
            ActiveHolograms.Add(newObject);
            HandsForActiveHolograms.Add(newObject.GetInstanceID(), 1);
        }
    }

    private void AddMeshColliderToAllChildren(GameObject obj)
    {
        for (int i = 0; i < obj.transform.childCount; i++)
            obj.transform.GetChild(i).gameObject.AddComponent<MeshCollider>();
    }

    private Vector3 RescaleToSameScaleFactor(GameObject objectToScale)
    {
        if (ScaleFactor == 0f)
            CalculateScaleFactor(TreePrefab, TreeSize);

        return objectToScale.transform.localScale * ScaleFactor;
    }

    private void CalculateScaleFactor(List<GameObject> prefabs, Vector3 prefabSize)
    {
        float maxScale = float.MaxValue;
        var ratio = CalcScaleFactorHelper(prefabs, prefabSize);
        if (ratio < maxScale)
            maxScale = ratio;

        ScaleFactor = maxScale;
    }
    
    private void CalculateScaleFactor(GameObject prefab, Vector3 prefabSize)
    {
        List<GameObject> list = new List<GameObject> { prefab };
        CalculateScaleFactor(list, prefabSize);
    }

    private float CalcScaleFactorHelper(List<GameObject> objects, Vector3 desiredSize)
    {
        float maxScale = float.MaxValue;

        foreach (var obj in objects)
        {
            var curBounds = GetBoundsForAllChildren(obj).size;
            var difference = curBounds - desiredSize;
            float ratio;

            if (difference.x > difference.y && difference.x > difference.z)
                ratio = desiredSize.x / curBounds.x;
            else if (difference.y > difference.x && difference.y > difference.z)
                ratio = desiredSize.y / curBounds.y;
            else
                ratio = desiredSize.z / curBounds.z;

            if (ratio < maxScale)
                maxScale = ratio;
        }

        return maxScale;
    }

    private Bounds GetBoundsForAllChildren(GameObject findMyBounds) //get bounds for object children
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
