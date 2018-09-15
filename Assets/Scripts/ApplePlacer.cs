using UnityEngine;

public class ApplePlacer : MonoBehaviour {

    public int NumberOfFruits;
    public GameObject FruitPrefab;
    public float fruitScale;


    void Start ()
    {
        for (float i = 0.0f; i < (float)NumberOfFruits; i=i+1.0f)
        {
            GameObject newFruit = Instantiate(FruitPrefab);
            newFruit.name = "Fruit_"+i ;
            if (newFruit != null)
            {
                newFruit.transform.parent = gameObject.transform;
                newFruit.transform.localScale = new Vector3(fruitScale, fruitScale, fruitScale);
                Vector3 treePos = gameObject.transform.position;
                Vector3 treeSize = gameObject.GetComponent<Renderer>().bounds.size/1.5f;
                float theta = 2.0f * Mathf.PI *( i /(float)NumberOfFruits +1) ;
                float y = Random.Range(treeSize.y/1.3f, treeSize.y);              
                float x = (Mathf.Cos(theta) * treeSize.x) + treePos.x;
                float z = (Mathf.Sin(theta) * treeSize.z) + treePos.z;
                Vector3 pos = new Vector3(x, y, z);
                pos = gameObject.GetComponent<CapsuleCollider>().bounds.ClosestPoint(pos);
                newFruit.transform.position = pos;

            }
        }
    }

}
