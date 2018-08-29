using System.Collections.Generic;
using HoloToolkit.Unity;
using UnityEngine;
//Done
public enum ObjectType
{
    Tree,
    Fruit,
    Box,
}

public struct PlacementQuery
{
    public readonly SpatialUnderstandingDllObjectPlacement.ObjectPlacementDefinition PlacementDefinition;
    public readonly Vector3 Dimensions;
    public readonly ObjectType ObjType;
    public readonly List<SpatialUnderstandingDllObjectPlacement.ObjectPlacementRule> PlacementRules;
    public readonly List<SpatialUnderstandingDllObjectPlacement.ObjectPlacementConstraint> PlacementConstraints;

    public PlacementQuery(
        SpatialUnderstandingDllObjectPlacement.ObjectPlacementDefinition placementDefinition,
        Vector3 dimensions,
        ObjectType objType,
        List<SpatialUnderstandingDllObjectPlacement.ObjectPlacementRule> placementRules = null,
        List<SpatialUnderstandingDllObjectPlacement.ObjectPlacementConstraint> placementConstraints = null)
    {
        PlacementDefinition = placementDefinition;
        PlacementRules = placementRules;
        PlacementConstraints = placementConstraints;
        Dimensions = dimensions;
        ObjType = objType;
    }
}