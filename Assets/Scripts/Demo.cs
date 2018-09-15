using UnityEngine;

public class Demo : MonoBehaviour
{
    public float FruitScale;
    private GameObject tree;
    private GameObject child;

    void Start()
    {
        tree = gameObject;
        for (int i =0;i<tree.transform.childCount; i++)
        {
            child = gameObject.transform.GetChild(i).gameObject;
            child.name = "Fruit";
            child.transform.localScale = new Vector3(FruitScale, FruitScale, FruitScale);
            ObjectCollectionManager.Instance.setActiveHologram(child.GetInstanceID(), 1);
        }
    }
}