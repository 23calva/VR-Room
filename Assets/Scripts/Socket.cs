using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Written by Adam Calvelage | adamjasoncalvelage@gmail.com
/// </summary>
public class Socket : MonoBehaviour
{
    private static XRRayInteractor leftController;
    private static XRRayInteractor rightController;

    /// <summary>
    /// References to the GameObject and Rigidbody Component within this socket.
    /// </summary>
    private GameObject obj;

    /// <summary>
    /// Collider radius.
    /// </summary>
    public float size = 0.5f;

    /// <summary>
    /// Speed at which the socketed entity travels.
    /// </summary>
    public float lerpSpeed = 5.0f;

    /// <summary>
    /// Tolerance.
    /// </summary>
    public float MaxAngle = 90.0f;

    private void Start()
    {
        // Assign controllers if they are not yet defined.
        leftController ??= GameObject.Find("LeftHand Controller").GetComponent<XRRayInteractor>();
        rightController ??= GameObject.Find("RightHand Controller").GetComponent<XRRayInteractor>();

        var collider = this.gameObject.AddComponent<SphereCollider>();
        collider.isTrigger = true;
        collider.radius = size;
    }

    private void Update()
    {
        print(leftController.attachTransform.name);

        if (leftController.attachTransform.gameObject == obj || 
            rightController.attachTransform.gameObject == obj)
        {
            return;
        }

        if (obj != null)
        {
            // Lerp til you can't no more.
            float distance = Vector3.Distance(obj.transform.position, this.transform.position);
            if (distance <= 0.001f)
            {
                obj.transform.position = this.transform.position;
            }
            else
            {
                obj.transform.position = Vector3.Lerp(obj.transform.position, this.transform.position, lerpSpeed * Time.deltaTime);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        var entity = other.gameObject;

        // Socket is occupied.
        if (obj != null)
        {
            return;
        }

        // Modify render color.
        var renderer = entity.GetComponent<MeshRenderer>();
        if (IsValidAngle(entity))
        {
            renderer.material.color = Color.green;
        }
        else
        {
            renderer.material.color = Color.red;
            return;
        }

        InsertObject(entity);
    }

    /// <summary>
    /// Remove the socketed object when it has exited this trigger.
    /// </summary>
    private void OnTriggerExit(Collider collider)
    {
        Vector3 point = collider.ClosestPoint(this.transform.position);
        float distance = Vector3.Distance(point, this.transform.position);

        if (collider.gameObject == obj && distance > size)
        {
            RemoveObject();
        }
    }

    /// <summary>
    /// Does this object align with this socket?
    /// </summary>
    public bool IsValidAngle(GameObject socketedObject)
    {
        float angle = Quaternion.Angle(socketedObject.transform.rotation, this.transform.rotation);
        angle = Math.Abs(angle);
        return angle <= MaxAngle;
    }

    /// <summary>
    /// Manage this object with this function.
    /// </summary>
    private void InsertObject(GameObject gameObject)
    {
        obj = gameObject;
        obj.transform.SetParent(this.transform);
    }

    private void RemoveObject()
    {
        //Vector3 parentPos = obj.transform.localPosition + this.transform.position;
        //Quaternion parentRot = obj.transform.localRotation * this.transform.rotation;

        obj.GetComponent<MeshRenderer>().material.color = Color.white;

        this.transform.DetachChildren();

        //obj.transform.position = parentPos;
        //obj.transform.rotation = parentRot;

        obj = null;
    }

    private void OnDrawGizmos()
    {
        // Draw a sphere.
        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(this.transform.position, size);

        // Draw an arrow.
        Gizmos.color = Color.red;
        float length = 0.75f;
        float arms = length / 4;
        Vector3 end = (this.transform.forward * length) + this.transform.position;
        Gizmos.DrawLine(this.transform.position, end);
        Gizmos.DrawLine(end, (this.transform.right - this.transform.forward) * arms + end);
        Gizmos.DrawLine(end, (-this.transform.right - this.transform.forward) * arms + end);
    }
}