using System;
using System.Collections.Generic;
using HoloToolkit.Unity;
using UnityEngine;
using Random = UnityEngine.Random;
//Almost done
public class ObjectPlacer : MonoBehaviour
{
    //Public Variables
    public int NumberOfFruits;
    public Material OccludedMaterial;
    public SpatialUnderstandingCustomMesh SpatialUnderstandingMesh;
    //Private Variables
    private readonly Queue<PlacementResult> _results = new Queue<PlacementResult>();
    private bool _timeToHideMesh;
    private bool treeCreated = false;
    private Vector3 treePosition;
    private Vector3 treeSize;
    private Vector3 fruitSize;

    // Use this for initialization
    void Start()
    {
        ObjectCollectionManager.Instance.TreeSize = ObjectCollectionManager.Instance.TreePrefab.GetComponent<Renderer>().bounds.size;
        treeSize = ObjectCollectionManager.Instance.TreeSize;
        ObjectCollectionManager.Instance.FruitSize = ObjectCollectionManager.Instance.FruitPrefab.GetComponent<Renderer>().bounds.size;
        fruitSize = ObjectCollectionManager.Instance.FruitSize;
    }

    void Update()
    {
        ProcessPlacementResults();
        if (_timeToHideMesh)
        {
            //SpatialUnderstandingState.Instance.HideText = true;
            HideGridEnableOcclulsion();
            _timeToHideMesh = false;
        }
    }

    private void HideGridEnableOcclulsion()
    {
        //SpatialUnderstandingMesh.DrawProcessedMesh = false;
        SpatialUnderstandingMesh.MeshMaterial = OccludedMaterial;
    }

    public void CreateScene()
    {
        //Simple check for Spatial Understanding
        if (!SpatialUnderstanding.Instance.AllowSpatialUnderstanding)
            return;

        SpatialUnderstandingDllObjectPlacement.Solver_Init();
        SpatialUnderstandingState.Instance.SpaceQueryDescription = "Generating World";

        List<PlacementQuery> queries = new List<PlacementQuery>();
        queries.AddRange(AddTree());
        //queries.AddRange(AddFruits());
        GetLocationsFromSolver(queries);
    }

    public List<PlacementQuery> AddTree()
    {
        return CreateLocationQueriesForSolver(1, ObjectCollectionManager.Instance.TreeSize, ObjectType.Tree);
    }

    public List<PlacementQuery> AddFruits()
    {
        return  CreateLocationQueriesForSolver(NumberOfFruits, ObjectCollectionManager.Instance.FruitSize, ObjectType.Fruit);
    }

    private void ProcessPlacementResults()
    {
        if (_results.Count > 0)
        {
            PlacementResult toPlace = _results.Dequeue();

            var rotation = Quaternion.LookRotation(toPlace.Normal, Vector3.up);
            switch (toPlace.ObjType)
            {
                case ObjectType.Tree:
                    ObjectCollectionManager.Instance.CreateTree(toPlace.Position, rotation);
                    treePosition = toPlace.Position;
                    treeCreated = true;
                    //Add the fruits
                    GetLocationsFromSolver(AddFruits());
                    break;
                case ObjectType.Fruit:
                    if (treeCreated)
                        ObjectCollectionManager.Instance.CreateFruit(toPlace.Position, rotation);
                    else
                        _results.Enqueue(toPlace);
                    break;
            }
        }
    }

    private void GetLocationsFromSolver(List<PlacementQuery> placementQueries)//2
    {
        //System.Threading.Tasks.Task.Run(() =>
        //{
            // Go through the queries in the list
            for (int i = 0; i < placementQueries.Count; ++i)
            {
                var result = PlaceObject(placementQueries[i].ObjType.ToString() + i,
                                         placementQueries[i].PlacementDefinition,
                                         placementQueries[i].Dimensions,
                                         placementQueries[i].ObjType,
                                         placementQueries[i].PlacementRules,
                                         placementQueries[i].PlacementConstraints);

                if (result != null) _results.Enqueue(result);
            }
            _timeToHideMesh = true;
        //});
    }

    private PlacementResult PlaceObject(string placementName,
        SpatialUnderstandingDllObjectPlacement.ObjectPlacementDefinition placementDefinition,
        Vector3 boxFullDims,
        ObjectType objType,
        List<SpatialUnderstandingDllObjectPlacement.ObjectPlacementRule> placementRules = null,
        List<SpatialUnderstandingDllObjectPlacement.ObjectPlacementConstraint> placementConstraints = null)//3
    {

        // New query
        if (SpatialUnderstandingDllObjectPlacement.Solver_PlaceObject(
                placementName,
                SpatialUnderstanding.Instance.UnderstandingDLL.PinObject(placementDefinition),
                (placementRules != null) ? placementRules.Count : 0,
                ((placementRules != null) && (placementRules.Count > 0)) ? SpatialUnderstanding.Instance.UnderstandingDLL.PinObject(placementRules.ToArray()) : IntPtr.Zero,
                (placementConstraints != null) ? placementConstraints.Count : 0,
                ((placementConstraints != null) && (placementConstraints.Count > 0)) ? SpatialUnderstanding.Instance.UnderstandingDLL.PinObject(placementConstraints.ToArray()) : IntPtr.Zero,
                SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticObjectPlacementResultPtr()) > 0)
        {
            SpatialUnderstandingDllObjectPlacement.ObjectPlacementResult placementResult = SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticObjectPlacementResult();

            return new PlacementResult(placementResult.Clone() as SpatialUnderstandingDllObjectPlacement.ObjectPlacementResult, boxFullDims, objType);
        }

        return null;
    }

    private List<PlacementQuery> CreateLocationQueriesForSolver(int prefabCount, Vector3 boxFullDims, ObjectType objType)
    {
        List<PlacementQuery> placementQueries = new List<PlacementQuery>();
        Vector3 halfBoxDims = boxFullDims * .5f;
        float disctanceFromOtherObjects = halfBoxDims.x > halfBoxDims.z ? halfBoxDims.x * 3f : halfBoxDims.z * 3f;
        
        for (int i = 0; i < prefabCount; i++)
        {
            var placementRules = new List<SpatialUnderstandingDllObjectPlacement.ObjectPlacementRule>
            {
                SpatialUnderstandingDllObjectPlacement.ObjectPlacementRule.Create_AwayFromOtherObjects(disctanceFromOtherObjects)
            };

            var placementConstraints = new List<SpatialUnderstandingDllObjectPlacement.ObjectPlacementConstraint>();
            SpatialUnderstandingDllObjectPlacement.ObjectPlacementDefinition placementDefinition;

            if (objType == ObjectType.Tree)
            {
                placementConstraints.Add(SpatialUnderstandingDllObjectPlacement.ObjectPlacementConstraint.Create_NearCenter());
                placementDefinition = SpatialUnderstandingDllObjectPlacement.ObjectPlacementDefinition.Create_OnFloor(halfBoxDims);
            }
            else if (objType == ObjectType.Fruit && treeCreated)
            {
                float x = Random.Range(treeSize.x / 2, treeSize.x);
                float y = Random.Range(treeSize.y / 1.5f, treeSize.y);
                float z = Random.Range(treeSize.z / 2, treeSize.z);

                Vector3 fruitPosition = new Vector3(treeSize.x + x + halfBoxDims.x,
                                                    y,
                                                    treeSize.z + z + halfBoxDims.z);
                fruitPosition = ObjectCollectionManager.Instance.TreePrefab.GetComponent<Renderer>().bounds.ClosestPoint(fruitPosition);
                placementConstraints.Add(SpatialUnderstandingDllObjectPlacement.ObjectPlacementConstraint.Create_NearCenter());
                placementConstraints.Add(SpatialUnderstandingDllObjectPlacement.ObjectPlacementConstraint.Create_NearPoint(fruitPosition, 0, .2f));
                placementDefinition = SpatialUnderstandingDllObjectPlacement.ObjectPlacementDefinition.Create_InMidAir(halfBoxDims);
            }
            else
            {
                placementDefinition = SpatialUnderstandingDllObjectPlacement.ObjectPlacementDefinition.Create_InMidAir(halfBoxDims);
            }

            placementQueries.Add(
                new PlacementQuery(placementDefinition,
                    boxFullDims,
                    objType,
                    placementRules,
                    placementConstraints
                ));
        }

        return placementQueries;
    }

}