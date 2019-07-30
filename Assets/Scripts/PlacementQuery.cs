using System.Collections.Generic;
using HoloToolkit.Unity;
using UnityEngine;

/*
 * PlacementQuery class
 * This class contatins the query-input for the solver of SpatialUnderstandingDllObjectPlacement library
 */

public enum ObjectType
{
    Tree,
    Box,
    Gate,
    Menu
}

public struct PlacementQuery
{
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

    public readonly SpatialUnderstandingDllObjectPlacement.ObjectPlacementDefinition PlacementDefinition;
    public readonly Vector3 Dimensions;
    public readonly ObjectType ObjType;
    public readonly List<SpatialUnderstandingDllObjectPlacement.ObjectPlacementRule> PlacementRules;
    public readonly List<SpatialUnderstandingDllObjectPlacement.ObjectPlacementConstraint> PlacementConstraints;
}