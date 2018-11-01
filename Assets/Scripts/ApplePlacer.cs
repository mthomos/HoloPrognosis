using UnityEngine;

public class ApplePlacer : MonoBehaviour {

    public int NumberOfFruits;
    public GameObject FruitPrefab;
    public float fruitScale;


    void Start()
    {
        for (float i = 0.0f; i < (float)NumberOfFruits; i = i + 1.0f)
        {
            GameObject newFruit = Instantiate(FruitPrefab);
            newFruit.name = "Fruit_" + i;
            if (newFruit != null)
            {
                newFruit.transform.parent = gameObject.transform;
                newFruit.transform.localScale = new Vector3(fruitScale, fruitScale, fruitScale);
                Vector3 treePos = gameObject.transform.position;
                Vector3 treeSize = gameObject.GetComponent<Renderer>().bounds.size / 1.5f;
                float theta = 2.0f * Mathf.PI * (i / (float)NumberOfFruits + 1);
                float y = Random.Range(treeSize.y / 1.3f, treeSize.y);
                float x = (Mathf.Cos(theta) * treeSize.x) + treePos.x;
                float z = (Mathf.Sin(theta) * treeSize.z) + treePos.z;
                Vector3 pos = new Vector3(x, y, z);
                pos = gameObject.GetComponent<Renderer>().bounds.ClosestPoint(pos);
                pos = Vector3.Lerp(new Vector3(0, pos.y, 0), pos, .7f);
                newFruit.transform.position = pos;
                //Collider treeColider = gameObject.GetComponent<MeshCollider>();
                //pos = treeColider.ClosestPoint(pos);
                pos = calcMinDistPos(gameObject.GetComponent<MeshFilter>(), newFruit);
                newFruit.transform.position = pos;

            }
        }
    }

    public Vector3 calcMinDistPos(MeshFilter filter, GameObject obj)
    {
        Vector3 pointCenter = obj.GetComponent<BoxCollider>().center;
        Vector3 size = obj.GetComponent<BoxCollider>().size;
        Vector3[]points = new Vector3[8];
        points[0] = new Vector3(pointCenter.x + size.x, pointCenter.y + size.y, pointCenter.z + size.z);
        points[1] = new Vector3(pointCenter.x + size.x, pointCenter.y + size.y, pointCenter.z - size.z);
        points[2] = new Vector3(pointCenter.x + size.x, pointCenter.y - size.y, pointCenter.z + size.z);
        points[3] = new Vector3(pointCenter.x + size.x, pointCenter.y - size.y, pointCenter.z - size.z);
        points[4] = new Vector3(pointCenter.x - size.x, pointCenter.y + size.y, pointCenter.z + size.z);
        points[5] = new Vector3(pointCenter.x - size.x, pointCenter.y - size.y, pointCenter.z + size.z);
        points[6] = new Vector3(pointCenter.x - size.x, pointCenter.y + size.y, pointCenter.z - size.z);
        points[7] = new Vector3(pointCenter.x - size.x, pointCenter.y - size.y, pointCenter.z - size.z);
        Vector3 pos = Vector3.zero;
        Vector3[] closestTrig = new Vector3[3];
        float distance = 100000.0f;
        int corner = 0; ;
        int[] triangles = filter.sharedMesh.triangles;
        Vector3[] vertices = filter.sharedMesh.vertices;
        var length = (int)(triangles.Length / 3);
        for (var t = 0; t < length; t++)
        {
            for (var i = 0; i < 8; i++)
            {
                Vector3 p1 = vertices[triangles[0 + t * 3]];
                Vector3 p2 = vertices[triangles[1 + t * 3]];
                Vector3 p3 = vertices[triangles[2 + t * 3]];
                float tempDistance = pointDistanceInTrig(points[i], p1, p2, p3);
                if (tempDistance < distance)
                {
                    distance = tempDistance;
                    closestTrig[0] = p1;
                    closestTrig[1] = p2;
                    closestTrig[2] = p3;
                    corner = i;
                }
            }
        }
        var trig = Vector3.Cross((closestTrig[1] - closestTrig[0]).normalized, (closestTrig[2] - closestTrig[0]).normalized);
        pos = points[corner] + Vector3.Dot((closestTrig[0] - points[corner]), trig) * trig;
        return pos;
    }

    public float pointDistanceInTrig(Vector3 point , Vector3 t1, Vector3 t2, Vector3 t3)
    {
       return Vector3.Magnitude(point -t1) +
                Vector3.Magnitude(point -t2) +
                Vector3.Magnitude(point -t3);
    }
}
