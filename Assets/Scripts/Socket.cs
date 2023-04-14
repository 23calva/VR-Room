using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Written by Adam Calvelage | adamjasoncalvelage@gmail.com
/// </summary>
public class Socket : MonoBehaviour
{
    /// <summary>
    /// References for the controllers
    /// </summary>
    public static XRRayInteractor rightController, leftController;

    /// <summary>
    /// References for the game object and important components within this socket
    /// </summary>
    private SocketChild child = new SocketChild();

    /// <summary>
    /// Speed at which an object travels to be socketed
    /// </summary>
    public float lerpSpeed = 5.0f;

    /// <summary>
    /// Socket radius
    /// </summary>
    public float size = 0.5f;

    /// <summary>
    /// Maximum angle considered valid for an object to be socketed
    /// </summary>
    public float tolerance = 90.0f;

    private void Start()
    {
        // Assign controllers if they are not yet defined.
        rightController ??= GameObject.Find("LeftHand Controller").GetComponent<XRRayInteractor>();
        leftController ??= GameObject.Find("RightHand Controller").GetComponent<XRRayInteractor>();

        // Add the trigger component and set values
        var collider = this.gameObject.AddComponent<SphereCollider>();
        collider.hideFlags = HideFlags.HideInInspector;
        collider.isTrigger = true;
        collider.radius = size;
    }

    private void Update()
    {
        // Grabbing the child object from the socket would
        // not reset the color or unparent properly
        if(this.transform.childCount > 0)
        {
            this.transform.GetChild(0).GetComponent<MeshRenderer>().material.color = Color.white;
            this.transform.DetachChildren();
        }

        if (child.gameObject != null)
        {
            if (IsGrabbed(child.gameObject))
            {
                RemoveChild();
                return;
            }

            // Leap child to the center of the socket
            float distance = Vector3.Distance(child.gameObject.transform.position, this.transform.position);
            if (distance <= 0.001f)
            {
                child.gameObject.transform.position = this.transform.position;
            }
            else
            {
                child.gameObject.transform.position = Vector3.Lerp(child.gameObject.transform.position, this.transform.position, lerpSpeed * Time.deltaTime);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // Socket is occupied
        if(child.gameObject != null)
            return;

        // References
        var obj = other.gameObject;
        var renderer = obj.GetComponent<MeshRenderer>();

        // Modify colors based on object rotation
        if(IsValidAngle(obj))
        {
            renderer.material.color = Color.green;

            // Socket the object in this trigger
            if(IsGrabbed(obj) == false)
            {
                child.gameObject = obj;
                child.gameObject.transform.SetParent(this.transform);
                child.renderer = renderer;
                child.Color = Color.white;

                // Preserve horizontal rotation
                //child.gameObject.transform.rotation = Quaternion.Euler(0, child.gameObject.transform.rotation.eulerAngles.y, 0);
            }
        }
        else
        {
            renderer.material.color = Color.red;
        }
    }

#if UNITY_EDITOR

    // NOTE: This doesn't actually do anything in VR
    private void OnTriggerExit(Collider collider)
    {
        Vector3 point = collider.ClosestPoint(this.transform.position);
        float distance = Vector3.Distance(point, this.transform.position);

        if(collider.gameObject == child.gameObject && distance > size)
        {
            RemoveChild();
        }
    }

#endif

    /// <summary>
    /// Does the user have this object in their hands?
    /// </summary>
    private bool IsGrabbed(GameObject obj)
    {
        //
        // This code is a bit of a hack but probably
        // one of the easiest ways to get what we want
        //

        if(rightController.hasSelection)
        {
            if(rightController.interactablesSelected[0].transform.gameObject == obj)
            {
                return true;
            }
        }
        if(leftController.hasSelection)
        {
            if(leftController.interactablesSelected[0].transform.gameObject == obj)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Does this object align with this socket?
    /// </summary>
    public bool IsValidAngle(GameObject obj)
    {
        //
        // It would be incredibly easy to set tolerances for each axis
        //

        float angle = Quaternion.Angle(obj.transform.rotation, this.transform.rotation);
        angle = Math.Abs(angle);
        return angle <= tolerance;
    }

    /// <summary>
    /// Remove the child object
    /// </summary>
    private void RemoveChild()
    {
        child.gameObject = null;
        child.renderer = null;
    }

    private void OnDrawGizmos()
    {
        // Draw a sphere
        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(this.transform.position, size);

        // Draw an arrow
        Gizmos.color = Color.red;
        float length = 0.75f;
        float arms = length / 4;
        Vector3 end = (this.transform.forward * length) + this.transform.position;
        Gizmos.DrawLine(this.transform.position, end);
        Gizmos.DrawLine(end, (this.transform.right - this.transform.forward) * arms + end);
        Gizmos.DrawLine(end, (-this.transform.right - this.transform.forward) * arms + end);
    }
}

[Serializable]
internal class SocketChild
{
    public GameObject gameObject = null;
    public MeshRenderer renderer = null;

    public Color Color
    {
        get => renderer.material.color;
        set => renderer.material.color = value;
    }
}