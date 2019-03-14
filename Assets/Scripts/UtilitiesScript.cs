using HoloToolkit.Unity;
using System.Collections.Generic;
using UnityEngine;

public class UtilitiesScript : Singleton<UtilitiesScript>
{
    public void EnableOutline(GameObject focusedObject, object color, bool createIfNeeded)
    {
        if (focusedObject == null)
            return;

        if (color == null)
            color = Color.red;

        var outline = focusedObject.GetComponent<Outline>();
        if (outline == null && createIfNeeded)
        {
            //Create Outline
            outline = gameObject.AddComponent<Outline>();
            if (outline != null)
            {
                outline.OutlineMode = Outline.Mode.OutlineAll;
                outline.OutlineWidth = 5f;
                outline.OutlineColor = (Color) color;
                outline.enabled = true;
            }
        }
        else if (outline != null)
        {
            outline.OutlineColor = (Color) color;
            outline.enabled = true;
        }
    }

    public void EnableGravity(GameObject obj, bool forceEnable)
    {
        if (obj == null)
            return;

        if (obj.GetComponent<Rigidbody>() == null && forceEnable)
        {
            obj.AddComponent<Rigidbody>();
            obj.GetComponent<Rigidbody>().useGravity = true;
        }
        else if (obj.GetComponent<Rigidbody>() != null)
            obj.GetComponent<Rigidbody>().useGravity = true;
    }

    public float GetDistanceObjects(Transform obj1, Transform obj2)
    {
        if (obj1 == null || obj2 == null)
            return -1 ;

        return Vector3.Magnitude(obj1.position - obj2.position);
    }

    public bool IsRightFromHead(Vector3 pos)
    {
        Vector3 heading = pos - Camera.main.transform.position;
        Vector3 perp = Vector3.Cross(Camera.main.transform.forward, heading);
        float dot = Vector3.Dot(perp, Camera.main.transform.up);
        if (dot >= 0)
            return true;
        else
            return false;
    }

    public void ChangeColorOutline(GameObject focusedObject, Color color)
    {
        if (focusedObject != null)
        {
            var outline = focusedObject.GetComponent<Outline>();
            if (outline != null)
            {
                if (outline.enabled == false) outline.enabled = true;
                outline.OutlineColor = color;
            }
        }
    }

    public void DisableOutline(GameObject focusedObject)
    {
        if (focusedObject != null)
        {
            Outline outline = focusedObject.GetComponent<Outline>();
            if (outline != null)
                outline.enabled = false;
        }
    }

    public void ChangeObjectColor(GameObject obj, Color color)
    {
        if (obj == null)
            return;

        obj.GetComponent<Renderer>().material.color = color;
    }

    public void ChangeObjectColor(Dictionary<uint, HandStruct> dictionary, Color color)
    {
        foreach (KeyValuePair<uint, HandStruct> entry in dictionary)
        {
            ChangeObjectColor(entry.Value.hand, color);
        }
    }

    public void EnableObject(GameObject obj)
    {
        if (obj != null)
            obj.SetActive(true);
    }

    public void DisableObject(GameObject obj)
    {
        if (obj != null)
            obj.SetActive(false);
    }

    public void PlaceInFrontOfUser(GameObject obj, float distance)
    {
        Vector3 pos = Camera.main.transform.position + Camera.main.transform.forward * distance;
        obj.transform.position = pos;
        //Fix menu direction
        Vector3 directionToTarget = Camera.main.transform.position - pos;
        directionToTarget.y = 0.0f;
        if (directionToTarget.sqrMagnitude > 0.005f)
            obj.transform.rotation = Quaternion.LookRotation(-directionToTarget);
    }
}
