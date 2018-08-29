using System;
using System.Collections.Generic;
using HoloToolkit.Unity;
using UnityEngine;
//Almost done
public class ObjectPlacer : MonoBehaviour
{
    //Public Variables
    public bool DrawTree = true;
    public int NumberOfFruits;
    public Material OccludedMaterial;
    public SpatialUnderstandingCustomMesh SpatialUnderstandingMesh;
    //Private Variables
    private readonly Queue<PlacementResult> _results = new Queue<PlacementResult>();
    private bool _timeToHideMesh;
    private int _placedTree;
    //Box functionality
    private bool DrawDebugBoxes = false;
    private BoxDrawer _boxDrawing;
    private readonly List<BoxDrawer.Box> _lineBoxList = new List<BoxDrawer.Box>();

    // Use this for initialization
    void Start()
    {
        if (DrawDebugBoxes)
            _boxDrawing = new BoxDrawer(gameObject);
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
        if (DrawDebugBoxes)
            _boxDrawing.UpdateBoxes(_lineBoxList);
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
        queries.AddRange(AddFruits());
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
            var toPlace = _results.Dequeue();
            // Output
            if (DrawDebugBoxes) DrawBox(toPlace, Color.red);

            var rotation = Quaternion.LookRotation(toPlace.Normal, Vector3.up);
            switch (toPlace.ObjType)
            {
                case ObjectType.Tree:
                    ObjectCollectionManager.Instance.CreateTree(toPlace.Position, rotation);
                    break;
                case ObjectType.Fruit:
                    ObjectCollectionManager.Instance.CreateFruit(toPlace.Position, rotation);
                    break;
            }
        }
    }

    private void DrawBox(PlacementResult boxLocation, Color color)
    {
        if (boxLocation != null)
        {
            _lineBoxList.Add(
                new BoxDrawer.Box(
                    boxLocation.Position,
                    Quaternion.LookRotation(boxLocation.Normal, Vector3.up),
                    color,
                    boxLocation.Dimensions * 0.5f)
            );
        }
    }

    private void GetLocationsFromSolver(List<PlacementQuery> placementQueries)//2
    {
        System.Threading.Tasks.Task.Run(() =>
        {
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
        });
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

        var halfBoxDims = boxFullDims * .5f;

        var disctanceFromOtherObjects = halfBoxDims.x > halfBoxDims.z ? halfBoxDims.x * 3f : halfBoxDims.z * 3f;

        for (int i = 0; i < prefabCount; ++i)
        {
            var placementRules = new List<SpatialUnderstandingDllObjectPlacement.ObjectPlacementRule>
            {
                SpatialUnderstandingDllObjectPlacement.ObjectPlacementRule.Create_AwayFromOtherObjects(disctanceFromOtherObjects)
            };

            var placementConstraints = new List<SpatialUnderstandingDllObjectPlacement.ObjectPlacementConstraint>();

            SpatialUnderstandingDllObjectPlacement.ObjectPlacementDefinition placementDefinition = SpatialUnderstandingDllObjectPlacement.ObjectPlacementDefinition.Create_OnFloor(halfBoxDims);

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