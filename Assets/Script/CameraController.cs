using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Camera mainCamera
    {
        get { return Camera.main; }
    }


    public float moveVelocity;
    public float angularVelocity;
    public float zoomVelocity;

    public float creatureMoveLerpVelocity;
    public float creatureCameraSize;
    public float creatureZoomVelocity;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //Creature creature = MouseManager.instance.creature;
        //if (creature == null)
        //{
        if (Input.GetKey(KeyCode.D)) transform.RotateAround(Vector3.zero, Vector3.back, angularVelocity * Time.deltaTime);
        if (Input.GetKey(KeyCode.A)) transform.RotateAround(Vector3.zero, -Vector3.back, angularVelocity * Time.deltaTime);
        if (Input.GetKey(KeyCode.W)) transform.position += transform.up * moveVelocity * Time.deltaTime;//transform.position.normalized * moveVelocity * Time.deltaTime;
        if (Input.GetKey(KeyCode.S)) transform.position -= transform.up * moveVelocity * Time.deltaTime;//transform.position.normalized * moveVelocity * Time.deltaTime;
        if (Vector3.Dot(transform.up, transform.position) <= 0) transform.position = new Vector3(0, 0, -10);
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        Camera.main.orthographicSize += scroll * zoomVelocity;
        /*
    }
    else
    {
        transform.position = Vector3.Lerp(transform.position, creature.transform.position, Time.deltaTime * creatureMoveLerpVelocity);
        mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, creatureCameraSize, creatureZoomVelocity) * Time.deltaTime;
    }
        */
    }
}
