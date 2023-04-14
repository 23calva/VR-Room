using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;

public class Player : MonoBehaviour
{
    private InputDevice rightController;

    private void Start()
    {
        var devices = new List<InputDevice>();
        InputDevices.GetDevices(devices);
       rightController = devices.First(x => x.characteristics == InputDeviceCharacteristics.Right);
    }

    private void Update()
    {
        rightController.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 wishDir);
        this.transform.position += (Vector3)(Time.deltaTime * wishDir);
    }
}
