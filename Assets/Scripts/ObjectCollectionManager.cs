using System.Collections.Generic;
using HoloToolkit.Unity;
using UnityEngine;

public class ObjectCollectionManager : Singleton<ObjectCollectionManager>
{
    //Public Variables - For Editor
    public GameObject TreePrefab;
    //public GameObject BoxPrefab;
    public GameObject GatePrefab;
    public Vector3 TreeSize;
    //public Vector3 BoxSize;
    public Vector3 GateSize;
    // Private Variables
    public FlowController flowController;
    private GameObject createdTree;
    //private GameObject createdBox;
    private GameObject createdGate;
    private float ScaleFactor;
    private List<GameObject> ActiveHolograms = new List<GameObject>();
    private bool boxCreated, treeCreated;
    private float angle = 90;

    public void CreateTree(Vector3 positionCenter, Quaternion rotation)
    {
        // Stay center in the square but move down to the ground
        Vector3 position = positionCenter - new Vector3(0, TreeSize.y * .5f, 0);
        GameObject newObject = Instantiate(TreePrefab, position, rotation);
        newObject.name = "Tree";
        if (newObject != null)
        {
            newObject.transform.parent = gameObject.transform;
            newObject.tag = "Dummy";
            newObject.transform.localScale = RescaleToSameScaleFactor(TreePrefab);
            ActiveHolograms.Add(newObject);
            createdTree = newObject;
            SetProps();
            treeCreated = true;
        }
    }
    /*
    public void CreateBox(Vector3 positionCenter, Quaternion rotation)
    {
        // Stay center in the square but move down to the ground
        var position = positionCenter + new Vector3(0, BoxSize.y * .25f, 0);
        GameObject newObject = Instantiate(BoxPrefab, position, rotation);
        newObject.name = "Box";
        if (newObject != null)
        {
            newObject.transform.parent = gameObject.transform;
            newObject.tag = "Dummy";
            newObject.transform.localScale = RescaleToSameScaleFactor(BoxPrefab);
            ActiveHolograms.Add(newObject);
            createdBox = newObject;
            boxCreated = true;
            if (treeCreated && boxCreated)
                flowController.PrepareNextManipulation();
        }
    }
    */
    public void CreateGate(Vector3 positionCenter, Quaternion rotation)
    {
        // Stay center in the square but move down to the ground
        var position = positionCenter + new Vector3(0, GateSize.y * .25f, 0);
        GameObject newObject = Instantiate(GatePrefab, position, rotation);
        newObject.name = "Gate";
        if (newObject != null)
        {
            newObject.transform.parent = gameObject.transform;
            newObject.tag = "Dummy";
            newObject.transform.localScale = RescaleToSameScaleFactor(GatePrefab);
            ActiveHolograms.Add(newObject);
            createdGate = newObject;
            boxCreated = true;
            createdGate.AddComponent<GateScript>().gateOpened = false;
            if (treeCreated && boxCreated)
                flowController.PrepareNextManipulation();
        }
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

    public void SetProps()
    {
        GameObject child;
        for (int i = 0; i < createdTree.transform.childCount; i++)
        {
            child = createdTree.transform.GetChild(i).gameObject;
            child.name = "apple_" + i;
            child.AddComponent<AppleScript>();
            ActiveHolograms.Add(child);
        }
        treeCreated = true;
        if (treeCreated && boxCreated)
            flowController.PrepareNextManipulation();
    }
    
    public GameObject getLowestFruit(float threshold)
    {
        GameObject lowestObject = null;
        float dist = 10000;
        if (createdTree == null)
            return null;
        for (int i = 0; i < createdTree.transform.childCount; i++)
        {
            if (createdTree.transform.GetChild(i).gameObject.transform.position.y > threshold &&
                    createdTree.transform.GetChild(i).gameObject.transform.position.y <= dist)
                lowestObject = createdTree.transform.GetChild(i).gameObject;
        }
        return lowestObject;
    }

    public void ClearScene()
    {
        foreach (GameObject i in ActiveHolograms)
            Destroy(i);
        ActiveHolograms.Clear();
    }

    public void appearGate(CalibrationController controller)
    {

        float height = controller.getRightPoseHandHeight();
        float distance = controller.getRightPoseHeadHandDistance();
        Vector3 position;
        position.y = height;
        if( controller.isRightHand() )
        {
            position.x = Camera.main.transform.position.x + Mathf.Cos(angle)*distance;
            position.z = Camera.main.transform.position.z + Mathf.Sin(angle)*distance;
        }
        else
        {
            position.x = Camera.main.transform.position.x + Mathf.Cos(-1*angle)*distance;
            position.z = Camera.main.transform.position.z + Mathf.Sin(-1*angle)*distance;
        }

        createdGate.SetActive(true);
        Vector3 difPos = position - createdGate.transform.position;
        createdGate.transform.position = position;
        createdGate.transform.rotation.Set(0, 90, 0, 1);
        createdGate.GetComponent<GateScript>().gateOpened = true;
        //For child objects ??????
    }
    /*
    public void disappearBox()
    {
        for (int a = 0; a < createdBox.transform.childCount; a++)
            createdBox.transform.GetChild(a).gameObject.SetActive(false);
        createdBox.SetActive(false);
    }

    public GameObject getCreatedBox()
    {
        return createdBox;
    }
    */
    public void disappearGate()
    {
        if (createdGate == null)
            return;

        createdGate.GetComponent<GateScript>().gateOpened = false;
        for (int a = 0; a < createdGate.transform.childCount; a++)
            createdGate.transform.GetChild(a).gameObject.SetActive(false);
        createdGate.SetActive(false);
    }

    public GameObject getCreatedGate()
    {
        return createdGate;
    }

    public void destoryActiveHologram(string name)
    {
        foreach (GameObject hologram in ActiveHolograms)
        {
            if(hologram.name == name)
            {
                ActiveHolograms.Remove(hologram);
                break;
            }
        }
    }
}
