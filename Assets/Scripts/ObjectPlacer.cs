using System.Collections.Generic;
using HoloToolkit.Unity;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

public class ObjectPlacer : MonoBehaviour
{
    public int NumberOfFruits;
    public SpatialUnderstandingCustomMesh SpatialUnderstandingMesh;
    public Material OccludedMaterial;

    private Queue<PlacementResult> _results = new Queue<PlacementResult>();
    private bool treeCreated = false;
    private bool fruitsQueriesCreated = false;
    private int fruitsCreated = 0;
    private GameObject tree;

    void Start()
    {

    }

    void Update()
    {
        if(treeCreated && !fruitsQueriesCreated)
        {
            List<PlacementQuery> queries = new List<PlacementQuery>();
            queries.AddRange(AddFruits());
            GetLocationsFromSolver(queries);
            SpatialUnderstandingState.Instance.SpaceQueryDescription = "Fruits queries";
            fruitsQueriesCreated = true;
        }
        ProcessPlacementResults();

    }

    private void HideGridEnableOcclulsion()
    {
        SpatialUnderstandingMesh.MeshMaterial = OccludedMaterial;
    }

    public void CreateScene()
    {
        if (!SpatialUnderstanding.Instance.AllowSpatialUnderstanding)
            return;

        SpatialUnderstandingDllObjectPlacement.Solver_Init();

        SpatialUnderstandingState.Instance.SpaceQueryDescription = "Generating World";
        HideGridEnableOcclulsion();

        List<PlacementQuery> queries = new List<PlacementQuery>();
        queries.AddRange(AddTree());
        queries.AddRange(AddBox());
        //Don't create fruits yet ,first place
        GetLocationsFromSolver(queries);
    }

    public List<PlacementQuery> AddTree()
    {
        SpatialUnderstandingState.Instance.SpaceQueryDescription = "Creating Tree";
        return CreateLocationQueriesForSolver(1, ObjectCollectionManager.Instance.TreeSize, ObjectType.Tree);
    }

    public List<PlacementQuery> AddFruits()
    {
        return CreateLocationQueriesForSolver(NumberOfFruits, ObjectCollectionManager.Instance.FruitSize, ObjectType.Fruit);
    }

    public List<PlacementQuery> AddBox()
    {
        SpatialUnderstandingState.Instance.SpaceQueryDescription = "Creating Box";
        return CreateLocationQueriesForSolver(1, ObjectCollectionManager.Instance.BoxSize, ObjectType.Box);
    }

    private void ProcessPlacementResults()
    {
        if (_results.Count > 0)
        {
            var toPlace = _results.Dequeue();
            Quaternion rotation = Quaternion.LookRotation(toPlace.Normal, Vector3.up);

            switch (toPlace.ObjType)
            {
                case ObjectType.Tree:
                    ObjectCollectionManager.Instance.CreateTree(toPlace.Position, rotation);
                    SpatialUnderstandingState.Instance.SpaceQueryDescription = "Tree Created";
                    tree = ObjectCollectionManager.Instance.createdTree;
                    treeCreated = true;
                    break;
                case ObjectType.Fruit:
                    ObjectCollectionManager.Instance.CreateFruit(toPlace.Position, rotation);
                    fruitsCreated++;
                    SpatialUnderstandingState.Instance.SpaceQueryDescription = "Creating Fruits " + fruitsCreated + "/" + NumberOfFruits;
                    if (fruitsCreated == NumberOfFruits)
                        SpatialUnderstandingState.Instance.SpaceQueryDescription = " ";
                    break;
                case ObjectType.Box:
                    ObjectCollectionManager.Instance.CreateBox(toPlace.Position, rotation);
                    SpatialUnderstandingState.Instance.SpaceQueryDescription = "Box Created";
                    break;
            }
        }
    }

    private void GetLocationsFromSolver(List<PlacementQuery> placementQueries)
    {
        for (int i = 0; i < placementQueries.Count; ++i)
        {
            var result = PlaceObject(placementQueries[i].ObjType.ToString() + i,
                                        placementQueries[i].PlacementDefinition,
                                        placementQueries[i].Dimensions,
                                        placementQueries[i].ObjType,
                                        placementQueries[i].PlacementRules,
                                        placementQueries[i].PlacementConstraints);
            if(placementQueries[i].ObjType == ObjectType.Fruit)
                SpatialUnderstandingState.Instance.SpaceQueryDescription = "Queries Fruits " + i + "/" + NumberOfFruits;
            if (result != null) _results.Enqueue(result);
            else SpatialUnderstandingState.Instance.SpaceQueryDescription += "is null";
        }
    }

    private PlacementResult PlaceObject(string placementName,
        SpatialUnderstandingDllObjectPlacement.ObjectPlacementDefinition placementDefinition,
        Vector3 boxFullDims,
        ObjectType objType,
        List<SpatialUnderstandingDllObjectPlacement.ObjectPlacementRule> placementRules,
        List<SpatialUnderstandingDllObjectPlacement.ObjectPlacementConstraint> placementConstraints)
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
        float distanceFromOtherObjects = halfBoxDims.x > halfBoxDims.z ? halfBoxDims.x * 1f : halfBoxDims.z * 1f;

        for (int i = 0; i < prefabCount; i++)
        {
            List<SpatialUnderstandingDllObjectPlacement.ObjectPlacementRule> placementRules;

            var placementConstraints = new List<SpatialUnderstandingDllObjectPlacement.ObjectPlacementConstraint>();
            SpatialUnderstandingDllObjectPlacement.ObjectPlacementDefinition placementDefinition;

            if (objType == ObjectType.Tree)
            {
                placementRules = new List<SpatialUnderstandingDllObjectPlacement.ObjectPlacementRule>{};
                placementConstraints.Add(SpatialUnderstandingDllObjectPlacement.ObjectPlacementConstraint.Create_NearCenter());
                placementDefinition = SpatialUnderstandingDllObjectPlacement.ObjectPlacementDefinition.Create_OnFloor(halfBoxDims);
            }
            else if (objType == ObjectType.Fruit && treeCreated)
            {
                placementRules = new List<SpatialUnderstandingDllObjectPlacement.ObjectPlacementRule>
                {
                SpatialUnderstandingDllObjectPlacement.ObjectPlacementRule.Create_AwayFromOtherObjects(distanceFromOtherObjects)
                };

                float x = Random.Range(-2f, 2f);
                float y = Random.Range(-.5f, 1f);
                float z = Random.Range(-2f, 2f);

                Vector3 fruitPosition = new Vector3(tree.transform.position.x + x + halfBoxDims.x,
                                                    tree.transform.position.y + y,
                                                    tree.transform.position.z + z + halfBoxDims.z);
                fruitPosition = ObjectCollectionManager.Instance.TreePrefab.GetComponent<Renderer>().bounds.ClosestPoint(fruitPosition);
                placementConstraints.Add(SpatialUnderstandingDllObjectPlacement.ObjectPlacementConstraint.Create_NearCenter());
                placementConstraints.Add(SpatialUnderstandingDllObjectPlacement.ObjectPlacementConstraint.Create_NearPoint(fruitPosition, .01f, .15f));
                placementDefinition = SpatialUnderstandingDllObjectPlacement.ObjectPlacementDefinition.Create_InMidAir(halfBoxDims);
            }
            else if (objType == ObjectType.Box)
            {
                placementRules = new List<SpatialUnderstandingDllObjectPlacement.ObjectPlacementRule>
                {
                SpatialUnderstandingDllObjectPlacement.ObjectPlacementRule.Create_AwayFromOtherObjects(0.1f)
                };

                placementConstraints.Add(SpatialUnderstandingDllObjectPlacement.ObjectPlacementConstraint.Create_NearWall());
                placementDefinition = SpatialUnderstandingDllObjectPlacement.ObjectPlacementDefinition.Create_InMidAir(halfBoxDims);
            }
            else
            {
                placementDefinition = SpatialUnderstandingDllObjectPlacement.ObjectPlacementDefinition.Create_InMidAir(halfBoxDims);
                placementRules = new List<SpatialUnderstandingDllObjectPlacement.ObjectPlacementRule>{};
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