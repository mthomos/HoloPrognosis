using System.Collections.Generic;
using HoloToolkit.Unity;
using UnityEngine;
using System;

public class ObjectPlacer : MonoBehaviour
{
    private Queue<PlacementResult> results = new Queue<PlacementResult>();

    void Start()
    {

    }

    void Update()
    {
        ProcessPlacementResults();
    }

    public void CreateScene()
    {
         if (!SpatialUnderstanding.Instance.AllowSpatialUnderstanding)
            return;

        SpatialUnderstandingDllObjectPlacement.Solver_Init();

        List<PlacementQuery> queries = new List<PlacementQuery>();
        queries.AddRange(AddTree());
        //queries.AddRange(AddGate());
        Vector3 cameraPos = Camera.main.transform.position;
        Vector3 angles = Camera.main.transform.eulerAngles;
        Vector3 pos = new Vector3 (cameraPos.x + Mathf.Sin((angles.y) * Mathf.Deg2Rad) * 1.5f, 
                                   cameraPos.y , 
                                   cameraPos.z + Mathf.Cos((angles.y) * Mathf.Deg2Rad) * 1.5f);
        Quaternion rot = Quaternion.LookRotation(Camera.main.transform.forward, Vector3.up);
        ObjectCollectionManager.Instance.CreateGate(pos, rot);
        GetLocationsFromSolver(queries);
    }

    public void CreateGate()
    {
        List<PlacementQuery> queries = new List<PlacementQuery>();
        queries.AddRange(AddGate());
        GetLocationsFromSolver(queries);
    }

    public void CreateTree()
    {
        if (!SpatialUnderstanding.Instance.AllowSpatialUnderstanding)
            return;
        SpatialUnderstandingDllObjectPlacement.Solver_Init();

        List<PlacementQuery> queries = new List<PlacementQuery>();
        queries.AddRange(AddTree());
        GetLocationsFromSolver(queries);
    }

    public void CreateTurtorialMenu()
    {
        GetLocationsFromSolver(AddTree());
    }

    public List<PlacementQuery> AddTree()
    {
        return CreateLocationQueriesForSolver(1, ObjectCollectionManager.Instance.TreeSize, ObjectType.Tree);
    }

    public List<PlacementQuery> AddGate()
    {
        return CreateLocationQueriesForSolver(1, ObjectCollectionManager.Instance.GateSize, ObjectType.Gate);
    }

    public List<PlacementQuery> AddTurtorialMenu()
    {
        return CreateLocationQueriesForSolver(1, ObjectCollectionManager.Instance.TurtorialMenuSize, ObjectType.TurtorialMenu);
    }


    private void ProcessPlacementResults()
    {
        if (results.Count > 0)
        {
            var toPlace = results.Dequeue();
            Quaternion rotation = Quaternion.LookRotation(toPlace.Normal, Vector3.up);

            switch (toPlace.ObjType)
            {
                case ObjectType.Tree:
                    ObjectCollectionManager.Instance.CreateTree(toPlace.Position, rotation);
                    break;
                case ObjectType.Gate:
                    ObjectCollectionManager.Instance.CreateGate(toPlace.Position, rotation);
                    break;
                case ObjectType.TurtorialMenu:
                    ObjectCollectionManager.Instance.CreateTurtorialMenu(toPlace.Position, rotation);
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
            if (result != null) results.Enqueue(result);
        }
    }

    private PlacementResult PlaceObject(string placementName,
        SpatialUnderstandingDllObjectPlacement.ObjectPlacementDefinition placementDefinition,
        Vector3 boxFullDims,
        ObjectType objType,
        List<SpatialUnderstandingDllObjectPlacement.ObjectPlacementRule> placementRules = null,
        List<SpatialUnderstandingDllObjectPlacement.ObjectPlacementConstraint> placementConstraints = null)
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

    private List<PlacementQuery> CreateLocationQueriesForSolver(int prefabCount, Vector3 fullDims, ObjectType objType)
    {
        List<PlacementQuery> placementQueries = new List<PlacementQuery>();
        Vector3 halfDims = fullDims * .5f;

        for (int i = 0; i < prefabCount; i++)
        {
            var placementRules = new List<SpatialUnderstandingDllObjectPlacement.ObjectPlacementRule> { };
            var placementConstraints = new List<SpatialUnderstandingDllObjectPlacement.ObjectPlacementConstraint>();
            var placementDefinition = SpatialUnderstandingDllObjectPlacement.ObjectPlacementDefinition.Create_InMidAir(halfDims);

            if (objType == ObjectType.Tree)
            {
                placementRules = new List<SpatialUnderstandingDllObjectPlacement.ObjectPlacementRule> { };
                placementConstraints.Add(SpatialUnderstandingDllObjectPlacement.ObjectPlacementConstraint.Create_NearCenter());
                placementDefinition = SpatialUnderstandingDllObjectPlacement.ObjectPlacementDefinition.Create_OnFloor(halfDims);
            }

            else if (objType == ObjectType.Gate)
            {
                placementRules = new List<SpatialUnderstandingDllObjectPlacement.ObjectPlacementRule>
                {
                SpatialUnderstandingDllObjectPlacement.ObjectPlacementRule.Create_AwayFromOtherObjects(1.5f)
                };
                placementConstraints.Add(SpatialUnderstandingDllObjectPlacement.ObjectPlacementConstraint.Create_AwayFromWalls());
            }

            else if (objType == ObjectType.TurtorialMenu)
            {
                //placementConstraints.Add(SpatialUnderstandingDllObjectPlacement.ObjectPlacementConstraint.Create_NearWall());
                placementDefinition = SpatialUnderstandingDllObjectPlacement.ObjectPlacementDefinition.Create_OnWall(halfDims, 1.5f, 2.0f, 
                    SpatialUnderstandingDllObjectPlacement.ObjectPlacementDefinition.WallTypeFlags.Normal);
            }

            placementQueries.Add(
                new PlacementQuery(placementDefinition,
                    fullDims,
                    objType,
                    placementRules,
                    placementConstraints
                ));
        }
        return placementQueries;
    }
}