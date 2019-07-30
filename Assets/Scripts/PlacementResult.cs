using HoloToolkit.Unity;
using UnityEngine;

/*
 * PlacementResult class
 * This class contatins the reuslt from the solver of SpatialUnderstandingDllObjectPlacement library
 */

public class PlacementResult
{
    public Vector3 Position { get { return _result.Position; } }
    public Vector3 Normal { get { return _result.Forward; } }
    public Vector3 Dimensions { get; private set; }
    public ObjectType ObjType { get; private set; }
    private readonly SpatialUnderstandingDllObjectPlacement.ObjectPlacementResult _result;

    public PlacementResult(SpatialUnderstandingDllObjectPlacement.ObjectPlacementResult result, Vector3 dimensions, ObjectType objType)
    {
        _result = result;
        Dimensions = dimensions;
        ObjType = objType;
    }
}