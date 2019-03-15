using System.Collections.Generic;
using HoloToolkit.Unity;
using UnityEngine;

public class ObjectCollectionManager : Singleton<ObjectCollectionManager>
{
    //Public Variables - For Editor
    public GameObject TreePrefab;
    public GameObject GatePrefab;
    public GameObject TurtorialPrefab;
    public Vector3 TreeSize;
    public Vector3 GateSize;
    public Vector3 TurtorialMenuSize;
    // Private Variables
    public FlowController flowController;
    private GameObject createdTree;
    private GameObject createdGate;
    private float ScaleFactor;
    private List<GameObject> ActiveHolograms = new List<GameObject>();
    private bool gateCreated, treeCreated;

    public void CreateTree(Vector3 positionCenter, Quaternion rotation)
    {
        Vector3 position = positionCenter - new Vector3(0, TreeSize.y * .5f, 0);
        GameObject newObject = Instantiate(TreePrefab, position, rotation);
        if (newObject != null)
        {
            newObject.name = "Tree";
            newObject.transform.parent = gameObject.transform;
            newObject.tag = "Dummy";
            newObject.transform.localScale = RescaleToSameScaleFactor(TreePrefab);
            ActiveHolograms.Add(newObject);
            createdTree = newObject;
            SetProps();
            treeCreated = true;
            if (treeCreated && gateCreated)
                EventManager.TriggerEvent("world_created");
        }
    }

    public void CreateGate(Vector3 positionCenter, Quaternion rotation)
    {
        var position = positionCenter + new Vector3(0, GateSize.y * .25f, 0);
        position.y = 1.3f;
        GameObject newObject = Instantiate(GatePrefab, position, rotation);
        if (newObject != null)
        {
            newObject.name = "Gate";
            newObject.transform.parent = gameObject.transform;
            newObject.tag = "Dummy";
            newObject.transform.localScale = RescaleToSameScaleFactor(GatePrefab);
            ActiveHolograms.Add(newObject);
            createdGate = newObject;
            createdGate.GetComponent<MeshCollider>().enabled = false;
            createdGate.transform.Rotate(0, 0, -90f);
            gateCreated = true;
            if (treeCreated && gateCreated)
                EventManager.TriggerEvent("world_created");
        }
    }

    public void CreateTurtorialMenu(Vector3 positionCenter, Quaternion rotation)
    {
        GameObject newObject = Instantiate(TurtorialPrefab, positionCenter, rotation);
        if (newObject != null)
        {
            newObject.name = "TurtorialMenu";
            newObject.tag = "UI";
            UiController uiController = FindObjectOfType(typeof(UiController)) as UiController;
            uiController.SetTurtorialMenu(newObject);
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
            child.GetComponent<SphereCollider>().enabled = false;
            child.AddComponent<Outline>();
            child.GetComponent<Outline>().enabled = false;
            ActiveHolograms.Add(child);
        }
    }
    
    public GameObject GetLowestFruit(float threshold)
    {
        GameObject lowestObject = null;
        float dist = 100000;
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

    public void AppearGate(float height, float distance, bool toRight)
    {
        if (createdGate == null)
        {
            Debug.Log("Gate was null");
            return;
        }

        Vector3 position = new Vector3(0, height, 0);
        Vector3 angles = Camera.main.transform.eulerAngles;
        Vector3 cameraPosition = Camera.main.transform.position;
        float d_angle = toRight ? 90f : -90f;
        position.x = cameraPosition.x + Mathf.Sin((angles.y + d_angle) * Mathf.Deg2Rad) * distance;
        position.z = cameraPosition.z + Mathf.Cos((angles.y + d_angle) * Mathf.Deg2Rad) * distance;

        createdGate.SetActive(true);
        createdGate.transform.position = position;
        createdGate.transform.position = new Vector3(createdGate.transform.position.x,  0.7f * height, createdGate.transform.position.z);
        UtilitiesScript.Instance.DisableOutline(createdGate);
        createdGate.GetComponent<GateScript>().gateOpened = true;
        createdGate.GetComponent<MeshCollider>().enabled = false;
    }

    public void DisappearGate()
    {
        if (createdGate == null)
            return;

        createdGate.GetComponent<GateScript>().gateOpened = false;
        UtilitiesScript.Instance.DisableOutline(createdGate);
        createdGate.SetActive(false);
    }

    public void AppearGate()
    {
        if (createdGate == null)
            return;

        createdGate.SetActive(true);
        UtilitiesScript.Instance.DisableOutline(createdGate);
    }

    public void DisAppearTree()
    {
        if (createdTree == null)
            return;

        for (int a = 0; a < createdTree.transform.childCount; a++)
            createdTree.transform.GetChild(a).gameObject.SetActive(false);
        createdTree.SetActive(false);
    }

    public void AppearTree()
    {
        if (createdTree == null)
            return;

        createdTree.SetActive(true);
        for (int a = 0; a < createdTree.transform.childCount; a++)
        {
            createdTree.transform.GetChild(a).gameObject.SetActive(true);
            UtilitiesScript.Instance.DisableOutline(createdTree.transform.GetChild(a).gameObject);
        }
    }

    public GameObject GetCreatedGate()
    {
        return createdGate;
    }

    public void DestoryActiveHologram(string name)
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
